using static System.Net.WebRequestMethods;
using System.Linq;

namespace OnlabTemakHelyzetkep
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            (string,string)[] courseTopicSummaryPageUrls = new (string,string)[]
            {
                
                ("https://www.aut.bme.hu/Education/BScInfo/Onlab", "Onlab BSc Info"),
                ("https://www.aut.bme.hu/Education/BScInfo/Szakdolgozat", "Szakdolgozat BSc Info")
            };

            var retriever = new TopicRetriever();
            var allTopics = new List<Topic>();
            foreach (var courseUrl in courseTopicSummaryPageUrls)
            {
                var topicUrls = await retriever.GetTopicUrlList(courseUrl.Item1);

                foreach (var topicUrl in topicUrls.Take(20))
                {
                    var fullUrl = "https://www.aut.bme.hu/Task/" + topicUrl;

                    Topic topic = allTopics.SingleOrDefault(t => t.Url == fullUrl);
                    if (topic.Title == null)
                    {
                        topic = await retriever.GetTopic(fullUrl);
                        topic.Url = fullUrl;
                        topic.Courses = new List<string>() { courseUrl.Item2 };
                        Console.WriteLine($"New topic: {topic.Title} (limit {topic.MaxStudentCount})");
                        allTopics.Add(topic);
                    }
                    else
                    {
                        topic.Courses.Add(courseUrl.Item2);
                        Console.WriteLine($"Extended topic: {topic.Title} (limit {topic.MaxStudentCount})");
                    }

                }
            }

            ShowStats(allTopics);
        }

        private static void ShowStats(IEnumerable<Topic> topics)
        {
            Console.WriteLine($"Topic count: {topics.Count()}");
            Console.WriteLine($"External topic count: {topics.Count(t=>t.IsExternal)}");
            Console.WriteLine($"Topics with multiple advisors: {topics.Count(t => t.Advisors.Count > 1)}");
            Console.WriteLine($"Topics with multiple courses: {topics.Count(t => t.Courses.Count > 1)}");
        }
    }
}
