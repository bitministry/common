import os
from datetime import datetime
from collections import defaultdict, Counter

from sqlalchemy import create_engine
from _config import ALCHEMY_CONN_STR
import utils.sql as sql 
from services.traffic_classifier import *
from services.geo_locate import get_ip_info
from services.session_walker import SessionWalker

VISITOR_TABLE = "IisVisitors"
REQUEST_TABLE = "IisRequests"
SESSION_TABLE = "IisSessions"



# -------------------------------------------------
# MAIN
# -------------------------------------------------

def process_logs( path: str):

    db = create_engine(ALCHEMY_CONN_STR)
    with db.connect().execution_options(isolation_level="AUTOCOMMIT") as conn:
        # --- single pass: aggregate + buffer rows ---
        ip_aggregate = defaultdict(lambda: {
            "urls": Counter(),
            "status": Counter(),
            "first_seen": None,
            "last_seen": None,
            "ua": None
        })
        buffered_rows = []  # (host, row_dict, ts)

        host = os.path.basename(path)
        for root, _, files in os.walk(path):
            for file in files:
                if not file.endswith(".log"):
                    continue

                for row in iterate_iis_rows(os.path.join(root, file)):
                    ip = row.get("c-ip")
                    if not ip:
                        continue

                    ts = parse_timestamp(row)
                    if not ts:
                        continue

                    ua = row.get("cs(User-Agent)", "")
                    url = row.get("cs-uri-stem", "")
                    status = int(row.get("sc-status", 0))

                    agg = ip_aggregate[ip]
                    agg["ua"] = ua
                    agg["urls"][url] += 1
                    agg["status"][status] += 1

                    if not agg["first_seen"] or ts < agg["first_seen"]:
                        agg["first_seen"] = ts
                    if not agg["last_seen"] or ts > agg["last_seen"]:
                        agg["last_seen"] = ts

                    buffered_rows.append((host, row, ts))

        # --- visitor upsert phase ---
        visitor_id_cache = {}

        for ip, data in ip_aggregate.items():

            result = classify_human_vs_bot(
                ua=data["ua"],
                urls_counter=data["urls"],
                status_counter=data["status"],
                first_seen=data["first_seen"],
                last_seen=data["last_seen"]
            )

            geo = get_ip_info(ip)

            visitor_row = {
                "IpAddress": ip,
                "UserAgent": user_agent_to_label(data["ua"]),
                "DeviceType": device_type_from_ua(data["ua"]),
                "IsBot": 1 if result["is_bot"] else 0,
                "Country": geo.get("country"),
                "City": geo.get("city"),
                "Region": geo.get("subdivisions"),
                "Latitude": geo.get("latitude"),
                "Longitude": geo.get("longitude"),
                "FirstSeenUtc": data["first_seen"],
                "LastSeenUtc": data["last_seen"]
            }

            sql.upsert_item(
                conn,
                visitor_row,
                VISITOR_TABLE,
                updatewhere_cols=["IpAddress"],
                doInsert=True
            )

            visitor_id = sql.get_id(
                VISITOR_TABLE,
                where_cols=["IpAddress"],
                data_dict=visitor_row,
                id_col="VisitorId"
            )

            visitor_id_cache[ip] = visitor_id

        # --- request insert + sessionization phase (from buffer) ---
        walker = SessionWalker(timeout_minutes=30)

        for host, row, ts in buffered_rows:
            ip = row.get("c-ip")
            visitor_id = visitor_id_cache.get(ip)
            if not visitor_id:
                continue

            url = row.get("cs-uri-stem", "")
            if is_asset_path(url):
                continue

            query = row.get("cs-uri-query")
            method = row.get("cs-method")
            status = int(row.get("sc-status", 0))
            time_taken = row.get("time-taken")
            ref = row.get("cs(Referer)")
            ref_class = normalize_referrer(ref)

            # session tracking
            events = walker.observe(
                visitor_id=visitor_id,
                host=host,
                ts_utc=ts,
                url_path=url[:128],
                referrer_class=ref_class,
            )
            for ev, st in events:
                if ev in ("OPEN", "CLOSE"):
                    session_row = st.to_row()
                    sql.upsert_item(
                        conn,
                        session_row,
                        SESSION_TABLE,
                        updatewhere_cols=["VisitorId", "Host", "StartedUtc"],
                        doInsert=True
                    )
                    if st.session_id is None:
                        st.session_id = sql.get_id(
                            SESSION_TABLE,
                            where_cols=["VisitorId", "Host", "StartedUtc"],
                            data_dict=session_row,
                            id_col="SessionId"
                        )

            session_id = events[-1][1].session_id

            request_row = {
                "SessionId": session_id,
                "VisitorId": visitor_id,
                "RequestTimeUtc": ts,
                "Method": method[:7] if method else None,
                "UrlPath": url[:128],
                "QueryString": None if query == "-" else query[:256] if query else None,
                "StatusCode": status,
                "TimeTakenMs": int(time_taken) if time_taken and time_taken != "-" else None
            }

            sql.upsert_item(
                conn,
                request_row,
                REQUEST_TABLE,
                updatewhere_cols=[],
                doInsert=True
            )

        # flush remaining open sessions
        for ev, st in walker.flush_all():
            session_row = st.to_row()
            sql.upsert_item(
                conn,
                session_row,
                SESSION_TABLE,
                updatewhere_cols=["VisitorId", "Host", "StartedUtc"],
                doInsert=True
            )

        # cleanup: delete processed .log files (exclude today's)
        today_suffix = datetime.today().strftime("%y%m%d")  # e.g. "260215"
        for root, _, files in os.walk(path):
            for file in files:
                if not file.endswith(".log"):
                    continue
                if today_suffix in file:
                    continue
                os.remove(os.path.join(root, file))



# -------------------------------------------------
# IIS PARSER
# -------------------------------------------------

def iterate_iis_rows(path):
    fields = None

    with open(path, "r", encoding="utf-8", errors="ignore") as f:
        for line in f:
            line = line.strip()
            if not line:
                continue

            if line.startswith("#Fields:"):
                fields = line.split()[1:]
                continue

            if line.startswith("#") or not fields:
                continue

            parts = line.split()
            if len(parts) != len(fields):
                continue

            yield dict(zip(fields, parts))


def parse_timestamp(row):
    try:
        return datetime.strptime(
            f"{row.get('date')} {row.get('time')}",
            "%Y-%m-%d %H:%M:%S"
        )
    except:
        return None
