import win32security, socket, xml.etree.ElementTree as ET
from typing import List, Dict, Any, Optional
import sys, time, subprocess
from datetime import timedelta
from utils.AlcRepo import upsert_data
from utils.WinLog import LogLevel, log_event
from utils.intervalDecorator import setInterval
from utils.Sql import get_scalar

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

def get_last_time(logtype: str) -> int:
    sql = (
        "SELECT MAX(event_record_id) "
        "FROM dbo.WindowsEvent "
        "WHERE host = '{0}' AND log_name = '{1}'"
    ).format(socket.gethostname(), logtype)

    return get_scalar(sql) or 0

def resolve_sid(sid_str: str) -> str:
    try:
        sid = win32security.ConvertStringSidToSid(sid_str)
        name, domain, _ = win32security.LookupAccountSid(None, sid)
        return f"{domain}\\{name}" if domain else name
    except Exception:
        return sid_str  # fallback to raw SID
    

def fetch_events( max_event_record_id:int = 0, logtype: str="Application", max_events:int=100):
    if max_event_record_id > 0:
        cmd = [
            "wevtutil", "qe", logtype,
            f"/c:{max_events}", "/f:RenderedXml", "/rd:true",
            f"/q:*[System[(EventRecordID>{max_event_record_id})]]"
        ]
    else:
        cmd = ["wevtutil", "qe", logtype, f"/c:{max_events}", "/f:RenderedXml", "/rd:true"]    

    xml_data = subprocess.check_output(cmd, text=True, errors="ignore")

    events = []
    for raw_xml in xml_data.strip().split("</Event>"):
        if not raw_xml.strip():
            continue
        raw_xml = raw_xml + "</Event>"
        try:
            root = ET.fromstring(raw_xml)
        except ET.ParseError:
            continue

        ns = {"ev": "http://schemas.microsoft.com/win/2004/08/events/event"}
        sys = root.find("ev:System", ns)
        rend = root.find("ev:RenderingInfo", ns)

        provider = sys.find("ev:Provider", ns).attrib.get("Name", None) if sys is not None else None
        event_id = sys.find("ev:EventID", ns).text if sys is not None else None
        task = rend.find("ev:Task", ns).text if rend is not None and rend.find("ev:Task", ns) is not None else None
        level = rend.find("ev:Level", ns).text if rend is not None and rend.find("ev:Level", ns) is not None else None
        time_created = sys.find("ev:TimeCreated", ns).attrib.get("SystemTime", None) if sys is not None else None
        user = sys.find("ev:Security", ns).attrib.get("UserID", None) if sys is not None and sys.find("ev:Security", ns) is not None else None

        message = rend.find("ev:Message", ns).text if rend is not None and rend.find("ev:Message", ns) is not None else None
        record_id = sys.find("ev:EventRecordID", ns).text if sys is not None and sys.find("ev:EventRecordID", ns) is not None else None

        winuser = resolve_sid(user)
        events.append({
            "host": socket.gethostname(),
            "log_name": logtype,
            "source": provider,
            "event_id": event_id,
            "event_record_id": record_id, 
            "category": task,
            "severity": level,
            "time_created": time_created,
            "winuser": winuser,
            "message": message
        })

    return events


def store_events(max_events: int = 10000):
    logs_to_fetch = ["Application", "System"]  # add "Security" if allowed
    all_data: List[Dict[str, Any]] = []
    for log in logs_to_fetch:
        max_record_id = get_last_time(log)
        all_data.extend(fetch_events( max_record_id, log, max_events=max_events))

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
