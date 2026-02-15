import pyodbc
from dataclasses import asdict
from typing import List, Dict, Any
from sqlalchemy import create_engine, text

from _config import *

def query(sql, params=None):
    with pyodbc.connect(DB_CONN_STR) as conn:
        cur=conn.cursor(); cur.execute(sql, params or [])
        rows=cur.fetchall(); cols=[c[0] for c in cur.description]
        return [dict(zip(cols,r)) for r in rows]


def get_id(tableName, where_cols: List[str], data_dict: Dict[str, Any], id_col: str = None):
    id_col = id_col or f"{tableName}Id"
    
    # Build "Col = ?" string
    where_clause = " AND ".join([f"{col} = ?" for col in where_cols])
    
    # Extract values from dict in the correct order
    params = [data_dict.get(col) for col in where_cols]
    
    sql = f"SELECT {id_col} FROM {tableName} WHERE {where_clause}"
    res = query(sql, params)
    
    return res[0][id_col] if res else None


def execute(sql, params=None):
    with pyodbc.connect(DB_CONN_STR) as conn:
        cur=conn.cursor(); cur.execute(sql, params or [])
        conn.commit()



def dataclass_list_to_dict_list(objs: List[Any]) -> List[Dict[str, Any]]:
    return [asdict(obj) for obj in objs] 



def upsert_data(
    data: List[Dict[str, Any]],
    table_name: str,
    id_cols: List[str],
    conn_str: str = ALCHEMY_CONN_STR,
    doInsert: bool = False
):
    if not data:
        return

    db = create_engine(conn_str)
    with db.begin() as conn:
        for row in data:
            upsert_item(conn, row, table_name, id_cols, doInsert)



def upsert_item(conn, row: Dict[str, Any], table: str, updatewhere_cols: List[str], doInsert: bool):

    if isinstance(updatewhere_cols, str):
        raise TypeError("id_cols must be a list of column names, not a string")

    cols = list(row.keys())

    if not updatewhere_cols:
        sql = f"INSERT INTO {table} ({', '.join(cols)}) VALUES ({', '.join(f':{c}' for c in cols)})"
    else:
        updates = [col for col in cols if col not in updatewhere_cols]
        source = ", ".join(f":{c} AS {c}" for c in cols)
        match = " AND ".join(f"target.{c} = source.{c}" for c in updatewhere_cols)

        if doInsert:
            sql = f"""
            MERGE INTO {table} AS target
            USING (SELECT {source}) AS source
            ON {match}
            WHEN MATCHED THEN UPDATE SET {', '.join(f'target.{c} = source.{c}' for c in updates)}
            WHEN NOT MATCHED THEN INSERT ({', '.join(cols)})
            VALUES ({', '.join(f'source.{c}' for c in cols)});
            """
        else:
            sql = f"""
            MERGE INTO {table} AS target
            USING (SELECT {source}) AS source
            ON {match}
            WHEN MATCHED THEN UPDATE SET {', '.join(f'target.{c} = source.{c}' for c in updates)};
            """

    # print (sql)
    print (row) 

    conn.execute(text(sql), row)



def insert_data(data: List[Dict[str, Any]], table_name: str, conn_str: str = ALCHEMY_CONN_STR):
    db = create_engine(conn_str)
    with db.begin() as conn:
        for row in data:
            cols = list(row.keys())
            names = ", ".join(cols)
            vals = ", ".join([f":{c}" for c in cols])
            sql = text(f"INSERT INTO {table_name} ({names}) VALUES ({vals})")
            conn.execute(sql, row)