using System;

namespace Aenima.EventStore
{
    ///// <summary>
    ///// Represents an event to be written to the store.
    ///// </summary>
    //public class NewStreamEvent
    //{
    //    public readonly Guid Id;
    //    public readonly string Type;
    //    public readonly string Data;
    //    public readonly string Metadata;

    //    public NewStreamEvent(Guid id, string type, string data, string metadata)
    //    {
    //        Id       = id;
    //        Type     = type;
    //        Data     = data;
    //        Metadata = metadata;
    //    }
    //}

    public class StreamEventData
    {
        public readonly Guid Id;
        public readonly string Type;
        public readonly string Data;
        public readonly string Metadata;
        public readonly DateTime? CreatedOn;

        public StreamEventData(Guid id, string type, string data, string metadata, DateTime? createdOn = null)
        {
            Id        = id;
            Type      = type;
            Data      = data;
            Metadata  = metadata;
            CreatedOn = createdOn;
        }
    }
}