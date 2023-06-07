using System.Runtime.Serialization;

namespace Common.Model
{
    [DataContract]
    public class Advisor
    {
        [DataMember]
        public string Name { get; set; }

        public Advisor(string name)
        {
            Name = name;
        }

        public Advisor()
        {
            Name = string.Empty;
        }
    }
}
