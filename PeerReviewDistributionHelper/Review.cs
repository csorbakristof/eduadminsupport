using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Email;

namespace PeerReviewDistributionHelper
{
    internal class Review
    {
        public Review(Dictionary<string, string> dictReviewItem)
        {
            PresenterEmail = dictReviewItem["PresenterEmail"];
            ReviewerNeptunCode = dictReviewItem["ReviewerNKod"];
            OverallScore = int.Parse(dictReviewItem["Score"]);
            Text = dictReviewItem["Text"];
        }

        // Data available from the review results XLS
        public string Text { get; set; }
        public int OverallScore { get; set; }
        public string ReviewerNeptunCode { get; set; }
        public string PresenterEmail { get; set; }

        // Data available from the supervision ("Terheles") XLS
        public string ReviewerName { get; set; }
        public string AdvisorName { get; set; }

        // Data available from the supervisor email XLS
        public string AdvisorEmail { get; set; }
    }
}
