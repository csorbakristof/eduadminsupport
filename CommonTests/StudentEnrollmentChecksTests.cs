using Common.Checks;

namespace CommonTests
{
    public class StudentEnrollmentChecksTests
    {
        [Fact]
        public void StudentInGradingButNotInCourseEnrollments()
        {
            // This means the student is graded in the portal but is not enrolled in any courses in Neptun.
            var builder = new ContextBuilder();
            var context = builder.Build();

            var checker = new StudentEnrollmentChecks();

            Assert.NotEmpty(checker.CheckGradedStudentsWithoutNeptunEnrollment(context));

        }
    }
}