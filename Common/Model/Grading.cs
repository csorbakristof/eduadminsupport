using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    [DataContract(IsReference = true)]
    public class Grading
    {
        [DataMember]
        public string StudentNKodFromGrading { get; set; }

        [DataMember]
        public string ClassCodeInGrading { get; set; }

        [DataMember]
        public int? Grade { get; set; }

        override public string ToString()
        {
            return $"(Grading {StudentNKodFromGrading} in {ClassCodeInGrading}: {Grade})";
        }
    }
}
