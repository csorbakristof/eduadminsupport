using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelReaderStandardLibrary
{
    public class Excel2Dict : ITableReader
    {
        public List<Dictionary<string, string>> Read(Stream inputStream, int worksheetIndex, int headerRowIndex)
        {
            using (ExcelPackage xls = new ExcelPackage(inputStream))
            {
                ExcelWorksheet ws = xls.Workbook.Worksheets[worksheetIndex];
                var headers = LoadHeadersFromFirstRow(ws, headerRowIndex);
                return LoadContent(ws, headers, headerRowIndex);
            }
        }

        private List<string> LoadHeadersFromFirstRow(ExcelWorksheet ws, int headerRowIndex)
        {
            List<string> headers = new List<string>();
            int col = 1;
            while (true)
            {
                string value = ws.Cells[headerRowIndex, col].Text;
                if (!value.Equals(""))
                    headers.Add(value);
                else
                    break;
                col++;
            }
            return headers;
        }

        private List<Dictionary<string, string>> LoadContent(ExcelWorksheet ws, List<string> headers, int headerRowIndex)
        {
            List<Dictionary<string, string>> res = new List<Dictionary<string, string>>();
            int row = headerRowIndex+1;
            while (true)
            {
                if (ws.Cells[row, 1].Text == "")
                    break;

                Dictionary<string, string> currentRow = new Dictionary<string, string>();
                for (int col=0; col<headers.Count; col++)
                {
                    string cellContent = ws.Cells[row, col+1].Text; // Node: 1-based column indexing
                    currentRow.Add(headers[col], cellContent);
                }
                res.Add(currentRow);
                row++;
            }
            return res;
        }
    }
}
