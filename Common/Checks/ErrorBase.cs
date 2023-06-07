using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Checks
{
    public class ErrorBase
    {
        public string Message { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
