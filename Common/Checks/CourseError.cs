using Common.Model;

namespace Common.Checks
{
    public class CourseError : ErrorBase
    {
        public Course Course { get; internal set; }

        public override string ToString()
        {
            return $"{base.ToString()}, Course: {Course}";
        }
    }
}