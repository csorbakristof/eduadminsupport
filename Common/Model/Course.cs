namespace Common.Model
{
    public class Course
    {
        public string ClassCode { get; set; }   // Like VIAUAL00
        public string CourseCode { get; set; }  // Like L

        public bool IsEnglish => CourseCode.StartsWith('A');
        public int? EnrolledStudentCount { get; set; }

        public Course(string classCode, string courseCode)
        {
            ClassCode = classCode;
            CourseCode = courseCode;
        }
    }
}