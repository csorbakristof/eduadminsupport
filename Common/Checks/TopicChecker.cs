using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Checks
{
    public class TopicChecker
    {
        public IEnumerable<TopicError> Check(Topic t, Context context)
        {
            if (t.Advisors.Count() == 0)
                yield return new TopicError() { Message = "Topic has no advisors", Topic = t };
            if (t.CourseCategories.Count() == 0)
                yield return new TopicError() { Message = "Topic has no course categories", Topic = t };
        }
    }
}
