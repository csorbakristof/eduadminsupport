using GradingRegistrationHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingRegistrationHelperTests
{
    [TestClass]
    public class XlsExporterTests
    {
        TestXlsExporter exporter = new TestXlsExporter();

        [TestMethod]
        public void CollectingAttendances()
        {
            const string code1 = "code1";
            const string code2 = "code2";
            const string code3 = "code3";
            var s1 = new Student();
            s1.AddAttendance(code1);
            s1.AddAttendance(code2);
            var s2 = new Student();
            s2.AddAttendance(code2);
            s2.AddAttendance(code3);
            var s = new List<Student>();
            s.Add(s1);
            s.Add(s2);
            var att = exporter.GetAttendances(s).ToList();
            Assert.AreEqual(3, att.Count);
            Assert.IsTrue(att.Contains(code1));
            Assert.IsTrue(att.Contains(code2));
            Assert.IsTrue(att.Contains(code3));
        }


        [TestMethod]
        public void GeneratesOutputForAllAttendances()
        {
            var dict = exporter.CollectExports(CreateStudentList());
            Assert.AreEqual(2, dict.Keys.Count);
            Assert.AreEqual(2, dict[subjectCode1].Count);
            Assert.AreEqual(1, dict[subjectCode2].Count);
        }

        [TestMethod]
        public void GetEntriesTest()
        {
            var e = exporter.GetEntries(CreateStudentList(), subjectCode1).ToList();
            Assert.AreEqual(2, e.Count);
            Assert.AreEqual(name1, e[0].Name);
            Assert.AreEqual(ncode1, e[0].NeptunCode);
            Assert.AreEqual(grade1.ToString(), e[0].Grade);
            Assert.AreEqual(name2, e[1].Name);
            Assert.AreEqual(ncode2, e[1].NeptunCode);
            Assert.AreEqual(grade2.ToString(), e[1].Grade);
        }

        const string subjectCode1 = "code1";
        const string subjectCode2 = "code2";
        const string name1 = "name1";
        const string name2 = "name2";
        const string ncode1 = "ncode1";
        const string ncode2 = "ncode2";
        const int grade1 = 5;
        const int grade2 = 4;
        private List<Student> CreateStudentList()
        {
            var s1 = new Student() { Name = name1, NeptunCode = ncode1, Grade = grade1 };
            s1.AddAttendance(subjectCode1);
            var s2 = new Student() { Name = name2, NeptunCode = ncode2, Grade = grade2 };
            s2.AddAttendance(subjectCode1);
            s2.AddAttendance(subjectCode2);
            List<Student> s = new List<Student>();
            s.Add(s1);
            s.Add(s2);
            return s;
        }

        class TestXlsExporter : XlsExporter
        {
            public new IEnumerable<string> GetAttendances(List<Student> students)
            {
                return base.GetAttendances(students);
            }

            public new IEnumerable<Entry> GetEntries(List<Student> students, string attendance)
            {
                return base.GetEntries(students, attendance);
            }


        }

    }
}
