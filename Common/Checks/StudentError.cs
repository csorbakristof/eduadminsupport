using Common.Model;

namespace Common.Checks
{
    public class StudentError : ErrorBase
    {
        public Student Student { get; internal set; }

        public override string ToString()
        {
            return $"{base.ToString()}, Student: {Student}";
        }
    }
}