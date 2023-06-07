using Common.DataSources;
using Common.Model;

namespace OnlabStats
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var c = await Context.RetrieveContextFromDataSources(new CourseCategorySource());

            c.PerformBaseChecks();
        }
    }
}
