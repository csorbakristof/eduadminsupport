using System;
using System.Collections.Generic;

namespace GradingRegistrationHelper
{
    public class Student
    {

        public string Name { get; set; } = null;
        public string NeptunCode { get; set; } = null;
        public string GradedSubject { get; set; } = null;
        public int? Grade { get; set; } = null;
        public string Advisor { get; set; }
        public List<string> Attendances = new List<string>();

        public static readonly string GradingOK = "OK";
        public static readonly string SingleMismatchingAttendanceOK = "OK, subject mismatch: attended {0}, graded {1}";
        public static readonly string MultipleAttendedSubjectsThisOneUngradedOK
            = "OK, multiple attendances, graded {0} instead of this {1}.";
        public static readonly string Ungraded = "Ungraded, advisor: {0}";

        public void SetGrade(string subject, int grade)
        {
            GradedSubject = subject;
            Grade = grade;
        }

        public void AddAttendance(string subject)
        {
            Attendances.Add(subject);
        }

        public bool IsAttending(string subject)
        {
            return Attendances.Contains(subject);
        }

        public int? GetGrade(string subject)
        {
            GetGradingSituation(subject);   // Remains for exception checking
            return Grade;
        }

        public string GetGradingSituation(string subject)
        {
            if (!Attendances.Contains(subject))
                throw new ArgumentException("Should not ask grading for unattended subject.");

            if (Grade == null)
                return string.Format(Ungraded, Advisor);

            // Now we are sure they attended the asked subject.
            if (Attendances.Count == 1)
            {
                return (subject.StartsWith($"BME{GradedSubject}_"))
                    ? GradingOK
                    : string.Format(SingleMismatchingAttendanceOK, subject, GradedSubject);
            }
            else // Multiple attendances
            {
                return (GradedSubject == subject)
                    ? GradingOK
                    : string.Format(MultipleAttendedSubjectsThisOneUngradedOK,
                        GradedSubject, subject);
            }
        }

        public void Merge(Student other)
        {
            if (Name != other.Name)
                throw new ArgumentException($"Cannot merge student entry, names mismatch: {Name} != {other.Name}");
            if (NeptunCode != other.NeptunCode)
                throw new ArgumentException($"Cannot merge student entry, neptun code mismatch: {NeptunCode} != {other.NeptunCode}");
            if (GradedSubject != null && other.GradedSubject != null && GradedSubject != other.GradedSubject)
                throw new ArgumentException($"Cannot merge student entry, different GradedSubject values: {GradedSubject} != {other.GradedSubject}");
            if (Grade != null && other.Grade != null && Grade != other.Grade)
                throw new ArgumentException($"Cannot merge student entry, different Grade values: {Grade} != {other.Grade}");

            if (GradedSubject == null && other.GradedSubject != null)
                GradedSubject = other.GradedSubject;

            if (Grade == null && other.Grade != null)
                Grade = other.Grade;

            if (Advisor == null && other.Advisor != null)
                Advisor = other.Advisor;
            if (Advisor != null && other.Advisor != null && Advisor != other.Advisor)
                Advisor += "," + other.Advisor;

            foreach (var att in other.Attendances)
                if (!Attendances.Contains(att))
                    Attendances.Add(att);
        }
    }
}
