
from __future__ import annotations

from dataclasses import dataclass
from datetime import datetime, timedelta
from typing import Dict, List, Optional, Tuple, Any


@dataclass
class SessionState:
    # identity
    visitor_id: int
    host: str

    # timing
    started_utc: datetime
    ended_utc: datetime

    # counters / navigation
    request_count: int = 0
    entry_page: Optional[str] = None
    exit_page: Optional[str] = None
    referrer_class: Optional[str] = None

    # db identity (filled when you upsert + get_id)
    session_id: Optional[int] = None

    def to_row(self, table_has_session_id: bool = False) -> dict:
        row = {
            "VisitorId": self.visitor_id,
            "Host": self.host[:32] if self.host else None,
            "StartedUtc": self.started_utc,
            "EndedUtc": self.ended_utc,
            "RequestCount": self.request_count,
            # "EntryPage": self.entry_page,
            # "ExitPage": self.exit_page,
            "ReferrerClass": self.referrer_class[:32] if self.referrer_class else None,
        }
        if table_has_session_id and self.session_id is not None:
            row["SessionId"] = self.session_id
        return row


class SessionWalker:
    """
    Sessionization:
      Key: (visitor_id, host)  [host = W3SVC1 folder name]
      Rule: new session if inactivity gap > timeout

    Works on interleaved traffic because it tracks per-key state.
    Assumes input is *roughly* chronological overall (IIS logs are).
    """

    def __init__(self, timeout_minutes: int = 30):
        self.timeout = timedelta(minutes=timeout_minutes)
        self._active: Dict[Tuple[int, str], SessionState] = {}

    def observe(
        self,
        visitor_id: int,
        host: str,
        ts_utc: datetime,
        url_path: Optional[str] = None,
        referrer_class: Optional[str] = None,
    ) -> List[Tuple[str, SessionState]]:
        """
        Returns a list of events:
          - ("CLOSE", state)  previous session ended due to timeout
          - ("OPEN",  state)  new session started
          - ("HIT",   state)  session updated with this request
        """
        events: List[Tuple[str, SessionState]] = []
        key = (visitor_id, host)

        st = self._active.get(key)

        if st is None:
            # start first session
            st = SessionState(
                visitor_id=visitor_id,
                host=host,
                started_utc=ts_utc,
                ended_utc=ts_utc,
                request_count=0,
                entry_page=url_path,
                exit_page=url_path,
                referrer_class=referrer_class,
            )
            self._active[key] = st
            events.append(("OPEN", st))

        else:
            gap = ts_utc - st.ended_utc
            if gap > self.timeout:
                # close old
                events.append(("CLOSE", st))

                # new session
                st = SessionState(
                    visitor_id=visitor_id,
                    host=host,
                    started_utc=ts_utc,
                    ended_utc=ts_utc,
                    request_count=0,
                    entry_page=url_path,
                    exit_page=url_path,
                    referrer_class=referrer_class,
                )
                self._active[key] = st
                events.append(("OPEN", st))

        # apply hit to current session
        st.ended_utc = ts_utc
        st.request_count += 1
        if st.entry_page is None:
            st.entry_page = url_path
        st.exit_page = url_path or st.exit_page
        if st.referrer_class is None and referrer_class:
            st.referrer_class = referrer_class

        events.append(("HIT", st))
        return events

    def flush_all(self) -> List[Tuple[str, SessionState]]:
        """
        Close everything still open at end of processing.
        """
        events: List[Tuple[str, SessionState]] = []
        for st in list(self._active.values()):
            events.append(("CLOSE", st))
        self._active.clear()
        return events
