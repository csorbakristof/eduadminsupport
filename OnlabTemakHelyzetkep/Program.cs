using static System.Net.WebRequestMethods;
using System.Linq;
using System.Text.RegularExpressions;
using OfficeOpenXml.FormulaParsing.ExpressionGraph;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;

namespace OnlabTemakHelyzetkep
{
    public class Program
    {
        public class Course
        {
            public string Title;
            public string Url;
            public string[] CourseCodes;
            public int enrolledHungarianStudentCount;
            public int enrolledEnglishStudentCount;
        }

        public class DepartmentPortalInfoContext
        {
            public List<Course> Courses;
            public List<Topic> Topics;
        }

        public DepartmentPortalInfoContext Context;

        static async Task Main(string[] args)
        {
            Program p = new Program();

            p.Context = new DepartmentPortalInfoContext() { Topics = new List<Topic>() };
            p.Context.Courses = new List<Course>()
            {
                new Course() { Title = "Onlab BSc Info", Url = "https://www.aut.bme.hu/Education/BScInfo/Onlab"},
                new Course() { Title="Témalabor BProf", Url = "https://www.aut.bme.hu/Education/BProf/Temalabor" },
                new Course() { Title="Önlab BSc Villany", Url = "https://www.aut.bme.hu/Education/BScVillany/Onlab" },
                new Course() { Title="Szakdolgozat BSc Villany", Url = "https://www.aut.bme.hu/Education/BScVillany/Szakdolgozat" },
                new Course() { Title="Témalabor BSc Info", Url = "https://www.aut.bme.hu/Education/BScInfo/Temalabor" },
                new Course() { Title="Önlab BSc Info", Url = "https://www.aut.bme.hu/Education/BScInfo/Onlab" },
                new Course() { Title="Szakdolgozat BSc Info", Url = "https://www.aut.bme.hu/Education/BScInfo/Szakdolgozat" },
                new Course() { Title="Önlab MSc Villany", Url = "https://www.aut.bme.hu/Education/MScVillany/Onlab" },
                new Course() { Title="Dipterv MSc Villany", Url = "https://www.aut.bme.hu/Education/MScVillany/Diploma" },
                new Course() { Title="Önlab MSc Info", Url = "https://www.aut.bme.hu/Education/MScInfo/Onlab" },
                new Course() { Title="Dipterv MSc Info", Url = "https://www.aut.bme.hu/Education/MScInfo/Diploma" },
                new Course() { Title="Önlab MSc Mecha", Url = "https://www.aut.bme.hu/Education/MScMechatronika/Onlab" },
                new Course() { Title="Dipterv MSc Mecha", Url = "https://www.aut.bme.hu/Education/MScMechatronika/Diploma" }
            };

            // Load current advisor data from excel (exported from the departments portal)
            Excel2Dict e = new Excel2Dict();
            var advisorTable = e.Read(@"c:\temp\Terheles_22-23-tavasz.xlsx", 1, 1);

            // Load current student counts on the courses (exported from Neptun)
            var courseEnrolledStudentCounts = e.Read(@"c:\temp\kurzusok_neptunExport.xlsx", 0, 1);

            // Load topic data from the web
            await p.RetrieveData();

            // Combine information
            p.AddCurrentAdvisorData(advisorTable);
            p.AddCourseEnrolledStudentCounts(courseEnrolledStudentCounts);

            // Show stats
            p.ShowStats();

            XmlSerializer xml = new XmlSerializer(typeof(DepartmentPortalInfoContext));
            xml.Serialize(new StreamWriter(@"c:\temp\data.xml"), p.Context);
            Console.WriteLine("Downloaded data saved.");
        }

        private void AddCourseEnrolledStudentCounts(List<Dictionary<string, string>> courseEnrolledStudentCounts)
        {
            Regex getEnrolledStudentCount = new Regex(@"(\d+)/\d+/\d+");
            for (int i=0; i<Context.Courses.Count; i++)
            {
                var currentCourse = Context.Courses[i];
                foreach(string courseCode in currentCourse.CourseCodes)
                {
                    // Look for all entries in Neptun for this course code. There may be hungarian and english courses as well under the same code.
                    foreach(var neptunEntryLine in courseEnrolledStudentCounts)
                    {
                        if (neptunEntryLine["Tárgykód"] == "BME"+ courseCode)
                        {
                            var course = neptunEntryLine["Kurzus kód"];
                            var enrollmentData = neptunEntryLine["Létszám"];
                            var enrolledStudentCountString = getEnrolledStudentCount.Match(enrollmentData).Groups[1].Value;
                            int studentCount = int.Parse(enrolledStudentCountString);
                            if (course.StartsWith('A'))
                                currentCourse.enrolledEnglishStudentCount += studentCount;
                            else
                                currentCourse.enrolledHungarianStudentCount += studentCount;
                        }

                    }

                }
            }
        }

        private void AddCurrentAdvisorData(List<Dictionary<string, string>> advisorTable)
        {
            // For each topic we search how many students are on it currently
            foreach(var topic in Context.Topics)
            {
                Console.WriteLine($"Searching for students on topic: {topic.Title}");
                foreach(var supervision in advisorTable)
                {
                    if (supervision["Téma címe"] == topic.Title)
                        topic.StudentNKods.Add(supervision["Hallg. nept"]);
                }
            }
        }

        async Task RetrieveData()
        {
            var retriever = new TopicRetriever();
            for(int i=0; i< Context.Courses.Count; i++)
            {
                var course = Context.Courses[i];
                var topicUrls = await retriever.GetTopicUrlList(course.Url);
                course.CourseCodes = await retriever.GetCourseCodes(course.Url);

                foreach (var topicUrl in topicUrls.Take(5))    // ------------------------------ WARNING, limiting to 20 topic per course!
                {
                    var fullUrl = "https://www.aut.bme.hu/Task/" + topicUrl;

                    Topic topic = Context.Topics.SingleOrDefault(t => t.Url == fullUrl);
                    if (topic.Title == null)
                    {
                        topic = await retriever.GetTopic(fullUrl);
                        topic.Url = fullUrl;
                        topic.Courses = new List<string>() { course.Title };
                        topic.StudentNKods = new List<string>();
                        Console.WriteLine($"New topic: {topic.Title} (limit {topic.MaxStudentCount})");
                        Context.Topics.Add(topic);
                    }
                    else
                    {
                        topic.Courses.Add(course.Title);
                        Console.WriteLine($"Extended topic: {topic.Title} (limit {topic.MaxStudentCount})");
                    }

                }
            }
        }

        private void ShowStats()
        {
            Console.WriteLine($"Topic count: {Context.Topics.Count()}");
            Console.WriteLine($"External topic count: {Context.Topics.Count(t=>t.IsExternal)}");
            Console.WriteLine($"Topics with multiple advisors: {Context.Topics.Count(t => t.Advisors.Count > 1)}");
            Console.WriteLine($"Topics with multiple courses: {Context.Topics.Count(t => t.Courses.Count > 1)}");

            Console.WriteLine($"Total capacity for students: {Context.Topics.Sum(t => t.MaxStudentCount)}");
            Console.WriteLine($"Total free capacity for students: {Context.Topics.Sum(t => t.MaxStudentCount - t.StudentNKods.Count)}");
            Console.WriteLine($"Number of topics with free seats: {Context.Topics.Count(t => t.MaxStudentCount > t.StudentNKods.Count)}");

        }
    }
}
