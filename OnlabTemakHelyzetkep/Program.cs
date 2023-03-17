namespace OnlabTemakHelyzetkep
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            (string,string)[] courseTopicSummaryPageUrls = new (string,string)[]
            {
                ("https://www.aut.bme.hu/Education/BScInfo/Szakdolgozat", "Szakdolgozat BSc Info")
            };

            var retriever = new TopicRetriever();
            var allTopics = new List<Topic>();
            foreach (var courseUrl in courseTopicSummaryPageUrls)
            {
                var topicUrls = await retriever.GetTopicUrlList(courseUrl.Item1);

                foreach (var topicUrl in topicUrls)
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
        }
    }
}
