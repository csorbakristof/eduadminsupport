using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Email;

namespace PeerReviewCommonLib
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

        public string Text { get; set; }
        public int OverallScore { get; set; }
        public string ReviewerNeptunCode { get; set; }
        public string PresenterEmail { get; set; }

        public string StudentName { get; set; }
        public string StudentNeptunCode { get; set; }
        public string StudentEmail { get; set; }
        public string AdvisorName { get; set; }
        public string AdvisorEmail { get; set; }
    }
}
