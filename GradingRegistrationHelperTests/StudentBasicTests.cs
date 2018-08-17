using System;
using GradingRegistrationHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GradingRegistrationHelperTests
{
    /// <summary>
    /// Summary description for StudentBasicTests
    /// </summary>
    [TestClass]
    public class StudentBasicTests
    {
        private Student s = new Student();
        public StudentBasicTests()
        {
        }

        const string SubjectCode = "VIAUA000";
        const string WrongSubjectCode = "ThereIsNoSuchCode";
        const string WrongSubjectCode2 = "ThereIsNoSuchCode2";

        [TestMethod]
        public void CanStoreGrade()
        {
            s.SetGrade(SubjectCode, 5);
            s.AddAttendance(SubjectCode);
            Assert.AreEqual(5, s.GetGrade(SubjectCode));
            expectGradingException(s, WrongSubjectCode);
        }

        [TestMethod]
        public void CanStoreAttendance()
        {
            s.AddAttendance(SubjectCode);
            Assert.IsTrue(s.IsAttending(SubjectCode));
            Assert.IsFalse(s.IsAttending(WrongSubjectCode));
        }

        #region Queries for grade with various attendance and graded subject patterns
        [TestMethod]
        public void NoAttendedSubject_AskedGradedSubject_Rejects()
        {
            s.SetGrade(SubjectCode, 5);
            expectGradingException(s, SubjectCode);
        }

        [TestMethod]
        public void NoAttendedSubject_AskedDifferentSubject_Rejects()
        {
            s.SetGrade(SubjectCode, 5);
            expectGradingException(s, WrongSubjectCode);
        }

        [TestMethod]
        public void SingleDifferentAttendedSubject_AskedAttendedSubject_ReturnsGrade()
        {
            s.SetGrade(SubjectCode, 5);
            s.AddAttendance(WrongSubjectCode);
            Assert.AreEqual(string.Format(
                Student.SingleMismatchingAttendanceOK, WrongSubjectCode, SubjectCode),
                s.GetGradingSituation(WrongSubjectCode));
            Assert.AreEqual(5, s.GetGrade(WrongSubjectCode));
        }

        [TestMethod]
        public void SingleDifferentAttendedSubject_AskedUnattendedSubject_Rejects()
        {
            s.SetGrade(SubjectCode, 5);
            s.AddAttendance(WrongSubjectCode);
            expectGradingException(s, WrongSubjectCode2);
        }

        [TestMethod]
        public void SingleDifferentAttendedSubject_AskedUnattendedButGradedSubject_Rejects()
        {
            s.SetGrade(SubjectCode, 5);
            s.AddAttendance(WrongSubjectCode);
            expectGradingException(s, SubjectCode);
        }

        [TestMethod]
        public void MultipleAttendedSubject_AskedAttendedGradedSubject_ReturnsGrade()
        {
            s.SetGrade(SubjectCode, 5);
            s.AddAttendance(SubjectCode);
            s.AddAttendance(WrongSubjectCode);
            Assert.AreEqual(Student.GradingOK, s.GetGradingSituation(SubjectCode));
            Assert.AreEqual(5, s.GetGrade(SubjectCode));
        }

        [TestMethod]
        public void MultipleAttendedSubject_AskedUnattendedGradedSubject_Rejects()
        {
            s.SetGrade(SubjectCode, 5);
            s.AddAttendance(WrongSubjectCode);
            s.AddAttendance(WrongSubjectCode2);
            expectGradingException(s, SubjectCode);
        }

        [TestMethod]
        public void MultipleAttendedSubject_AskedAttendedUngradedSubject_ReturnsGrade()
        {
            s.SetGrade(SubjectCode, 5);
            s.AddAttendance(SubjectCode);
            s.AddAttendance(WrongSubjectCode);
            Assert.AreEqual(string.Format(
                Student.MultipleAttendedSubjectsThisOneUngradedOK, SubjectCode, WrongSubjectCode),
                s.GetGradingSituation(WrongSubjectCode));
            Assert.AreEqual(5, s.GetGrade(WrongSubjectCode));
        }

        private void expectGradingException(Student s, string subject)
        {
            bool eThrown = false;
            try
            {
                s.GetGrade(subject);
            }
            catch (ArgumentException)
            {
                eThrown = true;
            }
            Assert.IsTrue(eThrown);
        }
        #endregion

        #region Setting advisor, query ungraded student
        [TestMethod]
        public void AttendingButUngraded_GradingSituationReturnsAdvisorName()
        {
            s.AddAttendance(SubjectCode);
            s.Advisor = "Minta Mókus";
            Assert.AreEqual(
                string.Format(Student.Ungraded,s.Advisor),
                s.GetGradingSituation(SubjectCode));
            Assert.AreEqual(null, s.GetGrade(SubjectCode));
        }

        [TestMethod]
        public void UnattendingAndUngraded_GradingThrowsException()
        {
            s.AddAttendance(SubjectCode);
            expectGradingException(s, WrongSubjectCode);
        }
        #endregion

        #region Merging
        const string Name1 = "Name1";
        const string Name2 = "Name2";
        const string NCode1 = "NCode1";
        const string NCode2 = "NCode2";
        const string Advisor1 = "Advisor1";
        const string Advisor2 = "Advisor2";
        const string OtherSubjectCode = "OtherSubjectCode";

        [TestMethod]
        public void Merging_NameAndNCode_Checks()
        {
            s.Name = Name1;
            s.NeptunCode = NCode1;
            var otherUnsetName = new Student() { NeptunCode = NCode1 };
            var otherUnsetNCode = new Student() { Name=Name1 };
            var otherNameMismatch = new Student() { Name=Name2, NeptunCode = NCode1 };
            var otherNCodeMismatch = new Student() { Name=Name1, NeptunCode = NCode2 };
            var otherUnset = new Student() { };
            expectMergeException(s, otherUnsetName);
            expectMergeException(s, otherUnsetNCode);
            expectMergeException(s, otherNameMismatch);
            expectMergeException(s, otherNCodeMismatch);
            expectMergeException(s, otherUnset);
            expectMergeException(otherUnset, s);
        }

        [TestMethod]
        public void Merging_GradeAndGradedSubjectAndAdvisor_Checks()
        {
            s.Name = Name1;
            s.NeptunCode = NCode1;
            var oNewGrade = new Student() { Name = Name1, NeptunCode = NCode1, Grade = 5, GradedSubject = SubjectCode, Advisor =Advisor1 };
            s.Merge(oNewGrade);
            Assert.AreEqual(5, s.Grade);
            Assert.AreEqual(SubjectCode, s.GradedSubject);
            Assert.AreEqual(Advisor1, s.Advisor);

            var oDiffGrade = new Student() { Name = Name1, NeptunCode = NCode1, Grade = 4 };
            expectMergeException(s, oDiffGrade);
            var oDiffGradedSubject = new Student() { Name = Name1, NeptunCode = NCode1, GradedSubject = WrongSubjectCode };
            expectMergeException(s, oDiffGradedSubject);
            var oDiffAdvisor = new Student() { Name = Name1, NeptunCode = NCode1, Advisor=Advisor2 };
            s.Merge(oDiffAdvisor);
            Assert.AreEqual($"{Advisor1},{Advisor2}", s.Advisor);
        }

        [TestMethod]
        public void Merging_Attendance()
        {
            s.Name = Name1;
            s.NeptunCode = NCode1;
            s.AddAttendance(SubjectCode);
            var oNewAttendance = new Student() { Name = Name1, NeptunCode = NCode1 };
            oNewAttendance.AddAttendance(SubjectCode);
            oNewAttendance.AddAttendance(OtherSubjectCode);
            s.Merge(oNewAttendance);
            Assert.AreEqual(2, s.Attendances.Count);
            Assert.AreEqual(OtherSubjectCode, s.Attendances[1]);
        }

        private void expectMergeException(Student s, Student o)
        {
            bool eThrown = false;
            try
            {
                s.Merge(o);
            }
            catch(ArgumentException)
            {
                eThrown = true;
            }
            Assert.IsTrue(eThrown);
        }

        #endregion
    }
}
