using System;
using System.Security.Cryptography;

namespace Aenima.System
{
    /// <summary>
    ///     Values that represent how the guid will be generated.
    /// </summary>
    public enum SequentialGuidType
    {
        /// <summary>
        ///     For MySQL and PostgreSQL. Should be used with ToString().
        /// </summary>
        /// <remarks>MySQL [char(36)], PostgreSQL [uuid]</remarks>
        SequentialAsString,

        /// <summary>
        ///     For Oracle. Should be used with ToByteArray().
        /// </summary>
        /// <remarks>Oracle [raw(16)]</remarks>
        SequentialAsBinary,

        /// <summary>
        ///     For Microsoft SQL Server.
        /// </summary>
        /// <remarks>Microsoft SQL Server [uniqueidentifier]</remarks>
        SequentialAtEnd
    }

    /// <summary>
    ///     A sequential unique identifier generator. 
    ///     Source: http://www.codeproject.com/Articles/388157/GUIDs-as-fast-primary-keys-under-multiple-database
    /// </summary>
    public static class SequentialGuid
    {
        private static readonly RNGCryptoServiceProvider Rng = new RNGCryptoServiceProvider();

        public static Guid New(SequentialGuidType guidType = SequentialGuidType.SequentialAtEnd)
        {
            var randomBytes = new byte[10];

            Rng.GetBytes(randomBytes);

            var timestamp = DateTime.UtcNow.Ticks / 10000L;
            var timestampBytes = BitConverter.GetBytes(timestamp);

            if(BitConverter.IsLittleEndian)
                Array.Reverse(timestampBytes);

            var guidBytes = new byte[16];

            switch(guidType)
            {
                case SequentialGuidType.SequentialAsString:
                case SequentialGuidType.SequentialAsBinary:
                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 0, 6);
                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 6, 10);

                    // If formatting as a string, we have to reverse the order
                    // of the Data1 and Data2 blocks on little-endian systems.
                    if(guidType == SequentialGuidType.SequentialAsString && BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(guidBytes, 0, 4);
                        Array.Reverse(guidBytes, 4, 2);
                    }

                    break;

                case SequentialGuidType.SequentialAtEnd:
                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 0, 10);
                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 10, 6);
                    break;
            }

            return new Guid(guidBytes);
        }
    }
}