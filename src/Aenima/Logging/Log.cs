using System;

namespace Aenima.Logging
{
    public static class Log
    {
        static Func<Type, ILog> _loggerFactory;

        public static void Customize(Func<Type, ILog> factory)
        {
            _loggerFactory = factory;
        }

        public static ILog ForContext<T>()
        {
            return _loggerFactory(typeof(T));
        }

        public static ILog ForContext(Type type)
        {
            return _loggerFactory(type);
        } 
    }
}