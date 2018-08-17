using ExcelReaderStandardLibrary;
using PeerReviewCommonLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using System.Linq;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PeerReviewDistributionHelper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Processor processor = new Processor();
        public MainPage()
        {
            this.InitializeComponent();
            EventsListBox.Items.Add("Initialized. First load Supervision file ('Terheles' on AUT portal).");
        }

        private async void LoadSupervisionButton_Click(object sender, RoutedEventArgs e)
        {
            processor.LoadSupervisions(await PickFileToOpen());
            EventsListBox.Items.Add("Supervisions loaded. Next, load reviews XLS.");
        }

        private async void LoadReviewsButton_Click(object sender, RoutedEventArgs e)
        {
            processor.LoadReviews(await PickFileToOpen());
            EventsListBox.Items.Add("Reviews loaded. Next, match advisors.");
        }

        private void CollectReviewDataButton_Click(object sender, RoutedEventArgs e)
        {
            processor.CollectReviewData();
            EventsListBox.Items.Add("Advisors matched to reviewers. Next, generate e-mails.");
        }

        private async void CreateStudentEmailsButton_Click(object sender, RoutedEventArgs e)
        {
            var emails = processor.CreateStudentEmails();
            foreach(var email in emails)
                await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(email);
            EventsListBox.Items.Add("Student e-mails generated.");
        }

        private async void CreateAdvisorEmailsButton_Click(object sender, RoutedEventArgs e)
        {
            var emails = processor.CreateAdvisorEmails();
            foreach (var email in emails)
                await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(email);
            EventsListBox.Items.Add("Advisor e-mails generated.");
        }

        private async Task<StorageFile> PickFileToOpen()
        {
            var fop = new Windows.Storage.Pickers.FileOpenPicker();
            fop.FileTypeFilter.Add("*");
            return await fop.PickSingleFileAsync();
        }
    }
}
