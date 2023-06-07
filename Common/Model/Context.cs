using Common.DataSources;
using Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Common.Model
{
    [DataContract]
    public class Context
    {
        [DataMember]
        public List<CourseCategory> CourseCategories { get; set; }
        [DataMember]
        public List<Course> Courses { get; set; }
        [DataMember]
        public List<Advisor> Advisors { get; set; } = new List<Advisor>();
        [DataMember]
        public List<Topic> Topics { get; set; } = new List<Topic>();
        [DataMember]
        public List<Student> Students { get; set; } = new List<Student>();
        [DataMember]
        public List<Grading>? Gradings { get; set; }
        [DataMember]
        public List<PresentationSessionType>? PresentationSessionTypes { get; set; }

        const string TopicUrlPrefix = @"https://www.aut.bme.hu/Task/";
        public const string GradeImportFilenameSemesterPostfix = "2022_23_2";
        const bool RetrieveOnlyFirst5Topics = true;

        public static async Task<Context> RetrieveContextFromDataSources(ICourseCategorySource categorySource)
        {
            var context = new Context();
            await GetCourseCategoriesAndCourses(categorySource, context);
            await GetTopicsAndAdvisors(context);
            await GetStudentsAndTopicRegistrations(context);
            await GetGradings(context);
            await GetEnrolledStudentsFromNeptun(context);

            return context;
        }

        #region Caching
        public async Task SaveToCache(string cacheFilename)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(Context));
            XmlWriter writer = XmlWriter.Create(cacheFilename);
            serializer.WriteObject(writer, this);
            writer.Close();
        }

        public static async Task<Context?> LoadFromCacheIfAvailable(string cacheFilename)
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

        #region Data retrieval methods
        private static async Task GetTopicsAndAdvisors(Context context)
        {
            Console.Out.WriteLine("Retrieving topics and advisors...");
            context.Advisors = new List<Advisor>();
            context.Topics = new List<Topic>();
            foreach (var cat in context.CourseCategories)
            {
                var urls = await cat.GetTopicUrls(TopicUrlPrefix);

                if (RetrieveOnlyFirst5Topics)
                    urls = urls.Take(5);

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

        private static async Task GetStudentsAndTopicRegistrations(Context context)
        {
            Console.Out.WriteLine("Loading students (and matching advisors) based on portal 'Terheles'...");
            var src = new AdvisorLoadSource();
            foreach (var (advisor, topicTitle, studentName, studentNKod) in src.GetStudentNamesAndAdvisorList())
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
                        Console.WriteLine("WARNING: Topic " + topicTitle + " has multiple instances with advisor name " + advisor);
                        t = topicsWithMatchingTitleAndAdvisor[0];   // They seem to be equivalent in all important aspects...
                    }
                }

                if (s.TopicRegistrations == null)
                    s.TopicRegistrations = new List<(Advisor, Topic)>();
                s.TopicRegistrations.Add((a, t));
            }
        }

        private static async Task GetCourseCategoriesAndCourses(ICourseCategorySource categorySource, Context context)
        {
            Console.Out.WriteLine("Retrieving course categories...");
            context.CourseCategories = categorySource.GetCourseCategories();

            Console.Out.WriteLine("Retrieving courses based on Neptun...");
            var neptunCourseSource = new NeptunCourseSource();
            context.Courses = neptunCourseSource.GetCourses().ToList();

            Console.Out.WriteLine("Collecting courses for course categories...");
            foreach (var category in context.CourseCategories)
            {
                await category.GetCourses(context.Courses);
            }
        }

        private static async Task GetEnrolledStudentsFromNeptun(Context context)
        {
            // Load JEGYIMPORT xlsx files
            foreach (Course c in context.Courses)
            {
                var xlsxFilename = @"c:\_onlabFelugyeletAdatok\" + c.GradeImportFilename(Context.GradeImportFilenameSemesterPostfix);
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

        private static async Task GetGradings(Context context)
        {
            var src = new GradingSource();
            var grades = src.GetGrades().ToList();
            context.Gradings = new List<Grading>();
            foreach (var line in grades)
            {
                var g = new Grading() { StudentNKodFromGrading = line.StudentNKod, ClassCodeInGrading = line.ClassCode, Grade = line.Grade };
                context.Gradings.Add(g);
            }
        }

        #endregion

        public void PerformBaseChecks()
        {
            if (RetrieveOnlyFirst5Topics)
                Console.WriteLine("WARNING: RetrieveOnlyFirst10Topics active");

            // All topics have at least one advisor and course category
            foreach(var t in Topics)
            {
                if (t.Advisors.Count() == 0)
                    Console.WriteLine("WARNING: Topic " + t.Title + " has no advisors");

                if (t.CourseCategories.Count() == 0)
                    Console.WriteLine("WARNING: Topic " + t.Title + " has no course categories");
            }

            // There are students, advisors, topics and courses in the context
            if (Students.Count() == 0)
                Console.WriteLine("WARNING: No Students in context");
            if (Advisors.Count() == 0)
                Console.WriteLine("WARNING: No Advisors in context");
            if (CourseCategories.Count() == 0)
                Console.WriteLine("WARNING: No CourseCategories in context");
            if (Courses.Count() == 0)
                Console.WriteLine("WARNING: No Courses in context");
            if (Gradings == null || Gradings?.Count() == 0)
                Console.WriteLine("WARNING: No Gradings in context");
        }
    }
}
