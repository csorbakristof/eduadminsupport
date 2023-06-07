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

            // Check whether the course belongs to exactly one course category
            if (context.CourseCategories.Where(cc => cc.Courses.Contains(c)).Count() != 1)
                yield return new CourseError() { Message = $"Course belongs to zero or multiple course categories.", Course = c };
        }
    }
}
