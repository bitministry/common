import json, ast , random
from decimal import Decimal 
from datetime import datetime, date
from typing import List, Dict, Any 

isnull = lambda x, default: default if (x is None or not x) else x

def batchify(items, batch_size):
    for i in range(0, len(items), batch_size):
        yield items[i:i+batch_size]

def read_file(filename):
    """Read and return file contents as string, or None if file doesn't exist."""
    try:
        with open(filename, "r", encoding="utf-8") as f:
            return f.read()
    except FileNotFoundError:
        return None

def write_file(filename, content):
    """Write string to file, overwrite if exists."""
    with open(filename, "w", encoding="utf-8") as f:
        f.write(content)

def read_json(filename):
    """Read a JSON file and return as Python object, or None if error."""
    s = read_file(filename)
    if s is None:
        return None
    try:
        return json.loads(s)
    except Exception as e:
        print(f"Failed to load JSON from {filename}: {e}")
        return None

def write_json(filename, obj, indent=2):
    """Extended JSON serializer that handles common types"""
    def extended_encoder(obj):
        if isinstance(obj, Decimal):
            return float(obj)
        if isinstance(obj, (datetime, date)):
            return obj.isoformat()
        if hasattr(obj, '__dict__'):
            return vars(obj)
        raise TypeError(f"Object of type {type(obj)} is not JSON serializable")
    
    write_file(
        filename,
        json.dumps(obj, ensure_ascii=False, indent=indent, default=extended_encoder)
    )



def safe_load_json(filename):
    """Try to read JSON; fall back to ast.literal_eval if needed."""
    try:
        with open(filename, "r", encoding="utf-8") as f:
            s = f.read()
        return json.loads(s)
    except json.JSONDecodeError:
        try:
            return ast.literal_eval(s)
        except Exception as e:
            print(f"Failed to parse file {filename}: {e}")
            return None
        


def get_array(data) -> List[Dict[str, Any]]:
    if isinstance(data, dict):
        first_key = next(iter(data), None)
        if first_key is not None:
            value = data[first_key]
            if isinstance(value, list):
                return value
            else:
                return [data]
        else:
            return []
    elif isinstance(data, list):
        return data
    else:
        raise TypeError("data can't be solved")


def randomize_around_anchor(anchor, percent_offset):
    offset = anchor * (percent_offset / 100)
    lower_bound = anchor - offset
    upper_bound = anchor + offset
    return random.uniform(lower_bound, upper_bound)