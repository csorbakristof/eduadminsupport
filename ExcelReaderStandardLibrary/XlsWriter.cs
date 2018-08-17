using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExcelReaderStandardLibrary
{
    public class XlsWriter
    {
        public void WriteXls(Stream s, List<string[]> lines)
        {
            using (var xls = new ExcelPackage(s))
            {
                ExcelWorksheet ws = xls.Workbook.Worksheets.Add("Jegyek");
                for (int row=1; row<=lines.Count;row++)
                    for(int col=1; col<=lines[row-1].Length;col++)
                        ws.Cells[row, col].Value = lines[row - 1][col - 1];
                xls.Save();
            }

        }
    }
}
