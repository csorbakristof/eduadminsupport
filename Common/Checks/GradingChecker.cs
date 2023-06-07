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
        }
    }
}
