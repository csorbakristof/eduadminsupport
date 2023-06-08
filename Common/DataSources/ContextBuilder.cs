using Common.Helpers;
using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Common.DataSources
{
    public class ContextBuilder
    {
        const string TopicUrlPrefix = @"https://www.aut.bme.hu/Task/";
        public const string GradeImportFilenameSemesterPostfix = "2022_23_2";
        const bool RetrieveOnlyFirst5Topics = false;
        const string PortalGradingExportFilename = @"c:\_onlabFelugyeletAdatok\tema-osztalyzatok-22-23-tavasz.xlsx";
        const string PortalTerhelesExportFilename = @"c:\_onlabFelugyeletAdatok\Terheles_22-23-tavasz.xlsx";
        const string NeptunExportFilename = @"c:\_onlabFelugyeletAdatok\kurzusok_neptunExport.xlsx";
        const string NeptunCourseGradeImportFilesPath = @"c:\_onlabFelugyeletAdatok\";

        public async Task<Context> Build(ICourseCategorySource categorySource, string cacheFilename)
        {
            var context = await LoadFromCacheIfAvailable(cacheFilename);
            if (context == null)
            {
                context = await RetrieveContextFromDataSources(new CourseCategorySource());
                await SaveToCache(context, cacheFilename);
            }
            return context;
        }

        private Context context = new Context();

        private async Task<Context> RetrieveContextFromDataSources(ICourseCategorySource categorySource)
        {
            context = new Context();
            await GetCourseCategoriesAndCourses(categorySource);
            await GetTopicsAndAdvisorsFromTopicWebPages();
            await GetStudentsAndTopicRegistrationsFromSupervisionXlsx();
            await GetGradings();
            await GetEnrolledStudentsFromNeptun();

            return context;
        }

        #region Data retrieval methods working on this.context
        // Adds topics and advisors
        private async Task GetTopicsAndAdvisorsFromTopicWebPages()
        {
            Console.Out.WriteLine("Retrieving topics and advisors...");
            context.Advisors = new List<Advisor>();
            context.Topics = new List<Topic>();
            foreach (var cat in context.CourseCategories)
            {
                var urls = await cat.GetTopicUrls(TopicUrlPrefix);

                if (RetrieveOnlyFirst5Topics)
                {
                    urls = urls.Take(5);
                    await Console.Out.WriteLineAsync("WARNING: RetrieveOnlyFirst5Topics is ACTIVE!");
                }

                foreach (var url in urls)
                {
                    // Add topic if it is not present in the list yet...
                    if (!context.Topics.Any(t => t.Url == url))
                    {
                        Topic newTopic = await Topic.RetrieveFromWeb(url, context.Advisors);
                        newTopic.CourseCategories = new List<CourseCategory>();

                        context.Topics.Add(newTopic);
                    }

                    var topic = context.Topics.Single(t => t.Url == url);
                    topic.CourseCategories.Add(cat);
                }
            }
        }

        // Extends list of advisors, adds students, fills Student.TopicRegistrations
        private async Task GetStudentsAndTopicRegistrationsFromSupervisionXlsx()
        {
            Console.Out.WriteLine("Loading students (and matching advisors) based on portal 'Terheles'...");
            var src = new AdvisorLoadSource();
            foreach (var (advisor, topicTitle, studentName, studentNKod) in src.GetStudentNamesAndAdvisorList(PortalTerhelesExportFilename))
            {
                // Find advisor
                Advisor? a = context.Advisors.SingleOrDefault(a => a.Name == advisor);
                if (a == null)
                {
                    a = new Advisor(advisor);
                    context.Advisors.Add(a);
                    await Console.Out.WriteLineAsync($"WARNING: Advisor '{advisor}' was first found in AdvisorLoadSource, not in topic web pages!");
                }

                // Add student
                Student? s = context.Students.SingleOrDefault(s => s.NKod == studentNKod);
                if (s == null)
                {
                    s = new Student(studentName, studentNKod);
                    context.Students.Add(s);
                }

                // Find topic
                Topic? t = null;
                var topicsWithMatchingTitles = context.Topics.Where(t => t.Title == topicTitle).ToList();
                if (topicsWithMatchingTitles.Count == 1)
                {
                    t = topicsWithMatchingTitles[0];
                }
                else if (topicsWithMatchingTitles.Count == 0)
                {
                    Console.WriteLine("WARNING: Topic " + topicTitle + " not found (appeared in AdvisorLoadSource)");
                }
                else
                {
                    // Checking advisor name...
                    var topicsWithMatchingTitleAndAdvisor =
                        topicsWithMatchingTitles.Where(t => t.Advisors.Any(a => advisor.StartsWith(a.Name))).ToList();
                    if (topicsWithMatchingTitleAndAdvisor.Count == 1)
                        t = topicsWithMatchingTitleAndAdvisor[0];
                    else
                    {
                        Console.WriteLine("INFO (Topic duplicate with same advisor): '" + topicTitle + "', advisor " + advisor);
                        t = topicsWithMatchingTitleAndAdvisor[0];   // They seem to be equivalent in all important aspects...
                    }
                }

                s.TopicRegistrations.Add((a, t));
            }
        }

        private async Task GetCourseCategoriesAndCourses(ICourseCategorySource categorySource)
        {
            Console.Out.WriteLine("Retrieving course categories...");
            context.CourseCategories.AddRange(categorySource.GetCourseCategories());

            Console.Out.WriteLine("Retrieving courses based on Neptun...");
            var neptunCourseSource = new NeptunCourseSource();
            var coursesFromNeptun = neptunCourseSource.GetCourses(NeptunExportFilename).ToList();
            // This is used as a complete list of all courses under the course codes.

            Console.Out.WriteLine("Collecting courses for course categories...");
            foreach (var category in context.CourseCategories)
            {
                await category.CollectCoursesOfCourseCategoryBasedOnCourseCategoryWebPages(coursesFromNeptun);
                // Note: there are courses which appear in multiple course categories...
                foreach(var c in category.Courses)
                    if (!context.Courses.Contains(c))
                        context.Courses.Add(c);
            }
        }

        // Fills Course.EnrolledStudentNKodsFromNeptun, does not add Student objects
        private async Task GetEnrolledStudentsFromNeptun()
        {
            // Load JEGYIMPORT xlsx files
            foreach (Course c in context.Courses)
            {
                var xlsxFilename = NeptunCourseGradeImportFilesPath + c.GradeImportFilename(GradeImportFilenameSemesterPostfix);
                var xlsxReader = new Excel2Dict();
                if (System.IO.File.Exists(xlsxFilename))
                {
                    var lines = xlsxReader.Read(xlsxFilename, 0, 1);
                    c.EnrolledStudentNKodsFromNeptun = new List<string>();
                    foreach (var line in lines)
                    {
                        var nkod = line["Neptun kód"];
                        c.EnrolledStudentNKodsFromNeptun.Add(nkod);
                    }
                }
                else
                {
                    await Console.Out.WriteLineAsync($"WARNING: No JEGYIMPORT file for course {c}");
                }
            }
        }

        // Fills Context.Gradings
        private async Task GetGradings()
        {
            var src = new GradingSource();
            var grades = src.GetGrades(PortalGradingExportFilename).ToList();
            foreach (var line in grades)
            {
                var g = new Grading() { StudentNKodFromGrading = line.StudentNKod, ClassCodeInGrading = line.ClassCode, Grade = line.Grade };
                context.Gradings.Add(g);
            }
        }
        #endregion

        #region Caching
        public async Task SaveToCache(Context context, string cacheFilename)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(Context));
            Console.WriteLine("DataContractSerializer.PreserveObjectReferences: " + serializer.PreserveObjectReferences);
            XmlWriter writer = XmlWriter.Create(cacheFilename);
            serializer.WriteObject(writer, context);
            writer.Close();
        }

        public async Task<Context?> LoadFromCacheIfAvailable(string cacheFilename)
        {
            if (!System.IO.File.Exists(cacheFilename))
                return null;
            DataContractSerializer serializer = new DataContractSerializer(typeof(Context));
            XmlReader reader = XmlReader.Create(cacheFilename);
            Context c = (Context)serializer.ReadObject(reader);
            reader.Close();
            return c;
        }
        #endregion


    }
}
