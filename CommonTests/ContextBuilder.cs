using Common.Model;

namespace CommonTests
{
    internal class ContextBuilder
    {
        public ContextBuilder()
        {
        }

        internal Context Build()
        {
            Context c = new Context();

            c.Courses = new List<Course>();
            var s = new Student() { Name = "NotEnrolled", NKod = "NE0000" };
            c.Students.Add(s);
            c.Gradings = new List<Grading>
            {
                new Grading() { Student = s, StudentNKodFromGrading = s.NKod, Grade=5 }
            };

            return c;
        }
    }
}