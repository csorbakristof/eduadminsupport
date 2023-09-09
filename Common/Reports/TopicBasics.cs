using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Reports
{
    public class TopicBasics : SimpleTableReportBase
    {
        protected override string[] headers => new string[] { "Cím", "Hallgatószám", "Kapacitás", "Konzulensek" };

        protected override IEnumerable<string[]> GenerateLines(Context context)
        {
            return context.Topics.OrderByDescending(t=>t.RegisteredStudents?.Count)
                .Select(t => new string[] {
                    t.Title, t.RegisteredStudents?.Count.ToString(), t.SeatCount.ToString(),
                    string.Join(", ", t.Advisors.Select(a => a.Name))
                });
        }
    }
}
