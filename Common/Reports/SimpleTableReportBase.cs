using Common.Helpers;
using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Reports
{
    public abstract class SimpleTableReportBase
    {
        protected abstract string[] headers { get; }
        protected abstract IEnumerable<string[]> GenerateLines(Context context);

        public void GenerateReport(Context context, string filename)
        {
            GenericExcelWriter.WriteToNewExcelFile(filename, headers, GenerateLines(context));
        }
    }
}
