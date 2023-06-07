using Common.Helpers;
using Common.Model;
using System.Text.RegularExpressions;

namespace Common.DataSources
{
    // All courses based on the Neptun export xlsx
    public class NeptunCourseSource
    {
        const string NeptunExportFilename = @"c:\_onlabFelugyeletAdatok\kurzusok_neptunExport.xlsx";

        public IEnumerable<Course> GetCourses()
        {
            Excel2Dict e = new Excel2Dict();
            // Load current student counts on the courses (exported from Neptun)
            var neptunTable = e.Read(NeptunExportFilename, 0, 1);

            Regex getEnrolledStudentCount = new Regex(@"(\d+)/\d+/\d+");
            foreach(var neptunEntryLine in neptunTable)
            {
                Course c = new Course();
                c.ClassCode = neptunEntryLine["Tárgykód"].Substring(3);  // Remove the BME prefix
                c.CourseCode = neptunEntryLine["Kurzus kód"];
                var enrollmentData = neptunEntryLine["Létszám"];
                var enrolledStudentCountString = getEnrolledStudentCount.Match(enrollmentData).Groups[1].Value;
                c.EnrolledStudentCountInNeptun = int.Parse(enrolledStudentCountString);
                yield return c;
            }
        }
    }
}
