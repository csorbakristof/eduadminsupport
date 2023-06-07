using Common.Helpers;

namespace Common.DataSources
{
    public class AdvisorLoadSource
    {
        const string PortalTerhelesExportFilename = @"c:\_onlabFelugyeletAdatok\Terheles_22-23-tavasz.xlsx";

        public IEnumerable<(string Advisor,string TopicTitle, string StudentName, string StudentNkod)> GetStudentNamesAndAdvisorList()
        {
            // Load current advisor data from excel (exported from the departments portal)
            Excel2Dict e = new Excel2Dict();
            var advisorTable = e.Read(PortalTerhelesExportFilename, 1, 1);
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
