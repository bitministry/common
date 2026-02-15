import geoip2.database

MMDB_PATH = r"services\GeoLite2-City.mmdb"

def get_ip_info(ip):
    try:
        with geoip2.database.Reader(MMDB_PATH) as reader:
            rp = reader.city(ip)
            sub = rp.subdivisions[0].names.get("en") if rp.subdivisions else None
            return {
                "country": rp.country.name,
                "city": rp.city.name,
                "subdivisions": sub,
                "latitude": rp.location.latitude,
                "longitude": rp.location.longitude,
                "postal_code": rp.postal.code,
                "accuracy_radius": rp.location.accuracy_radius,
            }
    except Exception:
        return {} 

