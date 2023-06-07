using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Checks
{
    public class StudentChecker
    {
        public IEnumerable<StudentError> Check(Student s, Context context)
        {
            if (s.EnrolledCourses == null)
                throw new ModelInconsistencyException("Student.EnrolledCourses not set.");

            foreach(var c in context.Courses)
            {
                if (c.EnrolledStudentNKodsFromNeptun == null)
                    throw new ModelInconsistencyException("Course.EnrolledStudentNKodsFromNeptun not set");
                if (c.EnrolledStudentNKodsFromNeptun.Contains(s.NKod))
                    if (!s.EnrolledCourses.Contains(c))
                        throw new ModelInconsistencyException($"Student.EnrolledCourses does not contain course {c} where student is enrolled.");
            }

            if (s.EnrolledCourses.Count == 0)
                yield return new StudentError() { Message = "Student has no enrolled courses in Neptun", Student = s };

            if (context.Students.Any(st => st.NKod == s.NKod && st != s))
                throw new ModelInconsistencyException($"Neptun code {s.NKod} appears in multiple Student objects!");

            if (context.Gradings == null)
                throw new ModelInconsistencyException("Context.Gradings not set.");
            if (context.Gradings.Count(g => g.StudentNKodFromGrading == s.NKod) == 0)
                yield return new StudentError() { Message = "Student has no gradings", Student = s };

            foreach (var c in s.EnrolledCourses)
            {
                if (c.EnrolledStudentNKodsFromNeptun == null)
                {
                    throw new ModelInconsistencyException("Course.EnrolledStudentNKodsFromNeptun not set");
                }
                else
                {
                    if (!c.EnrolledStudentNKodsFromNeptun.Contains(s.NKod))
                    {
                        yield return new StudentError() { Message = "Student is not enrolled in Neptun", Student = s };
                    }
                }
            }

            if (s.TopicRegistrations == null)
                throw new ModelInconsistencyException("Student.TopicRegistrations not set.");
            if (s.TopicRegistrations.Count == 0)
                yield return new StudentError() { Message = "Student has no topic registrations", Student = s };
            if (s.TopicRegistrations.Count > 1)
                yield return new StudentError() { Message = "Student has more than one topic registration", Student = s };
        }
    }
}
