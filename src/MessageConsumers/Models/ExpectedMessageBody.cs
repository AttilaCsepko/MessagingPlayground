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
    public class ExtendedMessageBody : ExpectedMessageBody
    {
        public string Processor { get; set; }
    }

    public class ExtendedMessageBodyForCosmos : ExtendedMessageBody
    {
        //NOTE: Azure CosmosDB records always should have an id field. 
        public string id { get; set; } = Guid.NewGuid().ToString();
    }
}
