import os
from datetime import datetime

from sqlalchemy import create_engine
from _config import ALCHEMY_CONN_STR
import utils.sql as sql
from services.traffic_classifier import *
from services.geo_locate import get_ip_info

VISITOR_TABLE = "IisVisitors"
REQUEST_TABLE = "IisRequests"
BATCH_SIZE = 5000


# -------------------------------------------------
# MAIN
# -------------------------------------------------

def process_logs( path: str):

    db = create_engine(ALCHEMY_CONN_STR)
    with db.connect().execution_options(isolation_level="AUTOCOMMIT") as conn:
        today_suffix = datetime.today().strftime("%y%m%d")
        host = os.path.basename(path)
        if len(host) > 16:
            raise ValueError(f"Host name too long ({len(host)}): {host}")

        # --- collect log files to process ---
        log_files = []
        for root, _, files in os.walk(path):
            for file in files:
                if file.endswith(".log") and today_suffix not in file:
                    log_files.append(os.path.join(root, file))

        # --- load known IPs from DB ---
        known_ips = {r["IpAddress"] for r in sql.query("SELECT IpAddress FROM IisVisitors")}

        request_batch = []

        # --- single pass ---
        for log_file in log_files:
            for row in iterate_iis_rows(log_file):
                ip = row.get("c-ip")
                if not ip:
                    continue

                # new IP: classify, geo, insert visitor
                if ip not in known_ips:
                    ua = row.get("cs(User-Agent)", "")
                    geo = get_ip_info(ip)

                    visitor_row = {
                        "IpAddress": ip,
                        "UserAgent": user_agent_to_label(ua),
                        "DeviceType": device_type_from_ua(ua),
                        "IsBot": 1 if classify_human_vs_bot(ua) else 0,
                        "Country": geo.get("country"),
                        "City": geo.get("city"),
                        "Region": geo.get("subdivisions"),
                        "Latitude": geo.get("latitude"),
                        "Longitude": geo.get("longitude"),
                    }

                    sql.upsert_item(
                        conn,
                        visitor_row,
                        VISITOR_TABLE,
                        updatewhere_cols=["IpAddress"],
                        doInsert=True
                    )
                    known_ips.add(ip)

                ts = parse_timestamp(row)
                if not ts:
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

                request_batch.append({
                    "IpAddress": ip,
                    "Host": host[:32],
                    "RequestTimeUtc": ts,
                    "Method": method[:7] if method else None,
                    "UrlPath": url[:128],
                    "QueryString": None if query == "-" else query[:256] if query else None,
                    "StatusCode": status,
                    "TimeTakenMs": int(time_taken) if time_taken and time_taken != "-" else None,
                    "ReferrerClass": ref_class[:32] if ref_class else None
                })

                if len(request_batch) >= BATCH_SIZE:
                    sql.bulk_insert(request_batch, REQUEST_TABLE)
                    request_batch.clear()

        # flush remaining
        sql.bulk_insert(request_batch, REQUEST_TABLE)

        # cleanup: delete processed .log files
        for log_file in log_files:
            os.remove(log_file)



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
