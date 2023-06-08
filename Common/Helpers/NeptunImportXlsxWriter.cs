using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public class NeptunImportXlsxWriter
    {
        public struct Entry
        {
            public string NKod;
            public string Grade;
        }

        public void WriteEntriesToExcel(string fileName, IEnumerable<Entry> entries)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var worksheet = package.Workbook.Worksheets.Add("Entries");
                int rowIndex = 0;
                foreach(var e in entries)
                {
                    worksheet.Cells[rowIndex + 1, 1].Value = e.NKod;
                    worksheet.Cells[rowIndex + 1, 2].Value = e.Grade;
                    worksheet.Cells[rowIndex + 1, 3].Value = "0";
                    rowIndex++;
                }

                package.Save();
            }
        }
    }
}
