using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.Model
{
    [DataContract(IsReference = true)]
    public class Topic
    {
        [DataMember] 
        public string Title { get; set; }
        [DataMember] 
        public int SeatCount { get; set; }
        [DataMember] 
        public string Url { get; set; }
        [DataMember]
        public List<Advisor> Advisors { get; set; } = new List<Advisor>();
        [DataMember]
        public List<CourseCategory> CourseCategories { get; set; } = new List<CourseCategory>();
        [DataMember] 
        public bool IsExternal { get; set; }

        public bool IsForEnglishStudents => Title.StartsWith("Z-ENG");

        [DataMember]
        public List<Student> RegisteredStudents { get; set; } = new List<Student>();

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

        public override string ToString()
        {
            return Title;
        }
    }
}
