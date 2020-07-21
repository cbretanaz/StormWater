using System.Data;
using System.Linq;
using ClosedXML.Excel;

namespace CoP.Enterprise
{
    public enum ClosedXMLDataStyle
    {
        Table,
        Range
    }
    public static class ExcelSupportClosedXml
    {
        public static XLWorkbook ExportToExcelClosedXML(
            this DataTable table, 
            string sheetName, string[] header, 
            string destination, bool unCamelCaseColumns = true, 
            ClosedXMLDataStyle dataStyle = ClosedXMLDataStyle.Range)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet(sheetName ?? "Default");
            var startingRow = 1;
            if (header != null && header.Any())
            {
                ws.Cell(1, 1).InsertData(header);
                startingRow = header.Length + 2;
            }

            if (unCamelCaseColumns)
            {
                foreach (DataColumn col in table.Columns)
                {
                    var rawColName = col.ColumnName;
                    var colName = "";
                    var lastChar = ' ';
                    for (var j = 0; j < rawColName.Length; j++)
                    {
                        if (char.IsLetterOrDigit(lastChar) //last char was a letter (not a space)
                            && (char.IsUpper(rawColName[j]) || char.IsDigit(rawColName[j])) //this char is uppercase so next char should be lower or the char is a digit 
                            && (j + 1) < rawColName.Length //index out of range test
                            && (char.IsLower(rawColName[j + 1]) || (char.IsDigit(rawColName[j]) && !char.IsDigit(rawColName[j - 1]))) //next char is lower or this char is a digit and the last was not a digit?
                            )
                            colName += " " + rawColName[j];
                        else colName += rawColName[j];
                        lastChar = rawColName[j];
                    }
                    col.ColumnName = colName;
                }
            }
            switch (dataStyle)
            {
                case ClosedXMLDataStyle.Table:
                    ws.Cell(startingRow, 1).InsertTable(table);
                    break;
                case ClosedXMLDataStyle.Range:
                    var ctr = 1;
                    foreach(DataColumn col in table.Columns)
                        ws.Cell(startingRow, ctr++).InsertData(new [] {col.ColumnName});
                    ws.Cell(startingRow+1, 1).InsertData(table.AsEnumerable());
                    break;
            }
            ws.Columns().AdjustToContents();
            if(destination!=null) wb.SaveAs(destination);
            return wb;
        }
    }
}
