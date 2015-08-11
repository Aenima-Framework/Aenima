using System;
using System.Threading.Tasks;
using Aenima.Logging;
using Serilog;

namespace Aenima.Serilog
{
    public class SerilogLog : ILog
    {
        private readonly ILogger _logger;

        public SerilogLog(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     Logs the most detailed level of diagnostic information.
        /// </summary>
        /// <param name="message">The diagnostic message to be logged.</param>
        /// <param name="values">All parameter to be formatted into the message, if any.</param>
        public Task Verbose(string message, params object[] values)
        {
            _logger.Verbose(message, values);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Logs the debug-level diagnostic information.
        /// </summary>
        /// <param name="message">The diagnostic message to be logged.</param>
        /// <param name="values">All parameter to be formatted into the message, if any.</param>
        public Task Debug(string message, params object[] values)
        {
            _logger.Debug(message, values);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Logs important runtime diagnostic information.
        /// </summary>
        /// <param name="message">The diagnostic message to be logged.</param>
        /// <param name="values">All parameter to be formatted into the message, if any.</param>
        public Task Information(string message, params object[] values)
        {
            _logger.Information(message, values);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Logs diagnostic issues to which attention should be paid.
        /// </summary>
        /// <param name="message">The diagnostic message to be logged.</param>
        /// <param name="values">All parameter to be formatted into the message, if any.</param>
        public Task Warning(string message, params object[] values)
        {
            _logger.Warning(message, values);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Logs diagnostic issues to which attention should be paid.
        /// </summary>
        /// <param name="exception">The relevant exception.</param>
        /// <param name="message">The diagnostic message to be logged.</param>
        /// <param name="values">All parameter to be formatted into the message, if any.</param>
        public Task Warning(Exception exception, string message, params object[] values)
        {
            _logger.Warning(exception, message, values);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Logs application and infrastructure-level errors.
        /// </summary>
        /// <param name="message">The diagnostic message to be logged.</param>
        /// <param name="values">All parameter to be formatted into the message, if any.</param>
        public Task Error(string message, params object[] values)
        {
            _logger.Error(message, values);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Logs application and infrastructure-level errors.
        /// </summary>
        /// sempre conseguiste? o Geada ajudou te?
        /// <param name="exception">The relevant exception.</param>
        /// <param name="message">The diagnostic message to be logged.</param>
        /// <param name="values">All parameter to be formatted into the message, if any.</param>
        public Task Error(Exception exception, string message, params object[] values)
        {
            _logger.Error(exception, message, values);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Logs fatal errors which result in process termination.
        /// </summary>
        /// <param name="message">The diagnostic message to be logged.</param>
        /// <param name="values">All parameter to be formatted into the message, if any.</param>
        public Task Fatal(string message, params object[] values)
        {
            _logger.Fatal(message, values);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Logs fatal errors which result in process termination.
        /// </summary>
        /// <param name="exception">The relevant exception.</param>
        /// <param name="message">The diagnostic message to be logged.</param>
        /// <param name="values">All parameter to be formatted into the message, if any.</param>
        public Task Fatal(Exception exception, string message, params object[] values)
        {
            _logger.Fatal(exception, message, values);
            return Task.CompletedTask;
        }
    }
}