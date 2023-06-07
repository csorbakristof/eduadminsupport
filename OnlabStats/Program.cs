using Common.Checks;
using Common.DataSources;
using Common.Helpers;
using Common.Model;
using System.Reflection.PortableExecutable;

namespace OnlabStats
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const string cacheFilename = @"c:\temp\contextcache.xml";
            Context? context = null;
            context = await Context.LoadFromCacheIfAvailable(cacheFilename);
            if (context == null)
            {
                context = await Context.RetrieveContextFromDataSources(new CourseCategorySource());
                await context.SaveToCache(cacheFilename);
            }

            foreach (var s in context.Students)
                s.EnrolledCourses = context.Courses.Where(c => c.EnrolledStudentNKodsFromNeptun.Contains(s.NKod)).ToList();

            foreach(var g in context.Gradings)
            {
                var s = context.Students.Where(s => s.NKod == g.StudentNKodFromGrading).SingleOrDefault();
                if (s != null)
                    s.Gradings.Add(g);
            }

            context.PerformBaseChecks();

            List<ErrorBase> errors = new List<ErrorBase>();

            var studentChecker = new StudentChecker();
            foreach(var s in context.Students)
                errors.AddRange(studentChecker.Check(s, context));

            var gradingChecker = new GradingChecker();
            foreach(var g in context.Gradings)
                errors.AddRange(gradingChecker.Check(g, context));

            var courseChecker = new CourseChecker();
            foreach(var c in context.Courses)
                errors.AddRange(courseChecker.Check(c, context));

            foreach(var e in errors)
                Console.WriteLine(e);
        }
    }
}
