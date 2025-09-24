import datetime
import threading
from typing import Callable, Optional, TypeVar, ParamSpec, Sequence

P = ParamSpec("P")
R = TypeVar("R")

def setInterval(
    intervalSeconds: float,
    firstRunTime: Optional[datetime.time] = None,
    firstRunAtCurrentHourMinutes: Optional[Sequence[int]] = None,
) -> Callable[[Callable[P, R]], Callable[P, threading.Event]]:
    def decorator(function: Callable[P, R]) -> Callable[P, threading.Event]:
        def wrapper(*args: P.args, **kwargs: P.kwargs) -> threading.Event:
            stopped = threading.Event()
            now = datetime.datetime.now()

            # --- Initial delay logic ---
            if firstRunAtCurrentHourMinutes:
                future_minutes = [m for m in sorted(firstRunAtCurrentHourMinutes) if m > now.minute]
                if future_minutes:  
                    minute = future_minutes[0]
                    run_dt = now.replace(minute=minute, second=0, microsecond=0)
                else:  
                    # no valid minute left this hour → take the first in next hour
                    minute = min(firstRunAtCurrentHourMinutes)
                    run_dt = (now + datetime.timedelta(hours=1)).replace(
                        minute=minute, second=0, microsecond=0
                    )
                initial_delay = (run_dt - now).total_seconds()

            elif firstRunTime is not None:
                run_dt = datetime.datetime.combine(datetime.date.today(), firstRunTime)
                if now >= run_dt:
                    run_dt += datetime.timedelta(days=1)
                initial_delay = (run_dt - now).total_seconds()

            else:
                initial_delay = 0
            # ---------------------------

            print(f"{function} initial delay {initial_delay:.1f}s")

            def loop() -> None:
                if not stopped.wait(initial_delay):
                    function(*args, **kwargs)
                while not stopped.wait(intervalSeconds):
                    function(*args, **kwargs)

            t = threading.Thread(target=loop, daemon=True)
            t.start()
            return stopped
        return wrapper
    return decorator
