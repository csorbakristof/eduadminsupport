using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.Model
{
    public class Topic
    {
        public string Title { get; set; }
        public int SeatCount { get; set; }
        public string Url { get; set; }
        public IList<Advisor>? Advisors { get; set; }
        public IList<CourseCategory>? CourseCategories { get; set; }
        public bool IsExternal { get; set; }

        public IList<Student>? RegisteredStudents { get; set; }

        public static async Task<Topic> RetrieveFromWeb(string url, IList<Advisor> advisorListToExtendAsNeeded)
        {
            await Console.Out.WriteLineAsync("Retrieving: "+url);

            var httpClient = new HttpClient();
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var pageSource = await response.Content.ReadAsStringAsync();

            var topic = new Topic();
            topic.Url = url;
            Regex findTitle = new Regex(@"<h1>(.+)</h1>");
            topic.Title = findTitle.Match(pageSource).Groups[1].Value;

            Regex findMaxStudentCount = new Regex("<span id=\"lblLimit\">(\\d+) fő</span>");
            var maxStudentCountString = findMaxStudentCount.Match(pageSource).Groups[1].Value;
            topic.SeatCount = int.Parse(maxStudentCountString);

            Regex findExternalPartner = new Regex("<span id=\"lblOuterPartner\">(.+)</span>");
            topic.IsExternal = findExternalPartner.IsMatch(pageSource);

            var advisorList = new List<Advisor>();
            Regex findAdvisors = new Regex("<a id=\"hypName\" title=\"(.+) adatainak megtekintése.\" href");
            foreach (Match advisorMatch in findAdvisors.Matches(pageSource))
            {
                var advisorName = advisorMatch.Groups[1].Value;
                var advisor = advisorListToExtendAsNeeded.FirstOrDefault(a => a.Name == advisorName);
                if (advisor != null)
                {
                    advisorList.Add(advisor);
                }
                else
                {
                    var newAdvisor = new Advisor(advisorName);
                    advisorListToExtendAsNeeded.Add(newAdvisor);
                    advisorList.Add(newAdvisor);
                }
            }
            topic.Advisors = advisorList;
            return topic;
        }
    }
}
