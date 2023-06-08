using Common.Helpers;

namespace Common.DataSources
{
    public class AdvisorLoadSource
    {
        public IEnumerable<(string Advisor,string TopicTitle, string StudentName, string StudentNkod)> GetStudentNamesAndAdvisorList(string filename)
        {
            // Load current advisor data from excel (exported from the departments portal)
            Excel2Dict e = new Excel2Dict();
            var advisorTable = e.Read(filename, 1, 1);
            // For each topic we search how many students are on it currently
            foreach (var line in advisorTable)
            {
                var advisorName = line["Konzulens"];
                var startOfUsernameInBrackets = advisorName.IndexOf(" (");
                advisorName = advisorName.Substring(0, startOfUsernameInBrackets);
                yield return (advisorName, line["Téma címe"], line["Hallgató neve"], line["Hallg. nept"]);
            }
        }
    }
}
