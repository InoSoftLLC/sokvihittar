using System;
using System.Runtime.Serialization;

namespace Sokvihittar
{
    public class SearchRequestStatiscs
    {
        [DataMember(Name = "searchTime")]
        public DateTime Time { get; set; }

        [DataMember(Name = "executionTime")]
        public long ExecutionTime { get; set; }

        [DataMember(Name = "text")]
        public string ProductText { get; set; }

        [DataMember(Name = "limit")]
        public int Limit { get; set; }

        [DataMember(Name = "isTest")]
        public bool IsTest { get; set; }
    }
}
