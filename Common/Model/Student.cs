using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class Student
    {
        public string Name { get; set; }
        public string NKod { get; set; }

        public IList<Course>? EnrolledCourses { get; set; }

        public Student(string name, string nKod)
        {
            Name = name;
            NKod = nKod;
        }
    }
}
