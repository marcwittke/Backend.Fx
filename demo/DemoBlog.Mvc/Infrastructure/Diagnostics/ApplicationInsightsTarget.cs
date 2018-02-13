namespace DemoBlog.Mvc.Infrastructure.Diagnostics
{
    using System;
    using System.Globalization;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using NLog;
    using NLog.Targets;

    // stolen: https://github.com/Microsoft/ApplicationInsights-dotnet-logging/blob/develop/src/Adapters/NLogTarget.Shared/ApplicationInsightsTarget.cs
    // no dotnetcore-package yet available, therefore implemented manually

    /// <summary>
    ///     NLog Target that routes all logging output to the Application Insights logging framework.
    ///     The messages will be uploaded to the Application Insights cloud service.
    /// </summary>
    [Target("ApplicationInsightsTarget")]
    public sealed class ApplicationInsightsTarget : TargetWithLayout
    {
        /// <summary>
        ///     Initializers a new instance of ApplicationInsightsTarget type.
        /// </summary>
        public ApplicationInsightsTarget()
        {
            Layout = @"${message}";
        }

        /// <summary>
        ///     The Application Insights instrumentationKey for your application.
        /// </summary>
        public string InstrumentationKey { get; set; }

        /// <summary>
        ///     The logging controller we will be using.
        /// </summary>
        internal TelemetryClient TelemetryClient { get; private set; }

        /// <summary>
        ///     Initializes the Target and perform instrumentationKey validation.
        /// </summary>
        /// <exception cref="NLogConfigurationException">Will throw when <see cref="InstrumentationKey" /> is not set.</exception>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            TelemetryClient = new TelemetryClient();
            if (!string.IsNullOrEmpty(InstrumentationKey))
            {
                TelemetryClient.Context.InstrumentationKey = InstrumentationKey;
            }
        }

        /// <summary>
        ///     Send the log message to Application Insights.
        /// </summary>
        /// <exception cref="ArgumentNullException">If <paramref name="logEvent" /> is null.</exception>
        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            if (logEvent.Exception != null)
            {
                SendException(logEvent);
            }
            else
            {
                SendTrace(logEvent);
            }
        }

        private void BuildPropertyBag(LogEventInfo logEvent, ITelemetry trace)
        {
            trace.Timestamp = logEvent.TimeStamp;
            trace.Sequence = logEvent.SequenceID.ToString(CultureInfo.InvariantCulture);

            var telemetry = trace as ExceptionTelemetry;
            var propertyBag = telemetry != null
                ? telemetry.Properties
                : ((TraceTelemetry)trace).Properties;

            if (!string.IsNullOrEmpty(logEvent.LoggerName))
            {
                propertyBag.Add("LoggerName", logEvent.LoggerName);
            }

            if (logEvent.UserStackFrame != null)
            {
                propertyBag.Add("UserStackFrame", logEvent.UserStackFrame.ToString());
                propertyBag.Add("UserStackFrameNumber", logEvent.UserStackFrameNumber.ToString(CultureInfo.InvariantCulture));
            }

            var properties = logEvent.Properties;
            if (properties != null)
                foreach (var keyValuePair in properties)
                {
                    var key = keyValuePair.Key.ToString();
                    var valueObj = keyValuePair.Value;
                    if (valueObj == null)
                    {
                        continue;
                    }

                    var value = valueObj.ToString();
                    if (propertyBag.ContainsKey(key))
                    {
                        if (value == propertyBag[key])
                        {
                            continue;
                        }
                        key += "_1";
                    }
                    propertyBag.Add(key, value);
                }
        }

        private SeverityLevel? GetSeverityLevel(LogLevel logEventLevel)
        {
            if (logEventLevel == null)
                return null;

            if ((logEventLevel.Ordinal == LogLevel.Trace.Ordinal) ||
                (logEventLevel.Ordinal == LogLevel.Debug.Ordinal))
            {
                return SeverityLevel.Verbose;
            }

            if (logEventLevel.Ordinal == LogLevel.Info.Ordinal)
            {
                return SeverityLevel.Information;
            }

            if (logEventLevel.Ordinal == LogLevel.Warn.Ordinal)
            {
                return SeverityLevel.Warning;
            }

            if (logEventLevel.Ordinal == LogLevel.Error.Ordinal)
            {
                return SeverityLevel.Error;
            }

            if (logEventLevel.Ordinal == LogLevel.Fatal.Ordinal)
            {
                return SeverityLevel.Critical;
            }

            // The only possible value left if OFF but we should never get here in this case
            return null;
        }

        private void SendException(LogEventInfo logEvent)
        {
            var exceptionTelemetry = new ExceptionTelemetry(logEvent.Exception)
            {
                SeverityLevel = GetSeverityLevel(logEvent.Level)
            };

            var logMessage = Layout.Render(logEvent);
            exceptionTelemetry.Properties.Add("Message", logMessage);

            BuildPropertyBag(logEvent, exceptionTelemetry);
            TelemetryClient.Track(exceptionTelemetry);
        }

        private void SendTrace(LogEventInfo logEvent)
        {
            var logMessage = Layout.Render(logEvent);

            var trace = new TraceTelemetry(logMessage)
            {
                SeverityLevel = GetSeverityLevel(logEvent.Level)
            };

            BuildPropertyBag(logEvent, trace);
            TelemetryClient.Track(trace);
        }
    }
}
