using Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataSources
{
    public class GradingSource
    {
        const string PortalGradingExportFilename = @"c:\_onlabFelugyeletAdatok\tema-osztalyzatok-22-23-tavasz.xlsx";

        public IEnumerable<(string StudentNKod, string ClassCode, int? Grade)> GetGrades()
        {
            // Load current advisor data from excel (exported from the departments portal)
            Excel2Dict e = new Excel2Dict();
            var gradeTable = e.Read(PortalGradingExportFilename, 0, 4);
            // For each topic we search how many students are on it currently
            foreach (var line in gradeTable)
            {
                int? grade = null;
                string gradeString = line["Osztályzat"];
                if (!gradeString.Contains('-'))
                    grade = int.Parse(gradeString);
                yield return (line["Hallgató NEPTUN"], line["Tárgy NEPTUN"], grade);
            }
        }

    }
}
