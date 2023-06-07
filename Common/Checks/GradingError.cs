using Common.Model;

namespace Common.Checks
{
    public class GradingError : ErrorBase
    {
        public Grading Grading { get; internal set; }

        public override string ToString()
        {
            return $"{base.ToString()}, Grading: {Grading}";
        }
    }
}