import win32evtlogutil, win32evtlog, traceback, os, winreg
from enum import Enum, auto

class LogLevel(Enum):
    INFO = auto()
    WARNING = auto()
    ERROR = auto()

class EventLogger:
    EVENT_TYPES = {
        LogLevel.INFO: win32evtlog.EVENTLOG_INFORMATION_TYPE,
        LogLevel.WARNING: win32evtlog.EVENTLOG_WARNING_TYPE,
        LogLevel.ERROR: win32evtlog.EVENTLOG_ERROR_TYPE,
    }

    def __init__(self, source="MyService", event_id=1000):
        self.source = source
        self.event_id = event_id
        self._ensure_event_source()

    def _ensure_event_source(self):
        key_path = fr"SYSTEM\CurrentControlSet\Services\EventLog\Application\{self.source}"
        try:
            with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, key_path, 0, winreg.KEY_READ):
                return
        except FileNotFoundError:
            pass

        dll_path = os.path.join(os.environ["SystemRoot"], "System32", "eventcreate.exe")
        with winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, key_path) as key:
            winreg.SetValueEx(key, "EventMessageFile", 0, winreg.REG_EXPAND_SZ, dll_path)
            winreg.SetValueEx(key, "TypesSupported", 0, winreg.REG_DWORD, 7)

    def _log(self, level: LogLevel, message):
        etype = self.EVENT_TYPES[level]
        if isinstance(message, Exception):
            msg = "".join(traceback.format_exception(type(message), message, message.__traceback__))
        else:
            msg = str(message)

        win32evtlogutil.ReportEvent(
            self.source,
            self.event_id,
            0,
            eventType=etype,
            strings=[msg],
            data=b""
        )

    def info(self, message):
        self._log(LogLevel.INFO, message)

    def warning(self, message):
        self._log(LogLevel.WARNING, message)

    def error(self, message):
        self._log(LogLevel.ERROR, message)
