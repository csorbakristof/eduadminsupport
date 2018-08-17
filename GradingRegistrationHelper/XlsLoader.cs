using ExcelReaderStandardLibrary;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Windows.Storage;
using System.Text.RegularExpressions;

namespace GradingRegistrationHelper
{
    public class XlsLoader
    {
        protected List<Student> students = new List<Student>();

        public ITableReader Reader { get; set; } = new Excel2Dict();

        public List<Student> GetStudentList() => this.students;

        public void Load(StorageFile gradesFile, StorageFile[] attendanceFiles, StorageFile advisorFile)
        {
            students = LoadGrades(gradesFile).ToList();
            MergeStudents(LoadAdvisors(advisorFile));
            MergeStudents(LoadAttendances(attendanceFiles));
        }

        private void MergeStudents(IEnumerable<Student> newStudents)
        {
            foreach (var s in newStudents)
            {
                Student toUpdate = students.Where(e => e.NeptunCode == s.NeptunCode).SingleOrDefault();
                if (toUpdate == null)
                    students.Add(s);
                else
                    toUpdate.Merge(s);
            }
        }

        protected virtual IEnumerable<Student> LoadGrades(StorageFile gradesFile)
        {
            var stream = gradesFile.OpenStreamForReadAsync().Result;
            var dict = Reader.Read(stream, 0, 4);
            return dict.Select(d => GradeEntryToStudent(d));
        }

        protected virtual IEnumerable<Student> LoadAttendances(StorageFile[] attendanceFiles)
        {
            IEnumerable<Student> result = null;
            foreach (var f in attendanceFiles)
            {
                var subject = SubjectCodeFromFilename(f.Name);
                var dict = Reader.Read(f.OpenStreamForReadAsync().Result, 0, 1);
                var enumerable = dict.Select(d => AttendanceEntryToStudent(subject,d));
                result = result == null ? enumerable : result.Concat(enumerable);
            }
            return result;
        }

        public static string SubjectCodeFromFilename(string filename)
        {
            Regex r = new Regex(@"jegyimport_([A-Z0-9]+_[A-Za-z]+)_.+", RegexOptions.IgnoreCase);
            Match m = r.Match(filename);
            if (!m.Success)
                throw new ArgumentException($"Cannot find subject code in filename {filename}");
            var subjectcode = m.Groups[1].Value;
            return subjectcode;
        }

        protected virtual IEnumerable<Student> LoadAdvisors(StorageFile advisorFile)
        {
            var dict = Reader.Read(advisorFile.OpenStreamForReadAsync().Result, 1, 1);
            return dict.Select(d => AdvisorEntryToStudent(d));
        }

        static public readonly string GradingNameKey = "Hallgató neve";
        static public readonly string GradingNCodeKey = "Hallgató NEPTUN";
        static public readonly string GradingGradeKey = "Osztályzat";
        static public readonly string GradingSubjectKey = "Tárgy NEPTUN";
        private Student GradeEntryToStudent(Dictionary<string, string> entry)
        {
            var student = new Student();
            student.Name = entry[GradingNameKey];
            student.NeptunCode = entry[GradingNCodeKey];
            if (int.TryParse(entry[GradingGradeKey], out int grade))
                student.SetGrade(entry[GradingSubjectKey], grade);

            return student;
        }

        static public readonly string AdvisorNameKey = "Hallgató neve";
        static public readonly string AdvisorNCodeKey = "Hallg. nept";
        static public readonly string AdvisorAdvisorKey = "Konzulens";
        private Student AdvisorEntryToStudent(Dictionary<string, string> entry)
        {
            var student = new Student();
            student.Name = entry[AdvisorNameKey];
            student.NeptunCode = entry[AdvisorNCodeKey];
            student.Advisor = entry[AdvisorAdvisorKey];
            return student;
        }

        static public readonly string AttendanceNameKey = "Név";
        static public readonly string AttendanceNCodeKey = "Neptun kód";
        private Student AttendanceEntryToStudent(string SubjectCode, Dictionary<string, string> entry)
        {
            var student = new Student();
            student.Name = entry["Név"];
            student.NeptunCode = entry["Neptun kód"];
            student.AddAttendance(SubjectCode);
            return student;
        }
    }
}
