using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Reports
{
    public class PresentationReports
    {
        public void ShowPresentingStudentCounts(Context context)
        {
            // Show count of students for each presentation category (SW, HW+SW, hu and eng courses, RobonAUT)
            var infoCourseCategories = context.CourseCategories.Where(cc => cc.Major == CourseCategory.MajorEnum.Info);
            var villAndMechaCourseCategories = context.CourseCategories.Where(cc => cc.Major == CourseCategory.MajorEnum.Villany || cc.Major == CourseCategory.MajorEnum.Mecha);
            var nSwHu = countPresentingStudents(infoCourseCategories, true);
            var nHwHu = countPresentingStudents(villAndMechaCourseCategories, true);
            var nSwEn = countPresentingStudents(infoCourseCategories, false);
            var nHwEn = countPresentingStudents(villAndMechaCourseCategories, false);

            Console.WriteLine($"SW session HU {nSwHu}\nSW session EN {nSwEn}\nHW session HU {nHwHu}\nHW session EN {nHwEn}");

            var robonautStudentCount = context.Topics.Where(c => c.Title.Contains("RobonAUT") && c.Title.Contains("verseny")).Select(t => t.RegisteredStudents.Count).Sum();
            Console.WriteLine($"Among these, RobonAUT topic student count: {robonautStudentCount}");
        }

        private int countPresentingStudents(IEnumerable<CourseCategory> courseCategories, bool countHungarianStudents)
        {
            return courseCategories.SelectMany(cc => cc.Courses).Where(c => c.HasPresentation && (c.IsEnglish ^ countHungarianStudents)).Select(c => c.EnrolledStudentCountInNeptun).Sum().Value;
        }
    }
}
