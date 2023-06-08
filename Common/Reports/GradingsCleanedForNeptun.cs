using Common.Checks;
using Common.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Reports
{
    public class GradingsCleanedForNeptun
    {
        public struct GradingForNeptun
        {
            public string ClassCourseCode;
            public string NKod;
            public string Grade;
        }

        private List<GradingForNeptun> gradings = new List<GradingForNeptun>();
        private List<GradingStatus> statuses = new List<GradingStatus>();

        public IEnumerable<GradingForNeptun> GetGradings() => gradings;
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
            gradings.Add(new GradingForNeptun() { ClassCourseCode = course.CourseCodeForExportFilename(), NKod = s.NKod, Grade = grade.ToString() });
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
            gradings.Add(new GradingForNeptun() { ClassCourseCode = course.CourseCodeForExportFilename(), NKod = s.NKod, Grade = grade.ToString() });
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
            statuses.Add(new GradingStatus()
            {
                TargetCourseInNeptun = course,
                Student = s,
                Status = GradingStatus.StatusEnum.NoGradingEntry,
                Details = $"Advisors: {string.Join(',', s.TopicRegistrations.Select(tr => tr.Item1))}"
            });
        }

        private void reportAwaitsGrading(Course? course, Student s)
        {
            statuses.Add(new GradingStatus()
            {
                TargetCourseInNeptun = course,
                Student = s,
                Advisor = s.TopicRegistrations.First().Item1 as Advisor,
                Status = GradingStatus.StatusEnum.AwaitsGrading
            });
        }
    }
}
