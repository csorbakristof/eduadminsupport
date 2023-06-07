using OfficeOpenXml.ConditionalFormatting;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Common.Model
{
    [DataContract(IsReference = true)]
    public class CourseCategory
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public List<Course> Courses { get; set; } = new List<Course>();

        public CourseCategory()
        {
            Title = string.Empty;
            Url = string.Empty;
        }

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
            var httpClient = new HttpClient();

            using HttpResponseMessage response = await httpClient.GetAsync(Url);
            response.EnsureSuccessStatusCode();

            var pageSource = await response.Content.ReadAsStringAsync();

            Regex findTopicUrls = new Regex("href=\"../../Course/([^\"]+)\"");

            var matches = findTopicUrls.Matches(pageSource);

            foreach (Match match in matches)
            {
                var classCode = match.Groups[1].Value;
                Courses.AddRange(courseList.Where(c => c.ClassCode == classCode)); // May have multiple courses in that class
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
