using System;
using System.Collections.Generic;
using System.Text;

namespace MessageConsumers.Models
{
    public class ExpectedMessageBody
    {
        public string Source { get; set; }
        public DateTime TimeStamp { get; set; }
        public Guid Guid { get; set; }
        public string Message { get; set; }
    }
}
