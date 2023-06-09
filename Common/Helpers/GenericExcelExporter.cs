using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public class GenericExcelWriter : IDisposable
    {
        public static void WriteToNewExcelFile(string fileName, IEnumerable<string[]> lines)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (System.IO.File.Exists(fileName))
                System.IO.File.Delete(fileName);

            using (var package = new ExcelPackage(new FileInfo(fileName)))
            {
                var worksheet = package.Workbook.Worksheets.Add("Default");
                int rowIndex = 0;
                foreach (var l in lines)
                {
                    for (int i = 0; i < l.Length; i++)
                        worksheet.Cells[rowIndex + 1, i + 1].Value = l[i];
                    rowIndex++;
                }

                package.Save();
            }
        }

        private ExcelPackage excel;
        private ExcelWorksheet worksheet;
        private int rowIndex;
        public GenericExcelWriter(string filename, string worksheetname = "default")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (System.IO.File.Exists(filename))
                System.IO.File.Delete(filename);

            excel = new ExcelPackage(new FileInfo(filename));
            worksheet = excel.Workbook.Worksheets.Add("Default");
            rowIndex = 0;
        }

        public void AppendRow(string[] values)
        {
            for (int i = 0; i < values.Length; i++)
                worksheet.Cells[rowIndex + 1, i + 1].Value = values[i];
            rowIndex++;
        }

        public void Dispose()
        {
            excel.Save();
            excel.Dispose();
        }
    }
}
