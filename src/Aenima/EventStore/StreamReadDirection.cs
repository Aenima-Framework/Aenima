﻿namespace Aenima.EventStore
{
    public enum StreamReadDirection
    {
        /// <summary>
        /// From beginning to end.
        /// </summary>
        Forward,
        /// <summary>
        /// From end to beginning.
        /// </summary>
        Backward
    }
}