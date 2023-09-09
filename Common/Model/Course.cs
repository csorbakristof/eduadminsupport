using System.Runtime.Serialization;

namespace Common.Model
{
    [DataContract(IsReference = true)]
    public class Course
    {
        [DataMember]
        public string Name { get; internal set; }
        [DataMember]
        public string ClassCode { get; set; } = string.Empty;   // Like VIAUAL00
        [DataMember]
        public string CourseCode { get; set; } = string.Empty;  // Like L

        public bool IsEnglish => CourseCode.StartsWith('A');
        [DataMember]
        public int? EnrolledStudentCountInNeptun { get; set; }
        [DataMember]
        public List<string> EnrolledStudentNKodsFromNeptun { get; set; } = new List<string>();
        public override string ToString()
        {
            return $"{ClassCode}-{CourseCode}";
        }

        // SemesterPostfix: like '2022_23_2'
        public string GradeImportFilename(string semesterPostfix) => $"jegyimport_BME{ClassCode}_{CourseCode}_{semesterPostfix}.xlsx";
        public string CourseCodeForExportFilename() => $"BME{ClassCode}_{CourseCode}_4Neptun.xlsx";
    }
}
