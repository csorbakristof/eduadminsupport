using Common.Checks;
using Common.Helpers;
using Common.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Common.Reports
{
    public class GradingsCleanedForNeptun
    {
        public struct GradingForNeptun
        {
            public string XlsFilename;
            public string NKod;
            public string Grade;
        }

        private List<GradingForNeptun> collectedGradings = new List<GradingForNeptun>();
        private List<GradingStatus> statuses = new List<GradingStatus>();

        public IEnumerable<GradingForNeptun> GetGradings() => collectedGradings;
        public IEnumerable<GradingStatus> GetStatuses() => statuses;

        public void CreateGradings(Context context)
        {
            foreach (Student s in context.Students)
            {
                var gradings = context.Gradings.Where(g => g.StudentNKodFromGrading == s.NKod).ToArray();
                if (gradings.Length == 0)
                {
                    if (s.EnrolledCourses.Count > 0)
                        reportNoGradingEntry(s.EnrolledCourses.FirstOrDefault(), s);
                    else
                        reportNoCourseNoTopic(s);
                }
                else if (gradings.Length == 1)
                {
                    if (gradings[0].Grade == null)
                    {
                        reportAwaitsGrading(s.EnrolledCourses.FirstOrDefault(), s);
                    }
                    else if (s.EnrolledCourses.Count == 1)
                    {
                        addGradingWithSingleGradingSingleCourse(s, s.EnrolledCourses.First(), gradings[0]);
                    }
                    else
                    {
                        reportMultiCourse(s, gradings, s.EnrolledCourses);
                    }
                }
                else
                {
                    reportMultiCourse(s, gradings, s.EnrolledCourses);
                }
            }
        }

        #region Methods used during grade collection
        private void addGradingWithSingleGradingSingleCourse(Student s, Course course, Grading grading)
        {
            if (course.ClassCode == grading.ClassCodeInGrading)
                addGrading(course, s, grading.Grade.Value, grading);
            else
                addGradingWithCourseCodeMismatch(course, s, grading.Grade.Value, grading);
        }

        private void reportMultiCourse(Student s, IEnumerable<Grading> gradings, IEnumerable<Course> enrolledCourses)
        {
            statuses.Add(new GradingStatus()
            {
                Student = s,
                Status = GradingStatus.StatusEnum.MultipleCoursesInNeptun,
                Details = "Gradings: "
                    + string.Join(',', gradings) + " Courses: "
                    + string.Join(',', enrolledCourses)
            });
        }

        private void reportNoCourseNoTopic(Student s)
        {
            statuses.Add(new GradingStatus()
            {
                Student = s,
                Status = GradingStatus.StatusEnum.NoNeptunCourseNoTopic
            });
        }

        private void addGrading(Course course, Student s, int grade, Grading grading)
        {
            collectedGradings.Add(new GradingForNeptun() { XlsFilename = course.CourseCodeForExportFilename(), NKod = s.NKod, Grade = grade.ToString() });
            statuses.Add(new GradingStatus()
            {
                TargetCourseInNeptun = course,
                Student = s,
                Grading = grading,
                Status = GradingStatus.StatusEnum.Success
            });
        }

        private void addGradingWithCourseCodeMismatch(Course course, Student s, int grade, Grading grading)
        {
            collectedGradings.Add(new GradingForNeptun() { XlsFilename = course.CourseCodeForExportFilename(), NKod = s.NKod, Grade = grade.ToString() });
            statuses.Add(new GradingStatus()
            {
                TargetCourseInNeptun = course,
                Student = s,
                Grading = grading,
                OtherClassCode = grading.ClassCodeInGrading,
                Status = GradingStatus.StatusEnum.OtherCourseInNeptun
            });
        }

        private void reportNoGradingEntry(Course? course, Student s)
        {
            string details;
            if (s.TopicRegistrations.Count > 0)
                details = $"Advisors: {string.Join(',', s.TopicRegistrations.Select(tr => tr.Item1))}";
            else
                details = "No topic registration either.";

            statuses.Add(new GradingStatus()
            {
                TargetCourseInNeptun = course,
                Student = s,
                Status = GradingStatus.StatusEnum.NoGradingEntry,
                Details = details
            });
        }

        private void reportAwaitsGrading(Course? course, Student s)
        {
            statuses.Add(new GradingStatus()
            {
                TargetCourseInNeptun = course,
                Student = s,
                Advisor = s.TopicRegistrations.FirstOrDefault().Item1 as Advisor,
                Status = (course!=null) ? GradingStatus.StatusEnum.AwaitsGrading : GradingStatus.StatusEnum.AwaitsGradingWithoutEnrollment
            });
        }
        #endregion

        public void GenerateExcelFiles(string path)
        {
            Dictionary<string, List<GradingForNeptun>> dict = new Dictionary<string, List<GradingForNeptun>>();

            foreach (GradingForNeptun grading in collectedGradings)
            {
                if (!dict.ContainsKey(grading.XlsFilename))
                {
                    dict[grading.XlsFilename] = new List<GradingForNeptun>();
                }
                dict[grading.XlsFilename].Add(grading);
            }

            // Use the 'dict' dictionary to generate Excel files
            NeptunImportXlsxWriter writer = new NeptunImportXlsxWriter();
            foreach (var filename in dict.Keys)
            {
                Console.WriteLine($"Writing file: {filename}...");
                File.Delete(path + filename);
                writer.WriteEntriesToExcel(path + filename, dict[filename].Select(gfn =>
                    new NeptunImportXlsxWriter.Entry() { NKod=gfn.NKod, Grade=gfn.Grade }));
            }
        }

        public void SaveGradingOutput(string lastGradingOutputFilename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<GradingForNeptun>));
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(lastGradingOutputFilename))
            {
                serializer.Serialize(file, collectedGradings);
            }
        }
    }
}
