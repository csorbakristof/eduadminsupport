using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static OnlabTemakHelyzetkep.TopicRetriever;

namespace OnlabTemakHelyzetkep
{
    internal class TopicRetriever
    {
        public async Task<IEnumerable<string>> GetTopicUrlList(string urlForTopicsOfInspectedCourse)
        {
            var httpClient = new HttpClient();
            var topicUrls = new List<string>();

            using HttpResponseMessage response = await httpClient.GetAsync(urlForTopicsOfInspectedCourse);
            response.EnsureSuccessStatusCode();

            var pageSource = await response.Content.ReadAsStringAsync();

            Regex findTopicUrls = new Regex("<a title=\"Téma adatainak megtekintése, jelentkezés a témára.\" href=\"../../Task/(.+)\">(.+) »</a>");

            var matches = findTopicUrls.Matches(pageSource);

            foreach(Match match in matches)
            {
                var url = match.Groups[1].Value;
                topicUrls.Add(url);
            }

            return topicUrls;
        }

        public async Task<Topic> GetTopic(string topicUrl)
        {
            var httpClient = new HttpClient();
            using HttpResponseMessage response = await httpClient.GetAsync(topicUrl);
            response.EnsureSuccessStatusCode();

            var pageSource = await response.Content.ReadAsStringAsync();

            var topic = new Topic();
            Regex findTitle = new Regex(@"<h1>(.+)</h1>");
            topic.Title = findTitle.Match(pageSource).Groups[1].Value;
            
            Regex findMaxStudentCount = new Regex("<span id=\"lblLimit\">(\\d+) fő</span>");
            var maxStudentCountString = findMaxStudentCount.Match(pageSource).Groups[1].Value;
            topic.MaxStudentCount = int.Parse(maxStudentCountString);

            topic.Advisors = new List<string>();
            Regex findAdvisors = new Regex("<a id=\"hypName\" title=\"(.+) adatainak megtekintése.\" href");
            foreach(Match advisorMatch in findAdvisors.Matches(pageSource))
            {
                topic.Advisors.Add(advisorMatch.Groups[1].Value);
            }

            Regex findExternalPartner = new Regex("<span id=\"lblOuterPartner\">(.+)</span>");
            topic.IsExternal = findExternalPartner.IsMatch(pageSource);

            return topic;
        }

        internal async Task<string[]> GetCourseCodesForCourseCategory(string url)
        {
            var httpClient = new HttpClient();
            var courseCodes = new List<string>();

            using HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var pageSource = await response.Content.ReadAsStringAsync();

            Regex findTopicUrls = new Regex("href=\"../../Course/([^\"]+)\"");

            var matches = findTopicUrls.Matches(pageSource);

            foreach (Match match in matches)
            {
                var courseCode = match.Groups[1].Value;
                courseCodes.Add(courseCode);
            }

            return courseCodes.ToArray();

        }
    }
}
