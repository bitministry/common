using BitMinistry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;

namespace BitMinistry.Data
{
    public class OleDbExcel : BSqlCommanderUtil, IDisposable
    {


        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="xbCreateNewFile_NoImex">IMEX=1 : mixed data type; no IMEX : opening connection creates a new file </param>

        FileInfo _file;

        OleDbConnection _oleCon;

        public OleDbExcel(string path, bool xbCreateNewFile_NoImex = false)
        {
            _file = new FileInfo(path);

            if (xbCreateNewFile_NoImex && _file.Exists) File.Delete(path);

            try
            {
                if (_file.Directory.Exists)
                    _file.Directory.Create();

                _oleCon = new OleDbConnection(
                    $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};Extended Properties=\"Excel 12.0 Xml;HDR=Yes;{(xbCreateNewFile_NoImex ? "" : "IMEX=1;")}\"");
                _oleCon.Open();
            }
            catch (Exception ex)
            {
                _oleCon = new OleDbConnection(
                    $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={path};Extended Properties=\"Excel 8.0;HDR=Yes;{(xbCreateNewFile_NoImex ? "" : "IMEX=1;")}\"");
                _oleCon.Open();

            }
        }


        #region fields, properites and indexers

        string[] _sheetNamesInWorkbook;
        public string[] SheetNamesInWorkbook => _sheetNamesInWorkbook ??
                (_sheetNamesInWorkbook = _oleCon.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows.Cast<DataRow>().Select(d => d["TABLE_NAME"].ToString()).ToArray());

        ExcelColumn[] _allColumnsInWorkbook;
        public ExcelColumn[] AllColumnsInWorkbook => _allColumnsInWorkbook ??
                    (_allColumnsInWorkbook = _oleCon.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows.Cast<DataRow>().Select(d => LoadEntityWithPropertyValuesFromDataRow<ExcelColumn>(d)).ToArray());


        public IEnumerable<ExcelColumn> GetColumns(int xiSheetNo) => AllColumnsInWorkbook.Where(c => c.TableName == SheetNamesInWorkbook[xiSheetNo]);



        #endregion

        public DataTable DataTable { get; private set; }

        public void FillTable(int sheetNumber) => FillTable("SELECT * FROM [" + SheetNamesInWorkbook[sheetNumber] + "]");


        public void FillTable(string xsQuery, int page = 1, int iPageSize = int.MaxValue)
        {

            var adapter = new OleDbDataAdapter { SelectCommand = new OleDbCommand(xsQuery, _oleCon) };
            DataTable = new DataTable();
            adapter.Fill(Math.Abs((page - 1) * iPageSize), iPageSize, DataTable);
            SqlQuery = xsQuery;
        }

        public string SqlQuery { get; private set; }

        public IEnumerable<TSqlQueriable> GetEntities<TSqlQueriable>() where TSqlQueriable : ISqlQueryable
            => DataTable.Rows.Cast<DataRow>().Select(row => LoadEntityWithPropertyValuesFromDataRow<TSqlQueriable>(row));



        public void Dispose()
        {
            _oleCon.Dispose();
        }


    }

    public class ExcelColumn : ISqlQueryable
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public int? DataType { get; set; }
        public int? MaxLength { get; set; }
        public bool? IsNullable { get; set; }

    }

}