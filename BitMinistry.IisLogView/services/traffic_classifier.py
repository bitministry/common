import re
from collections import Counter


# ============================================================
# SIGNATURE LISTS
# ============================================================

BOT_UA_HINTS = [
    "bot", "spider", "crawl", "crawler", "slurp", "archiver",
    "python-requests", "curl", "wget", "httpclient", "scrapy",
    "ahrefs", "semrush", "mj12bot", "dotbot",
    "bingpreview", "facebookexternalhit", "discordbot",
    "telegrambot", "uptime", "monitor", "check",
    "zgrab", "go-http-client", "libwww",
    "headless", "phantomjs", "selenium", "playwright"
]

BOT_REGEX = re.compile("|".join(BOT_UA_HINTS), re.I)

MOBILE_UA_HINTS = ["mobile", "android", "iphone", "ipod"]
TABLET_UA_HINTS = ["ipad", "tablet"]
DESKTOP_UA_HINTS = ["windows nt", "macintosh", "x11"]

ASSET_EXTENSIONS = (
    ".css", ".js", ".png", ".jpg", ".jpeg", ".gif",
    ".svg", ".ico", ".webp", ".woff", ".woff2", ".ttf"
)


# ============================================================
# DEVICE DETECTION
# ============================================================

def device_type_from_ua(ua: str) -> str:
    if not ua:
        return "Unknown"

    u = ua.lower()

    if any(x in u for x in MOBILE_UA_HINTS):
        return "Mobile"
    if any(x in u for x in TABLET_UA_HINTS):
        return "Tablet"
    if any(x in u for x in DESKTOP_UA_HINTS):
        return "Desktop"

    return "Desktop"


# ============================================================
# USER AGENT LABELING
# ============================================================

def user_agent_to_label(ua: str) -> str:
    if not ua or not ua.strip():
        return "Unknown"

    u = ua.lower()

    # Major bots
    if "googlebot" in u:
        return "Googlebot"
    if "bingbot" in u or "msnbot" in u:
        return "Bingbot"
    if "slurp" in u:
        return "Yahoo Slurp"
    if "duckduckbot" in u:
        return "DuckDuckBot"
    if "facebookexternalhit" in u:
        return "Facebook Bot"
    if "twitterbot" in u:
        return "Twitter Bot"
    if "linkedinbot" in u:
        return "LinkedIn Bot"
    if "slackbot" in u:
        return "Slack Bot"
    if "telegrambot" in u:
        return "Telegram Bot"
    if "discordbot" in u:
        return "Discord Bot"
    if "ahrefs" in u:
        return "Ahrefs Bot"
    if "semrush" in u:
        return "Semrush Bot"
    if "mj12bot" in u or "dotbot" in u:
        return "Majestic Bot"

    if any(h in u for h in ("curl", "wget", "python-requests", "scrapy")):
        return "Script/API"

    device = device_type_from_ua(ua)

    if "edg/" in u or "edge/" in u:
        return f"Edge ({device})"
    if "opr/" in u or "opera" in u:
        return f"Opera ({device})"
    if "chrome" in u and "edg" not in u and "opr" not in u:
        return f"Chrome ({device})"
    if "firefox" in u:
        return f"Firefox ({device})"
    if "safari" in u and "chrome" not in u:
        return f"Safari ({device})"
    if "msie" in u or "trident" in u:
        return f"IE ({device})"

    return f"Other ({device})"


# ============================================================
# PATH / ASSET CHECK
# ============================================================

def is_asset_path(path: str) -> bool:
    p = (path or "").lower()
    return p.endswith(ASSET_EXTENSIONS)


# ============================================================
# REFERRER CLASSIFICATION
# ============================================================

def normalize_referrer(ref: str) -> str:
    if not ref or ref == "-":
        return "Direct"

    r = ref.lower()

    if "google." in r:
        return "Google"
    if "bing.com" in r:
        return "Bing"
    if "duckduckgo.com" in r:
        return "DuckDuckGo"
    if "facebook.com" in r or "fb." in r:
        return "Facebook"
    if "instagram.com" in r:
        return "Instagram"
    if "twitter.com" in r or "x.com" in r or "t.co" in r:
        return "X/Twitter"

    return "Referral"


# ============================================================
# FULL BOT CLASSIFICATION (UA + BEHAVIOR)
# ============================================================

def classify_human_vs_bot(ua: str):
    score = 0

    if not ua:
        score += 3
    else:
        if BOT_REGEX.search(ua):
            score += 5
        if "headless" in ua.lower():
            score += 4

    return score >= 5
