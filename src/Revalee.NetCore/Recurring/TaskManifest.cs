using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Revalee.NetCore.Internal;
using Revalee.NetCore.Settings;
using Revalee.NetCore.Startup;
using Revalee.NetCore.StringKeys;

namespace Revalee.NetCore.Recurring
{
    internal sealed class TaskManifest : ITaskManifest
    {

        const string _recurringTaskHandlerPath = "/__RevaleeRecurring.axd/";

        Guid _id;
        ActivationState _currentState;
        IClockSource _clockSource;
        string _recurringTaskHandlerAbsolutePath;
        Timer _heartbeatTimer;
        int _heartbeatCount;
        Uri _callbackBaseUri;

        readonly ITaskCollection _taskCollection;
        readonly IRevaleeRegistrar _revalee;
        readonly RevaleeOptions _option;
        readonly HttpContext _context;

        #region Constructor
        internal TaskManifest(IRevaleeRegistrar revalee, HttpContext context, IOptions<RevaleeOptions> option)
        {
            InitVariables();

            _option = option.Value;
            _revalee = revalee;
            _context = context;
            _taskCollection = new ImmutableTaskCollection();
        }

        internal TaskManifest(IRevaleeRegistrar revalee, HttpContext context, IOptions<RevaleeOptions> option, IRevaleeClientRecurringSettings configuredTasks)
        {

            if (configuredTasks == null)
            {
                throw new ArgumentNullException(nameof(configuredTasks));
            }

            _option = option.Value;
            _revalee = revalee;
            _context = context;
            _callbackBaseUri = configuredTasks.CallbackBaseUri;

            InitVariables();

            var taskList = new List<ConfiguredTask>();

            using (var taskBuilder = new TaskBuilder(_callbackBaseUri))
            {
                foreach (var task in configuredTasks.TaskModel)
                {
                    taskList.Add(taskBuilder.Create(_clockSource, task.Periodicity, task.Day.GetValueOrDefault(), task.Hour.GetValueOrDefault(), task.Minute, task.Url));

                    if (CallbackBaseUri == null)
                    {
                        ScavengeForCallbackBaseUri(task.Url);
                    }
                }
            }

            _taskCollection = new ImmutableTaskCollection(taskList);
        }
        #endregion

        #region ITaskManifest

        public Uri CallbackBaseUri
        {
            get
            {
                return _callbackBaseUri;
            }
            set
            {
                Interlocked.Exchange(ref _callbackBaseUri, value);
            }
        }

        public bool IsActive => _currentState.IsActive;

        public bool IsEmpty => (_taskCollection.Count == 0);

        public IEnumerable<IRecurringTask> Tasks => _taskCollection.Tasks;

        public async Task AddDailyTaskAsync(int hour, int minute, Uri url)
        {
            if (hour < 0 || hour > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(hour));
            }

            if (minute < 0 || minute > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(minute));
            }

            if (this.CallbackBaseUri == null && !url.IsAbsoluteUri)
            {
                throw new ArgumentException("URL must be absolute if no CallbackBaseUri is set.");
            }

            if (url.IsAbsoluteUri && url.Scheme != UriKeys.UriSchemeHttp && url.Scheme != UriKeys.UriSchemeHttps)
            {
                throw new ArgumentException("Unsupported URL scheme.");
            }

