import pyodbc
from _config import DB_CONN_STR

def get_data(sql, conn_str:str=None):
    with pyodbc.connect(conn_str or DB_CONN_STR) as conn:
        with conn.cursor() as cur: 
            cur.execute(sql)
            rows = cur.fetchall()
            columns = [column[0] for column in cur.description]
            result = [dict(zip(columns, row)) for row in rows] 
            return result

def get_scalar(sql, conn_str: str = None):
    with pyodbc.connect(conn_str or DB_CONN_STR) as conn:
        with conn.cursor() as cur:
            cur.execute(sql)
            row = cur.fetchone()
            return row[0] if row and row[0] is not None else None


def sql_non_query( query, params=None, conn_str:str=None):
    with pyodbc.connect(conn_str or DB_CONN_STR) as conn:
        with conn.cursor() as cursor:
            cursor.execute(query, params or [])
            conn.commit()


