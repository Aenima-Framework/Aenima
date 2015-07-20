namespace Aenima
{
    public static class EventMetadataKeys
    { 
        public const string Id                   = "event-id";
        //public const string SequenceNumber       = "sequence-number"; // it's the event-version. no need for it to exist
        //public const string GlobalSequenceNumber = "global-sequence-number";
        public const string Version              = "event-version";
        public const string ClrType              = "event-clr-type";
        public const string RaisedOn             = "raised-on";
        public const string AggregateId          = "aggregate-id";
        public const string AggregateVersion     = "aggregate-version";
        public const string AggregateClrType     = "aggregate-clr-type";
        public const string CommandTypeName      = "command-type";
        public const string CommitId             = "commit-id";
    }
}