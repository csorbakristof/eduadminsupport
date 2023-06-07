using System.Runtime.Serialization;

namespace Common.Checks
{
    [Serializable]
    internal class ModelInconsistencyException : Exception
    {
        public ModelInconsistencyException()
        {
        }

        public ModelInconsistencyException(string? message) : base(message)
        {
        }

        public ModelInconsistencyException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ModelInconsistencyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}