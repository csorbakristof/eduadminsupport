using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public class WebDownloaderWithRegexMatcher
    {
        public static async Task<string[]> DownloadAndCollectMatches(string url, string regex)
        {
            var results = new List<string>();
            try
            {
                string pageSource = string.Empty;

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10); // add a timeout of 10 seconds
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    pageSource = await response.Content.ReadAsStringAsync();
                }

                Regex rg = new Regex(regex);
                var matches = rg.Matches(pageSource);
                foreach (Match match in matches)
                {
                    var foundValue = match.Groups[1].Value;
                    results.Add(foundValue);
                }
            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync(e.ToString());
            }
            return results.ToArray();
        }
    }
}
