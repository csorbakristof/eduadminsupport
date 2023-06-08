using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Checks
{
    public class CourseChecker
    {
        public IEnumerable<CourseError> Check(Course c, Context context)
        {
            if (context.CourseCategories.Any(cc => cc.Courses == null))
                throw new ModelInconsistencyException("Not all course categories have their CourseCategory.Courses set");

            if (c.EnrolledStudentCountInNeptun != c.EnrolledStudentNKodsFromNeptun.Count)
            {
                // We may be running the application without detailed export for all Neptun courses.
                if (context.CourseCategories.Any(cc => cc.Courses.Contains(c)) && c.EnrolledStudentNKodsFromNeptun.Count>0) // Otherwise we do not care...
                    throw new ModelInconsistencyException("Course's student count and length of student list does not match in Neptun!");
            }

            // Check whether the course belongs to exactly one course category
            var cCats = context.CourseCategories.Where(cc => cc.Courses.Contains(c)).ToArray();
            if (cCats.Length == 0)
            {
                yield return new CourseError() { Message = $"Course belongs to zero course categories. Should not appear here... ", Course = c };
            }
            else if (cCats.Length > 1)
            {
                yield return new CourseError() { Message = $"Course belongs to multiple course categories: "
                    + String.Join(',',cCats.Select(s=>s.ToString())), Course = c };
            }
        }
    }
}
