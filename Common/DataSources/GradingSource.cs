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
        public IEnumerable<(string StudentNKod, string ClassCode, int? Grade)> GetGrades(string filename)
        {
            HashSet<(string StudentNKod, string ClassCode, int? Grade)> results = new HashSet<(string StudentNKod, string ClassCode, int? Grade)>();

            // Load current advisor data from excel (exported from the departments portal)
            Excel2Dict e = new Excel2Dict();
            var gradeTable = e.Read(filename, 0, 4);
            // For each topic we search how many students are on it currently
            foreach (var line in gradeTable)
            {
                int? grade = null;
                string gradeString = line["Osztályzat"];
                if (!gradeString.Contains('-'))
                    grade = int.Parse(gradeString);
                var newValue = (line["Hallgató NEPTUN"], line["Tárgy NEPTUN"], grade);
                results.Add(newValue);
            }
            return results;
        }

    }
}
