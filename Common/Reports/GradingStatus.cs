using Common.Model;
using System.Text;

namespace Common.Reports
{
    public class GradingStatus
    {
        public StatusEnum Status;
        public Course? TargetCourseInNeptun;
        public Student Student;
        public Grading? Grading;
        public Advisor? Advisor;
        public string? OtherClassCode;
        public string Details;

        public enum StatusEnum
        {
            Success,
            AwaitsGrading,
            OtherCourseInNeptun,
            MultipleCoursesInNeptun,
            NoTopicNoAdvisor,
            NoNeptunCourseNoTopic, // Why is the student in the list at all?
            NoGradingEntry,
            AwaitsGradingWithoutEnrollment  // Not graded but appears in the portal, but not enrolled in any courses in Neptun
        }

        private StatusEnum[] handledStatusCodes = new StatusEnum[] { StatusEnum.Success, StatusEnum.OtherCourseInNeptun,
            StatusEnum.NoTopicNoAdvisor, StatusEnum.NoNeptunCourseNoTopic, StatusEnum.AwaitsGradingWithoutEnrollment };

        public bool IsHandled => handledStatusCodes.Contains(Status);

        public string GetConsoleString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Status.ToString()} ({Student.Name}):");
            if (TargetCourseInNeptun != null)
                sb.Append($" Neptun:{TargetCourseInNeptun}");
            if (Grading != null)
                sb.Append($" Grading:{Grading}");
            if (Advisor != null)
                sb.Append($" Advisor:{Advisor}");
            if (OtherClassCode != null)
                sb.Append($" OtherClassCode:{OtherClassCode}");
            if (Details != null)
                sb.Append($" Details:{Details}");
            return sb.ToString();
        }
    }
}