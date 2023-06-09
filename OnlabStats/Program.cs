using Common.Checks;
using Common.DataSources;
using Common.Helpers;
using Common.Model;
using Common.Reports;
using System.Reflection.PortableExecutable;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OnlabStats
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const string cacheFilename = @"c:\temp\contextcache.xml";
            const string LastGradingOutputFilename = @"c:\_onlabFelugyeletAdatok\lastGradingOutput.xml";
            Context context = await (new ContextBuilder()).Build(new CourseCategorySource(), cacheFilename);
            const string StatusExcelFilename = @"c:\temp\status.xlsx";

            foreach (var s in context.Students)
                s.EnrolledCourses = context.Courses.Where(c => c.EnrolledStudentNKodsFromNeptun.Contains(s.NKod)).ToList();

            foreach(var g in context.Gradings)
            {
                var s = context.Students.Where(s => s.NKod == g.StudentNKodFromGrading).SingleOrDefault();
                if (s != null)
                    s.Gradings.Add(g);
            }

            foreach(var s in context.Students)
                foreach((Advisor a, Topic t) in s.TopicRegistrations)
                {
                    if (t.RegisteredStudents == null)
                        t.RegisteredStudents = new List<Student>();
                    t.RegisteredStudents.Add(s);
                }

            await Console.Out.WriteLineAsync("---------- Topic reports ---------------");

            var stat = new TopicAvailability();
            stat.ShowFreeAndTotalAndRequiredSeatsPerCourseCategory(context);

            await Console.Out.WriteLineAsync("---------- Running checks ---------------");

            var errors = RunChecks(context);
            foreach (var e in errors)
                Console.WriteLine(e);

            await Console.Out.WriteLineAsync("---------- Collecting grades ---------------");

            var grader = new GradingsCleanedForNeptun();
            grader.CreateGradings(context);

            var skippedStatusCodes = new GradingStatus.StatusEnum[] {
                GradingStatus.StatusEnum.Success, GradingStatus.StatusEnum.AwaitsGrading, GradingStatus.StatusEnum.OtherCourseInNeptun,
                GradingStatus.StatusEnum.AwaitsGradingWithoutEnrollment};
            await Console.Out.WriteLineAsync($"Grading statuses (not showing {string.Join(',', skippedStatusCodes)}):");
            foreach (var status in grader.GetStatuses())
                if (!skippedStatusCodes.Contains(status.Status))
                    Console.WriteLine(status.GetConsoleString());

            //grader.GenerateExcelFiles(@"c:\temp\delme\");
            //grader.SaveGradingOutput(LastGradingOutputFilename);

            await Console.Out.WriteLineAsync("---------- Student errors (from above) with grading status ---------------");
            foreach(var e in errors)
            {
                if (e is StudentError)
                {
                    Student s = (e as StudentError).Student;
                    await Console.Out.WriteLineAsync($"--- Student {s.Name} ---");
                    await Console.Out.WriteLineAsync($"Error: {e}");
                    await Console.Out.WriteLineAsync($"Grading status: {grader.GetStatuses().SingleOrDefault(gs=>gs.Student==s)?.GetConsoleString()}");
                }
            }

            WriteUngradedStudentsToExcel(StatusExcelFilename, grader.GetStatuses());
        }

        private static List<ErrorBase> RunChecks(Context context)
        {
            context.PerformBaseChecks();

            List<ErrorBase> errors = new List<ErrorBase>();

            var studentChecker = new StudentChecker();
            foreach (var s in context.Students)
                errors.AddRange(studentChecker.Check(s, context));

            var gradingChecker = new GradingChecker();
            foreach (var g in context.Gradings)
                errors.AddRange(gradingChecker.Check(g, context));

            var courseChecker = new CourseChecker();
            foreach (var c in context.Courses)
                errors.AddRange(courseChecker.Check(c, context));

            var topicChecker = new TopicChecker();
            foreach (var t in context.Topics)
                errors.AddRange(topicChecker.Check(t, context));

            return errors;
        }

        private static void WriteUngradedStudentsToExcel(string filename, IEnumerable<GradingStatus> statuses)
        {
            using(GenericExcelWriter writer = new GenericExcelWriter(filename))
            {
                foreach(var status in statuses.Where(s=>s.Status == GradingStatus.StatusEnum.AwaitsGrading))
                {
                    writer.AppendRow(new string[] { status.Student.Name, status.Student.NKod, status.Status.ToString(), status.Advisor?.Name });
                }
            }
        }
    }
}
