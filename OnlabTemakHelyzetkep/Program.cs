using static System.Net.WebRequestMethods;
using System.Linq;

namespace OnlabTemakHelyzetkep
{
    internal class Program
    {
        struct Course
        {
            public string Title;
            public string Url;
        }

        Course[] Courses = new Course[]
        {
                new Course() { Title = "Onlab BSc Info", Url = "https://www.aut.bme.hu/Education/BScInfo/Onlab"},
                new Course() { Title="Szakdolgozat BSc Info", Url = "https://www.aut.bme.hu/Education/BScInfo/Szakdolgozat" },
        };

        List<Topic> Topics = new List<Topic>();

        static async Task Main(string[] args)
        {
            Program p = new Program();

            // Load current advisor data from excel (exported from the departments portal)
            Excel2Dict e = new Excel2Dict();
            var advisorTable = e.Read(@"c:\temp\Terheles_22-23-tavasz.xlsx", 1, 1);
            // Load topic data from the web
            await p.RetrieveData();

            // Combine information
            p.AddCurrentAdvisorData(advisorTable);


            // Show stats
            p.ShowStats();


        }

        private void AddCurrentAdvisorData(List<Dictionary<string, string>> advisorTable)
        {
            // For each topic we search how many students are on it currently
            foreach(var topic in Topics)
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
            foreach (var course in Courses)
            {
                var topicUrls = await retriever.GetTopicUrlList(course.Url);

                foreach (var topicUrl in topicUrls.Take(10))    // ------------------------------ WARNING, limiting to 20 topic per course!
                {
                    var fullUrl = "https://www.aut.bme.hu/Task/" + topicUrl;

                    Topic topic = Topics.SingleOrDefault(t => t.Url == fullUrl);
                    if (topic.Title == null)
                    {
                        topic = await retriever.GetTopic(fullUrl);
                        topic.Url = fullUrl;
                        topic.Courses = new List<string>() { course.Title };
                        topic.StudentNKods = new List<string>();
                        Console.WriteLine($"New topic: {topic.Title} (limit {topic.MaxStudentCount})");
                        Topics.Add(topic);
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
            Console.WriteLine($"Topic count: {Topics.Count()}");
            Console.WriteLine($"External topic count: {Topics.Count(t=>t.IsExternal)}");
            Console.WriteLine($"Topics with multiple advisors: {Topics.Count(t => t.Advisors.Count > 1)}");
            Console.WriteLine($"Topics with multiple courses: {Topics.Count(t => t.Courses.Count > 1)}");

            Console.WriteLine($"Total capacity for students: {Topics.Sum(t => t.MaxStudentCount)}");
            Console.WriteLine($"Total free capacity for students: {Topics.Sum(t => t.MaxStudentCount - t.StudentNKods.Count)}");
            Console.WriteLine($"Number of topics with free seats: {Topics.Count(t => t.MaxStudentCount > t.StudentNKods.Count)}");

        }
    }
}
