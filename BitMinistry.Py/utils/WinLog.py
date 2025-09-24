import win32evtlogutil, win32evtlog, traceback, os, winreg
from enum import Enum, auto

class LogLevel(Enum):
    INFO = auto()
    WARNING = auto()
    ERROR = auto()

EVENT_TYPES = {
    LogLevel.INFO: win32evtlog.EVENTLOG_INFORMATION_TYPE,
    LogLevel.WARNING: win32evtlog.EVENTLOG_WARNING_TYPE,
    LogLevel.ERROR: win32evtlog.EVENTLOG_ERROR_TYPE,
}

def ensure_event_source(source: str):
    key_path = fr"SYSTEM\CurrentControlSet\Services\EventLog\Application\{source}"
    try:
        with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, key_path, 0, winreg.KEY_READ):
            return  # already exists
    except FileNotFoundError:
        pass

    dll_path = os.path.join(os.environ["SystemRoot"], "System32", "eventcreate.exe")
    with winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, key_path) as key:
        winreg.SetValueEx(key, "EventMessageFile", 0, winreg.REG_EXPAND_SZ, dll_path)
        winreg.SetValueEx(key, "TypesSupported", 0, winreg.REG_DWORD, 7)

def log_event(level: LogLevel, message, source="MyService", event_id=1000):
    ensure_event_source(source)
    etype = EVENT_TYPES[level]
    if isinstance(message, Exception):
        msg = "".join(traceback.format_exception(type(message), message, message.__traceback__))
    else:
        msg = str(message)

    win32evtlogutil.ReportEvent(
        source,
        event_id,
        0,
        eventType=etype,
        strings=[msg],
        data=b""
    )
