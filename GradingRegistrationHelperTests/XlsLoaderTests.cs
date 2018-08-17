using ExcelReaderStandardLibrary;
using GradingRegistrationHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace GradingRegistrationHelperTests
{
    [TestClass]
    public class XlsLoaderTests
    {
        [TestMethod]
        public void TestSubjectCodeFromFilename()
        {
            Assert.AreEqual("BMEVIAUM963_Gy", XlsLoader.SubjectCodeFromFilename("jegyimport_BMEVIAUM963_Gy_2017_18_1.xlsx"));
            Assert.AreEqual("BMEVIAUMT00_Gy", XlsLoader.SubjectCodeFromFilename("jegyimport_BMEVIAUMT00_Gy_2017_18_1.xlsx"));
            Assert.AreEqual("BMEVIAUAL00_AL", XlsLoader.SubjectCodeFromFilename("jegyimport_BMEVIAUAL00_AL_2017_18_1.xlsx"));
            Assert.AreEqual("BMEVIAUAT00_AGY", XlsLoader.SubjectCodeFromFilename("jegyimport_BMEVIAUAT00_AGY_2017_18_1.xlsx"));
            Assert.AreEqual("BMEVIAUAL00_L", XlsLoader.SubjectCodeFromFilename("jegyimport_BMEVIAUAL00_L_2017_18_1.xlsx"));
        }

        const string Name1 = "Name1";
        const string NCode1 = "NCode1";
        const string Name2 = "Name2";
        const string NCode2 = "NCode2";
        const string Advisor = "Advisor";
        const string Subject1 = "Subject1";
        const string Subject2 = "Subject2";

        [TestMethod]
        public void TestGradeImport()
        {
            var t = new TestXlsLoader();
            t.Reader.AddGrading(Name1, NCode1, Subject1, 5);
            var res = t.LoadGrades().ToArray();
            Assert.AreEqual(1, res.Length);
            Assert.AreEqual(Name1, res[0].Name);
            Assert.AreEqual(NCode1, res[0].NeptunCode);
            Assert.AreEqual(Subject1, res[0].GradedSubject);
            Assert.AreEqual(5, res[0].Grade);
        }

        [TestMethod]
        public void TestAdvisorImport()
        {
            var t = new TestXlsLoader();
            t.Reader.AddAdvisor(Name1, NCode1, Advisor);
            t.Reader.AddAdvisor(Name2, NCode2, Advisor);
            var res = t.LoadAdvisors().ToArray();
            Assert.AreEqual(2, res.Length);
            Assert.AreEqual(Name1, res[0].Name);
            Assert.AreEqual(NCode1, res[0].NeptunCode);
            Assert.AreEqual(Advisor, res[0].Advisor);
        }

        class TestXlsLoader : XlsLoader
        {
            public List<Student> Students => base.students;

            public TestXlsLoader() : base()
            {
                base.Reader = new FakeReader();
            }

            public new FakeReader Reader => this.Reader as FakeReader;

            public IEnumerable<Student> LoadGrades()
            {
                return base.LoadGrades(null);
            }

            public IEnumerable<Student> LoadAdvisors()
            {
                return base.LoadAdvisors(null);
            }

            public IEnumerable<Student> LoadAttendances()
            {
                return base.LoadAttendances(null);
            }
        }

        class FakeReader : ITableReader
        {
            private List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            public List<Dictionary<string, string>> Read(Stream filename, int worksheetIndex, int headerRowIndex)
            {
                return list;
            }

            public void AddGrading(string name, string ncode, string gradedSubject, int? grade)
            {
                var d = new Dictionary<string, string>();
                d.Add(XlsLoader.GradingNameKey, name);
                d.Add(XlsLoader.GradingNCodeKey, ncode);
                d.Add(XlsLoader.GradingSubjectKey, gradedSubject);
                if (grade.HasValue)
                    d.Add(XlsLoader.GradingGradeKey, grade.Value.ToString());
                list.Add(d);
            }

            public void AddAdvisor(string name, string ncode, string advisor)
            {
                var d = new Dictionary<string, string>();
                d.Add(XlsLoader.AdvisorNameKey, name);
                d.Add(XlsLoader.AdvisorNCodeKey, ncode);
                d.Add(XlsLoader.AdvisorAdvisorKey, advisor);
                list.Add(d);
            }
        }
    }
}



