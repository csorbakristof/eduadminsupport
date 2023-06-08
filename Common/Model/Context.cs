using Common.DataSources;
using Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Common.Model
{
    [DataContract(IsReference = true)]
    public class Context
    {
        [DataMember]
        public List<CourseCategory> CourseCategories { get; set; } = new List<CourseCategory>();
        [DataMember]
        public List<Course> Courses { get; set; } = new List<Course>();
        [DataMember]
        public List<Advisor> Advisors { get; set; } = new List<Advisor>();
        [DataMember]
        public List<Topic> Topics { get; set; } = new List<Topic>();
        [DataMember]
        public List<Student> Students { get; set; } = new List<Student>();
        [DataMember]
        public List<Grading> Gradings { get; set; } = new List<Grading>();

        public void PerformBaseChecks()
        {
            // There are students, advisors, topics and courses in the context
            if (Students.Count() == 0)
                Console.WriteLine("WARNING: No Students in context");
            if (Advisors.Count() == 0)
                Console.WriteLine("WARNING: No Advisors in context");
            if (CourseCategories.Count() == 0)
                Console.WriteLine("WARNING: No CourseCategories in context");
            if (Courses.Count() == 0)
                Console.WriteLine("WARNING: No Courses in context");
            if (Gradings.Count() == 0)
                Console.WriteLine("WARNING: No Gradings in context");
        }
    }
}
