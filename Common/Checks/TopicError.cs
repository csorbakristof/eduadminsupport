using Common.Model;

namespace Common.Checks
{
    public class TopicError : ErrorBase
    {
        public Topic Topic { get; internal set; }

        public override string ToString()
        {
            return $"{base.ToString()}, Topic: {Topic}";
        }
    }
}