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
        public class CourseCategory
        {
            public string Title;
            public string Url;
            public string[] CourseCodes;
            public int enrolledHungarianStudentCount;
            public int enrolledEnglishStudentCount;
        }

        public class DepartmentPortalInfoContext
        {
            public List<CourseCategory> CourseCategories;
            public List<Topic> Topics;
        }

        public DepartmentPortalInfoContext Context;

        static async Task Main(string[] args)
        {
            // Command line arguments: terhelesXls coursesXls [retrievedPortalDataXml]
            // c:\temp\Terheles_22-23-tavasz.xlsx c:\temp\kurzusok_neptunExport.xlsx c:\temp\data.xml
            var terhelesXlsFilename = args[0];
            var coursesXlsFilename = args[1];
            var retrievedPortalData = string.Empty;
            if (args.Length>=3)
                retrievedPortalData = args[2];
            Console.WriteLine($"Using terheles xls: {terhelesXlsFilename}");
            Console.WriteLine($"Using kurzusok xls: {coursesXlsFilename}");
            Console.WriteLine($"Using retrievedData xml: {retrievedPortalData}");

            Program p = new Program();

            if (retrievedPortalData == string.Empty || !System.IO.File.Exists(retrievedPortalData))
            {
                Console.WriteLine("No previous download data, downloading from department portal...");
                p.Context = new DepartmentPortalInfoContext() { Topics = new List<Topic>() };
                p.Context.CourseCategories = new List<CourseCategory>()
                    {
                        new CourseCategory() { Title = "Onlab BSc Info", Url = "https://www.aut.bme.hu/Education/BScInfo/Onlab"},
                        new CourseCategory() { Title="Témalabor BProf", Url = "https://www.aut.bme.hu/Education/BProf/Temalabor" },
                        new CourseCategory() { Title="Önlab BSc Villany", Url = "https://www.aut.bme.hu/Education/BScVillany/Onlab" },
                        new CourseCategory() { Title="Szakdolgozat BSc Villany", Url = "https://www.aut.bme.hu/Education/BScVillany/Szakdolgozat" },
                        new CourseCategory() { Title="Témalabor BSc Info", Url = "https://www.aut.bme.hu/Education/BScInfo/Temalabor" },
                        new CourseCategory() { Title="Önlab BSc Info", Url = "https://www.aut.bme.hu/Education/BScInfo/Onlab" },
                        new CourseCategory() { Title="Szakdolgozat BSc Info", Url = "https://www.aut.bme.hu/Education/BScInfo/Szakdolgozat" },
                        new CourseCategory() { Title="Önlab MSc Villany", Url = "https://www.aut.bme.hu/Education/MScVillany/Onlab" },
                        new CourseCategory() { Title="Dipterv MSc Villany", Url = "https://www.aut.bme.hu/Education/MScVillany/Diploma" },
                        new CourseCategory() { Title="Önlab MSc Info", Url = "https://www.aut.bme.hu/Education/MScInfo/Onlab" },
                        new CourseCategory() { Title="Dipterv MSc Info", Url = "https://www.aut.bme.hu/Education/MScInfo/Diploma" },
                        new CourseCategory() { Title="Önlab MSc Mecha", Url = "https://www.aut.bme.hu/Education/MScMechatronika/Onlab" },
                        new CourseCategory() { Title="Dipterv MSc Mecha", Url = "https://www.aut.bme.hu/Education/MScMechatronika/Diploma" }
                    };

                // Load topic data from the web
                await p.RetrieveData();

                XmlSerializer xml = new XmlSerializer(typeof(DepartmentPortalInfoContext));
                xml.Serialize(new StreamWriter(@"c:\temp\data.xml"), p.Context);
                Console.WriteLine("Downloaded data saved.");
            }
            else
            {
                XmlSerializer xml = new XmlSerializer(typeof(DepartmentPortalInfoContext));
                p.Context = (DepartmentPortalInfoContext)xml.Deserialize(new StreamReader(@"c:\temp\data.xml"));
                Console.WriteLine("Downloaded data loaded from local file.");
            }

            // Load current advisor data from excel (exported from the departments portal)
            Excel2Dict e = new Excel2Dict();
            var advisorTable = e.Read(@"c:\temp\Terheles_22-23-tavasz.xlsx", 1, 1);

            // Load current student counts on the courses (exported from Neptun)
            var courseEnrolledStudentCounts = e.Read(@"c:\temp\kurzusok_neptunExport.xlsx", 0, 1);

            // Combine information
            p.AddCurrentAdvisorData(advisorTable);
            p.AddCourseEnrolledStudentCounts(courseEnrolledStudentCounts);

            // Show stats
            p.ShowStats();

        }

        // Function taking an URL and downloading an excel file and saving it into the file system.
        private async Task DownloadFile(string url, string filename)
        {
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(url))
                {
                    using (var fileStream = new FileStream(filename, FileMode.Create))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }
                }
            }
        }

        private void AddCourseEnrolledStudentCounts(List<Dictionary<string, string>> courseEnrolledStudentCounts)
        {
            Regex getEnrolledStudentCount = new Regex(@"(\d+)/\d+/\d+");
            for (int i=0; i<Context.CourseCategories.Count; i++)
            {
                var currentCourse = Context.CourseCategories[i];
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
                //Console.WriteLine($"Searching for students on topic: {topic.Title}");
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
            for(int i=0; i< Context.CourseCategories.Count; i++)
            {
                var courseCategory = Context.CourseCategories[i];
                var topicUrls = await retriever.GetTopicUrlList(courseCategory.Url);
                courseCategory.CourseCodes = await retriever.GetCourseCodesForCourseCategory(courseCategory.Url);

                foreach (var topicUrl in topicUrls)
                {
                    var fullUrl = "https://www.aut.bme.hu/Task/" + topicUrl;

                    Topic topic = Context.Topics.SingleOrDefault(t => t.Url == fullUrl);
                    if (topic.Title == null)
                    {
                        topic = await retriever.GetTopic(fullUrl);
                        topic.Url = fullUrl;
                        topic.CourseCategories = new List<string>() { courseCategory.Title };
                        topic.StudentNKods = new List<string>();
                        //Console.WriteLine($"New topic: {topic.Title} (limit {topic.MaxStudentCount})");
                        Console.Write('N');
                        Context.Topics.Add(topic);
                    }
                    else
                    {
                        topic.CourseCategories.Add(courseCategory.Title);
                        //Console.WriteLine($"Extended topic: {topic.Title} (limit {topic.MaxStudentCount})");
                        Console.Write('E');
                    }

                }
            }
        }

        private void ShowStats()
        {
            Console.WriteLine("========== Statistics ==========");
            Console.WriteLine($"Topic count: {Context.Topics.Count()}");
            Console.WriteLine($"External topic count: {Context.Topics.Count(t=>t.IsExternal)}");

            Console.WriteLine("====== Course based statistics: (total, occupied, and available seat counts for hungarian and english students)");
            foreach(var cc in Context.CourseCategories)
            {
                Console.WriteLine($"--- Course Category: {cc.Title}");
                Console.WriteLine($"Number of enrolled students: hungarian {cc.enrolledHungarianStudentCount}, english {cc.enrolledEnglishStudentCount}");
                var hunTopics = Context.Topics.Where(t => t.CourseCategories.Contains(cc.Title)).Where(t => !t.Title.StartsWith("Z-Eng")).ToArray();
                var engTopics = Context.Topics.Where(t => t.CourseCategories.Contains(cc.Title)).Where(t => t.Title.StartsWith("Z-Eng")).ToArray();
                var hunSeatCount = hunTopics.Sum(t => t.MaxStudentCount);
                var engSeatCount = engTopics.Sum(t => t.MaxStudentCount);
                //Console.WriteLine($"Total seat count HUN {hunSeatCount}, ENG {engSeatCount}");
                var hunOccupiedSeatCount = hunTopics.Sum(t => t.StudentNKods.Count);
                var engOccupiedSeatCount = engTopics.Sum(t => t.StudentNKods.Count);
                //Console.WriteLine($"Occupied seat count HUN {hunOccupiedSeatCount}, ENG {engOccupiedSeatCount}");
                Console.WriteLine($"Used seat ratios HUN {100 * hunOccupiedSeatCount / hunSeatCount}% ENG { ((engSeatCount>0) ? (100*engOccupiedSeatCount/engSeatCount):(0)) }% ");
            }

            // Melyik konzulensnél van még szabad hely (magyar-angol helyeket nem megkülönböztetve)
            var availableSeats = new Dictionary<string, int>();
            foreach(var t in Context.Topics)
            {
                if (t.IsExternal)
                    continue;   // KÜLSŐ TÉMÁK esetén ezt spécin kell kezelni!!
                var freeSeats = t.MaxStudentCount - t.StudentNKods.Count;
                foreach(var a in t.Advisors)
                {
                    if (availableSeats.ContainsKey(a))
                        availableSeats[a]+=freeSeats;
                    else
                        availableSeats.Add(a,freeSeats);
                }
            }
            foreach(var advisor in availableSeats.Keys)
            {
                Console.WriteLine($"Advisor {advisor} free seats: {availableSeats[advisor]}");
            }
            


            // Konzulensenként: mennyi szabad hely van és ez melyik kurzus kategóriákra vonatkozik (és angol vagy magyar)?
            //  Közös témán lévő szabad hely minden konzulenshez számítson ebben az esetben, de egyébként a szabad helyek számába csak egyszer!

            // Angol hallgatók (Neptun kurzus szerint) magyar kiírású témán? (Kell a Neptun kurzusok exportja egyesével)

            // Félév elejére:
            // Hány hallgatónak nincsen még témája? Ezek mely kurzusokon vannak?
            //  Ehhez kell a Neptun névsor minden tantárgyhoz

            // Csak érdekességként:
            // Külső témákon lévők aránya kurzus kategóriánként
            // Kiírt helyek száma belső és külső témákon?

            // Beszámolókon ki nem jelentkezett még felügyelőnek 2 (3) helyre?

            // Félév végére:
            // Utána igazából már a jegyeket is összeszedheti ez a rendszer, többszörös jelentkezésekre jobban felkészülve, mint a másik.

            Console.WriteLine("====== Further stats");
            Console.WriteLine($"Number or enrolled students: {Context.Topics.Count(t => t.IsExternal)}");
            Console.WriteLine($"Topics with multiple advisors: {Context.Topics.Count(t => t.Advisors.Count > 1)}");
            Console.WriteLine($"Topics with multiple courses: {Context.Topics.Count(t => t.CourseCategories.Count > 1)}");

            Console.WriteLine($"Total capacity for students: {Context.Topics.Sum(t => t.MaxStudentCount)}");
            Console.WriteLine($"Total free capacity for students: {Context.Topics.Sum(t => t.MaxStudentCount - t.StudentNKods.Count)}");
            Console.WriteLine($"Number of topics with free seats: {Context.Topics.Count(t => t.MaxStudentCount > t.StudentNKods.Count)}");

        }
    }
}
