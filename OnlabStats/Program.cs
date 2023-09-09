using Common.Checks;
using Common.DataSources;
using Common.Helpers;
using Common.Model;
using Common.Reports;
using OfficeOpenXml.LoadFunctions.Params;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OnlabStats
{
    internal class Program
    {
        const string cacheFilename = @"c:\_onlabFelugyeletAdatok\contextcache.xml";
        const string LastGradingOutputFilename = @"c:\_onlabFelugyeletAdatok\lastGradingOutput.xml";
        const string StatusExcelFilename = @"c:\_onlabFelugyeletAdatok\status.xlsx";
        const string AdvisorStatsXlsFilename = @"c:\_onlabFelugyeletAdatok\AdvisorCapacityReport.xlsx";
        const string TopicBasicsReportFilename = @"c:\_onlabFelugyeletAdatok\TopicBasicsReport.xlsx";
        const string StudentListWithNoTopicRegistrations = @"c:\_onlabFelugyeletAdatok\StudentsWithoutTopicsReport.xlsx";

        const bool ExpectGradings = false;  // false at the beginning of the term

        private Context context;

        static async Task Main(string[] args)
        {
            var p = new Program();
            await p.LoadDataAsync();
            await p.ShowAdvisorCapacityStats();

            await p.ShowTopicReports();

            await p.RunChecks();
            if (ExpectGradings)
            {
                await p.CollectGradesAndGenerateGradingOutputFiles();
                await p.ShowErrorsWithGradingStatus();
            }
        }

        private async Task ShowAdvisorCapacityStats()
        {
            await Console.Out.WriteLineAsync("---------- Advisor capacity stats ---------------");

            var stat = new AdvisorCapacityAndLoad();
            stat.ShowAdvisorCapacityAndLoad(context, AdvisorStatsXlsFilename);
        }

        private async Task LoadDataAsync()
        {
            context = await(new ContextBuilder()).Build(new CourseCategorySource(), cacheFilename);

            foreach (var s in context.Students)
                s.EnrolledCourses = context.Courses.Where(c => c.EnrolledStudentNKodsFromNeptun.Contains(s.NKod)).ToList();

            foreach (var g in context.Gradings)
            {
                var s = context.Students.Where(s => s.NKod == g.StudentNKodFromGrading).SingleOrDefault();
                if (s != null)
                    s.Gradings.Add(g);
            }

            foreach (var s in context.Students)
                foreach ((Advisor a, Topic t) in s.TopicRegistrations)
                {
                    if (t.RegisteredStudents == null)
                        t.RegisteredStudents = new List<Student>();
                    // Topics with multiple advisor should not add student multiple times!
                    if (!t.RegisteredStudents.Contains(s))
                        t.RegisteredStudents.Add(s);
                }
        }

        private async Task ShowTopicReports()
        {
            await Console.Out.WriteLineAsync("---------- Topic reports ---------------");

            var stat = new TopicAvailability();
            stat.ShowFreeAndTotalAndRequiredSeatsPerCourseCategory(context);

            var topicBasics = new TopicBasics();
            topicBasics.GenerateReport(context, TopicBasicsReportFilename);

            var n = context.Courses.Sum(e => e.EnrolledStudentCountInNeptun);
            await Console.Out.WriteLineAsync($"Total enrolled student count: {n}");
        }

        private List<ErrorBase> errors;
        private async Task RunChecks()
        {
            await Console.Out.WriteLineAsync("---------- Running checks ---------------");

            errors = RunChecks(ExpectGradings);

            foreach (var e in errors)
                Console.WriteLine(e);

            var studentsWithNoTopicReg = errors.Where(e => e.Message.Contains("Student has no topic registrations")).Select(e=>((StudentError)e).Student);
            GenericExcelWriter.WriteToNewExcelFile(StudentListWithNoTopicRegistrations,
                new string[] { "Course name", "Course code", "Student name", "NKod" },
                studentsWithNoTopicReg.Select(s => new string[] {
                    s.EnrolledCourses.Single().Name,
                    s.EnrolledCourses.Single().ToString(),
                    s.Name, s.NKod }));
        }

        private GradingsCleanedForNeptun grader;
        private async Task CollectGradesAndGenerateGradingOutputFiles()
        {
            await Console.Out.WriteLineAsync("---------- Collecting grades ---------------");

            grader = new GradingsCleanedForNeptun();
            grader.CreateGradings(context);

            var skippedStatusCodes = new GradingStatus.StatusEnum[] {
                GradingStatus.StatusEnum.Success, GradingStatus.StatusEnum.AwaitsGrading, GradingStatus.StatusEnum.OtherCourseInNeptun,
                GradingStatus.StatusEnum.AwaitsGradingWithoutEnrollment};
            await Console.Out.WriteLineAsync($"Grading statuses (not showing {string.Join(',', skippedStatusCodes)}):");
            foreach (var status in grader.GetStatuses())
                if (!skippedStatusCodes.Contains(status.Status))
                    Console.WriteLine(status.GetConsoleString());

            grader.GenerateExcelFiles(@"c:\temp\delme\");
            grader.SaveGradingOutput(LastGradingOutputFilename);
        }

        private async Task ShowErrorsWithGradingStatus()
        {
            await Console.Out.WriteLineAsync("---------- Student errors (from above) with grading status ---------------");
            foreach (var e in errors)
            {
                if (e is StudentError)
                {
                    Student s = (e as StudentError).Student;
                    await Console.Out.WriteLineAsync($"--- Student {s.Name} ---");
                    await Console.Out.WriteLineAsync($"Error: {e}");
                    await Console.Out.WriteLineAsync($"Grading status: {grader.GetStatuses().SingleOrDefault(gs => gs.Student == s)?.GetConsoleString()}");
                }
            }

            WriteUngradedStudentsToExcel(StatusExcelFilename, grader.GetStatuses());
        }

        private List<ErrorBase> RunChecks(bool expectGradings)
        {
            context.PerformBaseChecks();

            List<ErrorBase> errors = new List<ErrorBase>();

            var studentChecker = new StudentChecker();
            foreach (var s in context.Students)
                errors.AddRange(studentChecker.Check(s, context));

            if (!expectGradings)
            {
                var gradingChecker = new GradingChecker();
                foreach (var g in context.Gradings)
                    errors.AddRange(gradingChecker.Check(g, context));
            }

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
