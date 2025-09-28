from dataclasses import asdict
from typing import List, Dict, Any
from sqlalchemy import create_engine, text, Connection 
from _config import ALCHEMY_CONN_STR

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


def upsert_item( item: Dict[str, Any], table_name: str, updatewhere_cols: List[str], doInsert: bool, conn_str : str = ALCHEMY_CONN_STR ):
    db = create_engine(conn_str)
    with db.begin() as conn:
        upsert_item(conn, item, table_name, updatewhere_cols, doInsert)

def upsert_item(conn : Connection, row: Dict[str, Any], table: str, updatewhere_cols: List[str], do_insert: bool, ignore_nulls: bool = True):

    if isinstance(updatewhere_cols, str):
        raise TypeError("id_cols must be a list of column names, not a string")

    if ignore_nulls : 
        row = {k: v for k, v in row.items() if v is not None}

    cols = list(row.keys())
    updates = [col for col in cols if col not in updatewhere_cols]

    source = ", ".join(f":{c} AS {c}" for c in cols)
    match = " AND ".join(f"target.{c} = source.{c}" for c in updatewhere_cols)


    if do_insert:
        # Full UPSERT
        sql = f"""
        MERGE INTO {table} AS target
        USING (SELECT {source}) AS source
        ON {match}
        WHEN MATCHED THEN UPDATE SET {', '.join(f'target.{c} = source.{c}' for c in updates)}
        WHEN NOT MATCHED THEN INSERT ({', '.join(cols)})
        VALUES ({', '.join(f'source.{c}' for c in cols)});
        """
    else:
        # Only UPDATE
        sql = f"""
        MERGE INTO {table} AS target
        USING (SELECT {source}) AS source
        ON {match}
        WHEN MATCHED THEN UPDATE SET {', '.join(f'target.{c} = source.{c}' for c in updates)};
        """

    # print (sql)
    print (row) 

    conn.execute(text(sql), row)



def dataclass_list_to_dict_list(objs: List[Any]) -> List[Dict[str, Any]]:
    return [asdict(obj) for obj in objs]                