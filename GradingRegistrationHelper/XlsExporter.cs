using System;
using System.Collections.Generic;
using System.Linq;

namespace GradingRegistrationHelper
{
    public class XlsExporter
    {
        public Dictionary<string, List<Entry>> CollectExports(List<Student> students)
        {
            var exportDictionary = new Dictionary<string, List<Entry>>();
            foreach (var att in GetAttendances(students))
                exportDictionary.Add(att, GetEntries(students, att).ToList());
            return exportDictionary;
        }

        protected virtual IEnumerable<Entry> GetEntries(List<Student> students, string att)
        {
            return students.Where(s => s.IsAttending(att))
                .Select(s => new Entry(s, att));
        }

        protected virtual IEnumerable<string> GetAttendances(List<Student> students)
        {
            return students.SelectMany(s => s.Attendances).Distinct();
        }

        public class Entry
        {
            public Entry()
            {
            }

            public Entry(Student s, string attendance)
            {
                Name = s.Name;
                NeptunCode = s.NeptunCode;
                var grade = s.GetGrade(attendance);
                Grade = (grade != null) ? grade.Value.ToString() : null;
                Comment = s.GetGradingSituation(attendance);
            }

            public string Name { get; set; }
            public string NeptunCode { get; set; }
            public string Grade { get; set; }
            public string Comment { get; set; }

            public override string ToString()
            {
                return $"{Name} ({NeptunCode}) {Grade} : {Comment}";
            }
        }
    }
}
