using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    [DataContract(IsReference = true)]
    public class Student
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember] 
        public string NKod { get; set; }

        [DataMember]
        public List<Course> EnrolledCourses { get; set; } = new List<Course>();

        [DataMember]
        public List<Grading> Gradings { get; set; } = new List<Grading>();

        [DataMember]
        public List<(Advisor, Topic)>? TopicRegistrations { get; set; } // Based on AdvisorLoadSource

        public Student(string name, string nKod)
        {
            Name = name;
            NKod = nKod;
        }

        public Student()
        {
            Name = string.Empty;
            NKod = string.Empty;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
