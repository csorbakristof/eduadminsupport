using Common.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class Context
    {
        public IEnumerable<CourseCategory> CourseCategories { get; set; }
        public IList<Course> Courses { get; set; } = new List<Course>();
        public IList<Advisor> Advisors { get; set; } = new List<Advisor>();
        public IList<Topic> Topics { get; set; } = new List<Topic>();
        public IList<Student> Students { get; set; } = new List<Student>();
        public IList<Grading>? Gradings { get; set; }
        public IList<PresentationSessionType>? PresentationSessionTypes { get; set; }

        const string TopicUrlPrefix = @"https://www.aut.bme.hu/Task/";
        const bool RetrieveOnlyFirst10Topics = true;

        public static async Task<Context> RetrieveContextFromDataSources(ICourseCategorySource categorySource)
        {
            var context = new Context();
            context.CourseCategories = categorySource.GetCourseCategories();

            context.Courses = new List<Course>();   // TODO: retrieve all classes and courses here...

            foreach (var cat in context.CourseCategories)
            {
                await cat.GetCourses(context.Courses);
            }

            Console.Out.WriteLine("Retrieving topics and advisors...");
            context.Advisors = new List<Advisor>();
            context.Topics = new List<Topic>();
            foreach (var cat in context.CourseCategories)
            {
                var urls = await cat.GetTopicUrls(TopicUrlPrefix);

                if (RetrieveOnlyFirst10Topics)
                    urls = urls.Take(10);

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

            return context;

        }

        public void PerformBaseChecks()
        {
            if (RetrieveOnlyFirst10Topics)
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
            if (Gradings.Count() == 0)
                Console.WriteLine("WARNING: No Gradings in context");


        }
    }
}
