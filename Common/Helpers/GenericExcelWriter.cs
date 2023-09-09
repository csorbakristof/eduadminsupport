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
        public static void WriteToNewExcelFile(string fileName, string[] headers, IEnumerable<string[]> lines)
        {
            using (var writer = new GenericExcelWriter(fileName))
            {
                writer.AppendRow(headers);
                foreach (var l in lines)
                    writer.AppendRow(l);
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
            worksheet = excel.Workbook.Worksheets.Add(worksheetname);
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

        public static void WriteToNewExcelFile(object studentListWithNoTopicRegistrations, string[] strings, object value)
        {
            throw new NotImplementedException();
        }
    }
}
