SQLSRV_HOST= "JDEV"
SQLDB="bmSysLog"

DB_CONN_STR = 'DRIVER={ODBC Driver 17 for SQL Server};SERVER='+ SQLSRV_HOST +';DATABASE='+ SQLDB +';Trusted_Connection=yes;'
ALCHEMY_CONN_STR = "mssql+pyodbc://"+SQLSRV_HOST+":1433/"+ SQLDB +"?driver=ODBC+Driver+17+for+SQL+Server"