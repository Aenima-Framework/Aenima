using System;

namespace Aenima.Logging
{
    public static class Log
    {
        static Func<Type, ILog> loggerFactory;

        public static void Customize(Func<Type, ILog> factory)
        {
            loggerFactory = factory;
        }

        public static ILog ForContext<T>()
        {
            return loggerFactory(typeof(T));
        }

        public static ILog ForContext(Type type)
        {
            return loggerFactory(type);
        } 
    }
}