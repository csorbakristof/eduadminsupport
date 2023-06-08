using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Checks
{
    public class GradingChecker
    {
        public IEnumerable<GradingError> Check(Grading g, Context context)
        {
            if (g.StudentNKodFromGrading == string.Empty)
                yield return new GradingError() { Message = "Grading.StudentNKodFromGrading not set", Grading = g };

            int studentCount = context.Students.Count(s => s.NKod == g.StudentNKodFromGrading);
            if (studentCount == 0)
            {
                var courses = context.Courses.Where(c=>c.EnrolledStudentNKodsFromNeptun.Contains(g.StudentNKodFromGrading));
                yield return new GradingError() { Message = $"Grading.StudentNKodFromGrading not found in Students, enrolled in: [{string.Join(',',courses)}]", Grading = g };
            }
            else if (studentCount > 1)
                throw new ModelInconsistencyException($"Grading.StudentNKodFromGrading {g.StudentNKodFromGrading} belongs to multiple students!");
        }
    }
}
