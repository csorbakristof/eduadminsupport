using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Checks
{
    public class StudentEnrollmentChecks
    {
        public IEnumerable<GradingError> CheckGradedStudentsWithoutNeptunEnrollment(Context context)
        {
            foreach(var g in context.Gradings)
            {
                string nkod = g.StudentNKodFromGrading;
                int enrollmentCount = context.Courses.Count(c => c.EnrolledStudentNKodsFromNeptun.Contains(nkod));
                if (enrollmentCount==0)
                {
                    yield return new GradingError() { Message = "Student not enrolled in any courses in Neptun", Grading = g };
                }
            }
        }
    }
}
