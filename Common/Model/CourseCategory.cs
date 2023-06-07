using System.Text.RegularExpressions;

namespace Common.Model
{
    public class CourseCategory
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public IEnumerable<Course>? Courses { get; set; }

        public CourseCategory(string title, string url)
        {
            Title = title;
            Url = url;
        }

        public async Task<IEnumerable<string>> GetTopicUrls(string urlPrefix)
        {
            var httpClient = new HttpClient();
            var topicUrls = new List<string>();
            using HttpResponseMessage response = await httpClient.GetAsync(Url);
            response.EnsureSuccessStatusCode();
            var pageSource = await response.Content.ReadAsStringAsync();
            Regex findTopicUrls = new Regex("<a title=\"Téma adatainak megtekintése, jelentkezés a témára.\" href=\"../../Task/(.+)\">(.+) »</a>");
            var matches = findTopicUrls.Matches(pageSource);
            foreach (Match match in matches)
            {
                var url = match.Groups[1].Value;
                topicUrls.Add(urlPrefix + url);
            }
            return topicUrls;
        }

        internal async Task GetCourses(IEnumerable<Course> courseList)
        {
            var courses = new List<Course>();

            var httpClient = new HttpClient();

            using HttpResponseMessage response = await httpClient.GetAsync(Url);
            response.EnsureSuccessStatusCode();

            var pageSource = await response.Content.ReadAsStringAsync();

            Regex findTopicUrls = new Regex("href=\"../../Course/([^\"]+)\"");

            var matches = findTopicUrls.Matches(pageSource);

            foreach (Match match in matches)
            {
                var classCode = match.Groups[1].Value;
                courses.AddRange(courseList.Where(c => c.ClassCode == classCode)); // May have multiple courses in that class
            }

            this.Courses = courses;
        }
    }
}
