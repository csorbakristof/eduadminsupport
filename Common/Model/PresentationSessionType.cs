using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class PresentationSessionType
    {
        public string Title { get; set; }

        public PresentationSessionType(string title)
        {
            Title = title;
        }
    }
}
