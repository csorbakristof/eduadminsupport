using ExcelReaderStandardLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Storage;

namespace PeerReviewCommonLib
{
    public class Processor
    {
        private List<Dictionary<string, string>> dictReviews;
        private List<Dictionary<string, string>> dictSupervisions;
        private List<Review> reviews;

        #region Public methods, to be called after each other.
        public void LoadReviews(StorageFile reviewsFile)
        {
            dictReviews = LoadXlsIntoDictionaryList(reviewsFile, 0, 1);
        }

        public void LoadSupervisions(StorageFile supervisionsFile)
        {
            dictSupervisions = LoadXlsIntoDictionaryList(supervisionsFile, 1, 1);
        }

        public void CollectReviewData()
        {
            reviews = dictReviews.Select(d => new Review(d)).ToList();
            var supervisions = dictSupervisions.Select(d => new Supervision(d));
            foreach (var r in reviews)
            {
                var s = supervisions
                        .FirstOrDefault(x => x.StudentNeptunCode == r.ReviewerNeptunCode);
                s?.ExtendReview(r);
            }
        }

        public IEnumerable<EmailMessage> CreateStudentEmails()
        {
            var allPresenters = reviews.Select(r => r.PresenterEmail).Distinct();
            foreach (var presenterEmail in allPresenters)
                yield return GetCollectedPresenterEmail(presenterEmail);
        }

        public IEnumerable<EmailMessage> CreateAdvisorEmails()
        {
            var allAdvisorNames = reviews.Select(r => r.AdvisorName).Distinct();
            foreach (var advisorName in allAdvisorNames)
                yield return GetCollectedAdvisorEmail(advisorName);
        }
        #endregion

        private class Supervision
        {
            public Supervision(Dictionary<string, string> dictSupervisionItem)
            {
                AdvisorName = dictSupervisionItem["Konzulens"];
                StudentName = dictSupervisionItem["Hallgató neve"];
                StudentNeptunCode = dictSupervisionItem["Hallg. nept"];
            }

            public void ExtendReview(Review target)
            {

                if (target.ReviewerNeptunCode != this.StudentNeptunCode)
                    throw new ArgumentException("Neptun code mismatch!");
                target.AdvisorName = AdvisorName;
                target.StudentName = StudentName;
            }

            internal string StudentName { get; set; }
            internal string StudentNeptunCode { get; set; }
            internal string AdvisorName { get; set; }
        }

        #region Helpers
        private List<Dictionary<string, string>> LoadXlsIntoDictionaryList(StorageFile file, int worksheetIndex, int headerRowIndex)
        {
            var stream = file.OpenStreamForReadAsync().Result;
            var reader = new Excel2Dict();
            return reader.Read(stream, worksheetIndex, headerRowIndex);
        }

        public EmailMessage GetCollectedPresenterEmail(string presenterEmail)
        {
            var currentReviews = this.reviews.Where(r => r.PresenterEmail == presenterEmail);
            var e = new EmailMessage();
            e.To.Add(new EmailRecipient(presenterEmail));
            e.Subject = "Beszámolóra kapott értékelések";
            StringBuilder sb = new StringBuilder();
            sb.Append("Kedves Hallgatónk!\n\n" +
                "A beszámolódhoz az alábbi értékelések gyűltek össze:\n\n");
            foreach (var r in currentReviews)
                sb.Append($"----- Értékelés - összesített pontszám: {r.OverallScore}\n\n{r.Text}\n\n");
            e.Body = sb.ToString();
            return e;
        }

        public EmailMessage GetCollectedAdvisorEmail(string advisorName)
        {
            var e = new EmailMessage();
            var advisorEmail = reviews.First(r => r.AdvisorName == advisorName).AdvisorEmail;
            e.To.Add(new EmailRecipient(advisorEmail));
            StringBuilder sb = new StringBuilder();
            sb.Append($"Kedves {advisorName}!\n\n" +
                "Hallgatóid az alábbi értékeléseket adták le a beszámolókra:\n\n");
            foreach (var r in reviews)
            {
                if (r.AdvisorEmail == advisorEmail)
                {
                    sb.Append($"--- Értékelő hallgató: {r.StudentName}({r.StudentNeptunCode}, {r.StudentEmail}) írta:\n\n");
                    sb.Append($"- összesített pontszám: {r.OverallScore}\n\n{r.Text}\n\n");
                }
            }
            e.Body = sb.ToString();
            return e;
        }
        #endregion
    }
}