            await AddTaskAsync(_clockSource, PeriodicityType.Daily, 0, hour, minute, url);
        }

        public async Task AddHourlyTaskAsync(int minute, Uri url)
        {
            if (minute < 0 || minute > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(minute));
            }

            if (this.CallbackBaseUri == null && !url.IsAbsoluteUri)
            {
                throw new ArgumentException("URL must be absolute if no CallbackBaseUri is set.");
            }

            if (url.IsAbsoluteUri && url.Scheme != UriKeys.UriSchemeHttp && url.Scheme != UriKeys.UriSchemeHttps)
            {
                throw new ArgumentException("Unsupported URL scheme.");
            }

            await AddTaskAsync(_clockSource, PeriodicityType.Hourly, 0, 0, minute, url);
        }

        public async Task AddWithinHourlyTaskAsync(int minute, Uri url)
        {
            if (minute < 0 || minute > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(minute));
            }

            if (this.CallbackBaseUri == null && !url.IsAbsoluteUri)
            {
                throw new ArgumentException("URL must be absolute if no CallbackBaseUri is set.");
            }

            if (url.IsAbsoluteUri && url.Scheme != UriKeys.UriSchemeHttp && url.Scheme != UriKeys.UriSchemeHttps)
            {
                throw new ArgumentException("Unsupported URL scheme.");
            }
            await AddTaskAsync(_clockSource, PeriodicityType.WithinHourly, 0, 0, minute, url);
        }

        public async Task AddMonthlyTaskAsync(int day, int hour, int minute, Uri url)
        {
            if (day <= 0 || day > 30)
            {
                throw new ArgumentOutOfRangeException(nameof(day));
            }

            if (hour < 0 || hour > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(hour));
            }

            if (minute < 0 || minute > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(minute));
            }

            if (this.CallbackBaseUri == null && !url.IsAbsoluteUri)
            {
                throw new ArgumentException("URL must be absolute if no CallbackBaseUri is set.");
            }

            if (url.IsAbsoluteUri && url.Scheme != UriKeys.UriSchemeHttp && url.Scheme != UriKeys.UriSchemeHttps)
            {
                throw new ArgumentException("Unsupported URL scheme.");
            }
            await AddTaskAsync(_clockSource, PeriodicityType.WithinHourly, day, hour, minute, url);
        }


        public async Task RemoveTaskAsync(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            await _taskCollection.RemoveAsync(identifier);
        }

        #endregion

        #region Internal Only

        internal bool TryGetTask(string identifier, out ConfiguredTask taskConfig)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                taskConfig = null;
                return false;
            }

            return _taskCollection.TryGetTask(identifier, out taskConfig);
        }

        internal async Task Start()
        {
            if (CallbackBaseUri != null)
            {
                if (!IsActive)
                {
                    if (_heartbeatTimer == null)
                    {
                        // Schedule a heartbeat on a timer
                        lock (_taskCollection)
                        {
                            if (_heartbeatTimer == null)
                            {
                                _heartbeatTimer = new Timer(delegate (object self)
                                {
                                    try
                                    {
                                        if (IsActive)
                                        {
                                            lock (_taskCollection)
                                            {
                                                if (_heartbeatTimer != null)
                                                {
                                                    _heartbeatTimer.Dispose();
                                                    _heartbeatTimer = null;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (_heartbeatTimer == null)// || AppDomain.CurrentDomain.IsFinalizingForUnload())
                                            {
                                                return;
                                            }

                                            int failureCount = Interlocked.Increment(ref _heartbeatCount) - 1;

                                            lock (_taskCollection)
                                            {
                                                if (_heartbeatTimer != null)
                                                {
                                                    if (_heartbeatCount > 20)
                                                    {
                                                        // Leave current timer setting in-place
                                                    }
                                                    else if (_heartbeatCount > 13)
                                                    {
                                                        _heartbeatTimer.Change(3600000, 14400000);
                                                    }
                                                    else if (_heartbeatCount > 3)
                                                    {
                                                        _heartbeatTimer.Change(60000, 60000);
                                                    }
                                                    else if (_heartbeatCount > 2)
                                                    {
                                                        _heartbeatTimer.Change(49750, 60000);
                                                    }
                                                }
                                            }

                                            if (failureCount > 0)
                                            {
                                                OnActivationFailure(failureCount);
                                            }

                                            try
                                            {
                                                var id = _revalee.RequestCallbackAsync(GenerateHeartbeatCallbackUri(), _clockSource.Now).Result;
                                            }
                                            catch (RevaleeRequestException)
                                            {
                                                // Ignore network errors and retry based on the timer schedule
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // Ignore errors when already shutting down
                                    }

                                }, null, 250, 10000);
                            }
                        }
                    }
                    else
                    {
                        // Schedule an on-demand heartbeat
                        await Schedule(this.PrepareHeartbeat());
                    }
                }
            }
        }

        internal async Task<RequestAnalysis> AnalyzeRequest(HttpRequest request)
        {
            string absolutePath = request.Path;//request.Url.AbsolutePath;

            if (absolutePath.StartsWith(_recurringTaskHandlerAbsolutePath, StringComparison.Ordinal))
            {
                var analysis = new RequestAnalysis();
                analysis.IsRecurringTask = true;
                int parameterStartingIndex = _recurringTaskHandlerAbsolutePath.Length;

                if (absolutePath.Length > parameterStartingIndex)
                {
                    // AbsolutePath format:
                    // task       -> ~/__RevaleeRecurring.axd/{identifier}/{occurrence}
                    // heartbeat  -> ~/__RevaleeRecurring.axd/{heartbeatId}

                    int taskParameterDelimiterIndex = absolutePath.IndexOf('/', parameterStartingIndex);

                    if (taskParameterDelimiterIndex < 0)
                    {
                        // no task parameter delimiter

                        if ((absolutePath.Length - parameterStartingIndex) == 32)
                        {
                            Guid heartbeatId;

                            if (Guid.TryParseExact(absolutePath.Substring(parameterStartingIndex), "N", out heartbeatId))
                            {
                                if (heartbeatId.Equals(_id))
                                {
                                    await OnActivate();
                                }
                            }
                        }
                    }
                    else
                    {
                        // task parameter delimiter present

                        if ((absolutePath.Length - taskParameterDelimiterIndex) > 1)
                        {
                            if (long.TryParse(absolutePath.Substring(taskParameterDelimiterIndex + 1), NumberStyles.None, CultureInfo.InvariantCulture, out analysis.Occurrence))
                            {
                                analysis.TaskIdentifier = absolutePath.Substring(parameterStartingIndex, taskParameterDelimiterIndex - parameterStartingIndex);
                            }
                        }
                    }
                }

                // If the TaskIdentifier is not set the default will be string.Empty, which will be discarded by the HttpModule

                return analysis;
            }

            return new RequestAnalysis();
        }

        internal async Task Reschedule(ConfiguredTask taskConfig)
        {
            await Schedule(PrepareNextCallback(taskConfig));
        }

        #endregion

        #region Private only

        async Task AddTaskAsync(IClockSource clockSource, PeriodicityType periodicity, int day, int hour, int minute, Uri url)
        {
            using (var taskBuilder = new TaskBuilder(CallbackBaseUri))
            {
                ConfiguredTask taskConfig = taskBuilder.Create(clockSource, periodicity, day, hour, minute, url);

                if (await _taskCollection.AddAsync(taskConfig))
                {
                    if (CallbackBaseUri == null)
                    {
                        ScavengeForCallbackBaseUri(taskConfig.Url);
                    }

                    await Schedule(PrepareNextCallback(taskConfig));

                    if (!_currentState.IsActive)
                    {
                        await Start();
                    }
                }
            }
        }

        async Task OnActivate()
        {
            _heartbeatCount = 0;

            if (_currentState.TransitionToActive())
            {
                //Trace.TraceInformation("The Revalee recurring task manager has been activated.");

                foreach (ConfiguredTask taskConfig in _taskCollection.Tasks)
                {
                    if (!taskConfig.HasOccurred)
                    {
                        await Schedule(PrepareNextCallback(taskConfig));
                    }
                }

                _option?.TaskEvent.Activated();
            }
        }

        async Task OnDeactivate(RevaleeRequestException exception)
        {
            //Trace.TraceError("A Revalee recurring task could not be scheduled.");

            if (_currentState.TransitionToInactive())
            {
                await Start();

                _option?.TaskEvent.Deactivated(new DeactivationEventArgs(exception));
            }
        }

        void OnActivationFailure(int failureCount)
        {
            _option?.TaskEvent.ActivationFailed(new ActivationFailureEventArgs(failureCount));
        }

        void ScavengeForCallbackBaseUri(Uri url)
        {
            if (url.IsAbsoluteUri)
            {
                Uri baseUri;
                string leftPart = url.Authority;

                if (Uri.TryCreate(leftPart, UriKind.Absolute, out baseUri))
                {
                    CallbackBaseUri = baseUri;
                }
            }
        }

        async Task Schedule(CallbackRequest callbackDetails)
        {
            try
            {
                await _revalee.RequestCallbackAsync(callbackDetails.CallbackUri, callbackDetails.CallbackTime);
            }
            catch (RevaleeRequestException exception)
            {
                await OnDeactivate(exception);
            }
        }

        CallbackRequest PrepareHeartbeat()
        {
            Uri callbackUri = this.GenerateHeartbeatCallbackUri();
            DateTimeOffset callbackTime = _clockSource.Now;
            return new CallbackRequest(callbackTime, callbackUri);
        }

        Uri GenerateHeartbeatCallbackUri() => new Uri(string.Concat(
    this.CallbackBaseUri.Scheme,
    UriKeys.SchemeDelimiter,
    this.CallbackBaseUri.Authority,
    _recurringTaskHandlerAbsolutePath,
    _id.ToString("N")));

        private CallbackRequest PrepareNextCallback(ConfiguredTask taskConfig)
        {
            long occurrence = taskConfig.GetNextOccurrence();
            Uri callbackUri = BuildTaskCallbackUri(taskConfig, occurrence);
            DateTimeOffset callbackTime = new DateTimeOffset(occurrence, TimeSpan.Zero);
            return new CallbackRequest(callbackTime, callbackUri);
        }

        private Uri BuildTaskCallbackUri(ConfiguredTask taskConfig, long occurrence)
        {
            if (!taskConfig.Url.IsAbsoluteUri && _context != null && _context.Request != null)
            {
                string callbackUrlLeftPart = $"{_context.Request.Scheme}{UriKeys.SchemeDelimiter}{_context.Request.Host.ToUriComponent()}";

                return new Uri(string.Concat(
                    callbackUrlLeftPart,
                    _recurringTaskHandlerAbsolutePath,
                    taskConfig.Identifier,
                    "/",
                    occurrence.ToString(CultureInfo.InvariantCulture)
                    ), UriKind.Absolute);
            }

            return new Uri(string.Concat(
                taskConfig.Url.Scheme,
                UriKeys.SchemeDelimiter,
                taskConfig.Url.Authority,
                _recurringTaskHandlerAbsolutePath,
                taskConfig.Identifier,
                "/",
                occurrence.ToString(CultureInfo.InvariantCulture)
                ));
        }

        private string GetHandlerAbsolutePath()
        {
            string virtualRoot = _context.Request.Path.Value;//.RequestServices.GetService(typeof(IHostingEnvironment))).ContentRootPath;

            if (string.IsNullOrEmpty(virtualRoot) || virtualRoot[0] == '/')
            {
                return _recurringTaskHandlerPath;
            }

            if (virtualRoot[virtualRoot.Length - 1] == '/')
            {
                return string.Concat(virtualRoot, _recurringTaskHandlerPath.Substring(1));
            }

            return virtualRoot;
        }

        void InitVariables()
        {
            _id = Guid.NewGuid();
            _currentState = new ActivationState(false);
            _clockSource = SystemClock.Instance;
            _recurringTaskHandlerAbsolutePath = GetHandlerAbsolutePath();
            _heartbeatCount = 0;
        }
        #endregion
    }
}
