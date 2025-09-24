import win32evtlog, win32security, socket, xml.etree.ElementTree as ET
from typing import List, Dict, Any, Optional
import sys, time
from datetime import timedelta
from utils.AlcRepo import upsert_data
from utils.WinLog import LogLevel, log_event
from utils.intervalDecorator import setInterval
from utils.Sql import get_data

LOG_SRC = "Bm_WindowsEventsToSql"

LEVEL_XML_MAP = {
    "1": "Critical",
    "2": "Error",
    "3": "Warning",
    "4": "Information",
    "5": "Verbose",
}

def xprint( msg: str): 
    log_event(LogLevel.INFO, msg, source=LOG_SRC)    

def get_last_time(logtype: str ) -> Optional[str]:
    sql = (
        "SELECT MAX(time_created) AS last_time "
        "FROM dbo.WindowsEvent "
        "WHERE host = '{0}' AND log_name = '{1}'").format(socket.gethostname(), logtype)
        
    res = get_data(sql)
    if not res or res[0]["last_time"] is None:
        return None
    last_time = res[0]["last_time"]

    # subtract 2 seconds 
    safe_time = last_time - timedelta(seconds=2)

    # must end with 'Z' for EvtQuery
    return safe_time.strftime("%Y-%m-%dT%H:%M:%S.000Z")

def resolve_sid(sid_str: str) -> str:
    try:
        sid = win32security.ConvertStringSidToSid(sid_str)
        name, domain, _ = win32security.LookupAccountSid(None, sid)
        return f"{domain}\\{name}" if domain else name
    except Exception:
        return sid_str  # fallback to raw SID

def fetch_events(logtype="Application", last_time=None, max_events=100):
    out, count = [], 0

    if last_time:
        query = f"*[System[TimeCreated[@SystemTime>'{last_time}']]]"
    else:
        query = "*"

    xprint( query )

    h = win32evtlog.EvtQuery(logtype, win32evtlog.EvtQueryReverseDirection, query)

    while True:
        events = win32evtlog.EvtNext(h, 10)
        if not events:
            break
        for ev in events:
            xml = win32evtlog.EvtRender(ev, win32evtlog.EvtRenderEventXml)
            root = ET.fromstring(xml)
            ns = "{http://schemas.microsoft.com/win/2004/08/events/event}"
            system = root.find(f"{ns}System")

            provider = system.find(f"{ns}Provider").attrib.get("Name")
            event_id = int(system.find(f"{ns}EventID").text)
            level_val = system.find(f"{ns}Level").text if system.find(f"{ns}Level") is not None else None
            level = LEVEL_XML_MAP.get(level_val, "Unknown")
            task = system.find(f"{ns}Task").text if system.find(f"{ns}Task") is not None else None
            time_created = system.find(f"{ns}TimeCreated").attrib.get("SystemTime")

            raw_user = system.find(f"{ns}Security").attrib.get("UserID") if system.find(f"{ns}Security") is not None else None
            user = resolve_sid(raw_user) if raw_user else None

            eventdata = root.find(f"{ns}EventData")
            message = " | ".join(d.text for d in eventdata.findall(f"{ns}Data") if d.text) if eventdata is not None else None

            out.append({
                "host": socket.gethostname(),
                "log_name": logtype,
                "source": provider,
                "event_id": event_id,
                "category": task,
                "severity": level,
                "time_created": time_created,
                "winuser": user,
                "message": message,
            })

            count += 1
            if max_events and count >= max_events:
                return out
    return out

def store_events(max_events: int = 10000):
    logs_to_fetch = ["Application", "System"]  # add "Security" if allowed
    all_data: List[Dict[str, Any]] = []
    for log in logs_to_fetch:
        last_time = get_last_time(log)
        all_data.extend(fetch_events(log, last_time, max_events=max_events))

    xprint( "event count for upsert: " + str(len(all_data)))

    if all_data:
        upsert_data(
            data=all_data,
            table_name="dbo.WindowsEvent",
            id_cols=["host", "log_name", "source", "event_id", "time_created"],
            doInsert=True
        )

if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1].isdigit():
        interval = int(sys.argv[1])
        xprint(f"Scheduling event export every {interval} seconds...")

        @setInterval(intervalSeconds=interval)
        def scheduleExport():
            try:
                store_events()
            except Exception as e:
                log_event(LogLevel.ERROR, e, source=LOG_SRC)
                raise

        stop_event = scheduleExport()

        try:
            while True:
                time.sleep(1)
        except KeyboardInterrupt:
            xprint("Stopping schedule...")
            stop_event.set()
    else:
        store_events()
