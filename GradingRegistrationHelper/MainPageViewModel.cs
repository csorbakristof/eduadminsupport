using ExcelReaderStandardLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace GradingRegistrationHelper
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public List<Student> Students { get; set; }

        private List<XlsExporter.Entry> exportEntries = new List<XlsExporter.Entry>();
        public List<XlsExporter.Entry> ExportEntries
        {
            get => this.exportEntries;
            set
            {
                if (this.exportEntries != value)
                {
                    exportEntries = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportEntries"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool generateOutputFiles = true;
        public bool GenerateOutputFiles
        {
            get => this.generateOutputFiles;
            set
            {
                if (this.generateOutputFiles != value)
                {
                    generateOutputFiles = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GenerateOutputFiles"));
                }
            }
        }


        public MainPageViewModel()
        {
            exportEntries.Add(new XlsExporter.Entry() { Name="Minta Mókus", NeptunCode="ABC123", Grade="5", Comment="Temp entry" });
        }

        internal async void LoadFromFolder(StorageFolder dir)
        {
            string folderAuthToken = Windows.Storage.AccessCache.
                StorageApplicationPermissions.FutureAccessList.Add(dir);

            var loader = new XlsLoader();   // Builder design pattern
            var files = await dir.GetFilesAsync();
            var gradesFile = files.Where(s => s.Name.StartsWith("tema-osztalyzatok")).Single();
            var attandanceFiles = files.Where(s => s.Name.StartsWith("jegyimport")).ToArray();
            var advisorFile = files.Where(s => s.Name.StartsWith("Terheles_")).Single();
            loader.Load(gradesFile, attandanceFiles, advisorFile);
            Students = loader.GetStudentList();

            var exporter = new XlsExporter();
            var exportDictionary = exporter.CollectExports(Students);
            ExportEntries = exportDictionary.SelectMany(dictEntry => dictEntry.Value).ToList();

            if (GenerateOutputFiles)
            {
                CreateOutputXlsFiles(dir, exportDictionary);
                CreateStatusResultXlsFile(dir);
            }
        }

        private void CreateStatusResultXlsFile(StorageFolder dir)
        {
            var filename = "grading_status.xlsx";
            var lines = ExportEntries
                .Select(e => new string[] { e.Name, e.NeptunCode, e.Grade, e.Comment });
            XlsWriter writer = new XlsWriter();
            using (var outputstream = dir.OpenStreamForWriteAsync(filename, CreationCollisionOption.ReplaceExisting).Result)
            {
                writer.WriteXls(outputstream, lines.ToList());
            }
        }

        private void CreateOutputXlsFiles(StorageFolder dir, Dictionary<string, List<XlsExporter.Entry>> exportDictionary)
        {
            foreach (var subject in exportDictionary.Keys)
            {
                var filename = subject + ".xlsx";
                var lines = exportDictionary[subject].Where(e=>e.Grade != null)
                    .Select(e => new string[] { e.Name, e.NeptunCode, e.Grade });
                XlsWriter writer = new XlsWriter();
                using (var outputstream = dir.OpenStreamForWriteAsync(filename, CreationCollisionOption.ReplaceExisting).Result)
                {
                    //CreateCsv(outputstream, lines);
                    writer.WriteXls(outputstream, lines.ToList());
                }
            }
        }

        private void CreateCsv(Stream outputstream, IEnumerable<string[]> lines)
        {
            using (var sw = new StreamWriter(outputstream))
            {
                foreach (var l in lines)
                    sw.WriteLine(string.Join(';', l));
                sw.Flush();
            }
        }
    }
}
