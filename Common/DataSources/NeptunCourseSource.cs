﻿using Common.Helpers;
using Common.Model;
using System.Text.RegularExpressions;

namespace Common.DataSources
{
    // All courses based on the Neptun export xlsx
    public class NeptunCourseSource
    {
        public IEnumerable<Course> GetCourses(string filename)
        {
            Excel2Dict e = new Excel2Dict();
            // Load current student counts on the courses (exported from Neptun)
            var neptunTable = e.Read(filename, 0, 1);

            Regex getEnrolledStudentCount = new Regex(@"(\d+)/\d+/\d+");
            foreach(var neptunEntryLine in neptunTable)
            {
                Course c = new Course();
                c.Name = neptunEntryLine["Tárgy név"];
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
