using Common.Helpers;
using Common.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Common.Reports.GradingsCleanedForNeptun;

namespace Common.Reports
{
    public class AdvisorCapacityAndLoad
    {
        class AdvisorCounters
        {
            // Some topics are shared among multiple advisors
            public float TotalCapacity;
            public float FreeSeatCount;
            public float CurrentStudentCount;

            public AdvisorCounters()
            {
                TotalCapacity = 0;
                FreeSeatCount = 0;
                CurrentStudentCount = 0;
            }
        }

        private Dictionary<Advisor, AdvisorCounters> counters;

        public async Task ShowAdvisorCapacityAndLoad(Context context, string? xlsFilenameOrNull)
        {
            // Students on topics with multiple advisors count to every advisor!
            counters = new Dictionary<Advisor, AdvisorCounters>();

            foreach (var topic in context.Topics)
            {
                float advisorCount = topic.Advisors.Count;
                foreach(var advisor in topic.Advisors)
                {
                    if (!counters.Keys.Contains(advisor))
                        counters.Add(advisor, new AdvisorCounters());

                    counters[advisor].TotalCapacity += topic.SeatCount / advisorCount;
                    counters[advisor].FreeSeatCount += (topic.SeatCount - topic.RegisteredStudents.Count) / advisorCount;
                    counters[advisor].CurrentStudentCount += topic.RegisteredStudents.Count / advisorCount;
                }
            }

            // Check for staff without any topics...
            //var staffNames = await WebDownloaderWithRegexMatcher.DownloadAndCollectMatches("https://www.aut.bme.hu/Staff", "\"(.+) fényképe\" src");
            //foreach (var name in staffNames)
            //    if (!counters.Keys.Any(a => a.Name == name))
            //        await Console.Out.WriteLineAsync($"Staff without any topics: {name}");

            Console.WriteLine("Advisor capacities: free seats count / current student count / total capacity on all topics");
            foreach (var advisor in counters.Keys)
            {
                Console.WriteLine($"Advisor {advisor.Name}: free {counters[advisor].FreeSeatCount} / occupied {counters[advisor].CurrentStudentCount} / total {counters[advisor].TotalCapacity}");
            }

            if (xlsFilenameOrNull != null)
                ExportStatsInExcel(xlsFilenameOrNull);
        }

        private void ExportStatsInExcel(string filename)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(filename)))
            {
                if (!package.Workbook.Worksheets.Any(ws=>ws.Name==("Advisors")))
                    package.Workbook.Worksheets.Add("Advisors");
                var worksheet = package.Workbook.Worksheets["Advisors"];
                worksheet.Cells[1, 1].Value = "Advisor";
                worksheet.Cells[1, 2].Value = "Free";
                worksheet.Cells[1, 3].Value = "Occupied";
                worksheet.Cells[1, 4].Value = "Total";
                int rowIndex = 1;
                foreach (var advisor in counters.Keys)
                {
                    worksheet.Cells[rowIndex + 1, 1].Value = advisor.Name;
                    worksheet.Cells[rowIndex + 1, 2].Value = counters[advisor].FreeSeatCount;
                    worksheet.Cells[rowIndex + 1, 3].Value = counters[advisor].CurrentStudentCount;
                    worksheet.Cells[rowIndex + 1, 4].Value = counters[advisor].TotalCapacity;
                    rowIndex++;
                }
                package.Save();
            }

        }
    }
}
