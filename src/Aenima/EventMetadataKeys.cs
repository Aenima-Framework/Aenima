namespace Aenima
{
    public static class EventMetadataKeys
    {
        /// <summary>
        /// The unique identifier of the event.
        /// </summary>
        public const string Id = "event-id";

        /// <summary>
        /// The CLR type of the event used to deserialize it from the store.
        /// </summary>
        public const string ClrType = "event-clr-type";

        /// <summary>
        /// The date and time when the event was raised.
        /// </summary>
        public const string RaisedOn = "raised-on";

        /// <summary>
        /// The owner of the event.
        /// </summary>
        public const string AggregateId = "aggregate-id";

        /// <summary>
        /// The version of the owner of the event after the event was generated.
        /// </summary>
        public const string Version = "aggregate-version";

        /// <summary>
        /// The type name of the owner of the event. The aggregate root that raised the event.
        /// </summary>
        public const string Owner = "owner";

        /// <summary>
        /// The command that affected the owner and therefor the origin of the event.
        /// </summary>
        public const string CommandTypeName = "command-type";

        /// <summary>
        /// The unique identifier of the commit.
        /// </summary>
        public const string CommitId = "commit-id";

        /// <summary>
        /// The unique identifier for the long running process or saga where the event was generated
        /// </summary>
        public const string ProcessId = "process-id";

        /// <summary>
        /// The unique identifier of the stream generated for the owner of the event.
        /// </summary>
        public const string StreamId = "stream-id";
    }
}