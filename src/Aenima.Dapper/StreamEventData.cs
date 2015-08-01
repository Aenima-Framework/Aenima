using System;

namespace Aenima.Dapper
{
    internal class StreamEventData
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Data { get; set; }

        public string Metadata { get; set; }

        public DateTime CreatedOn { get; set; }

        public string StreamId { get; set; }

        public int StreamVersion { get; set; }
    }
}