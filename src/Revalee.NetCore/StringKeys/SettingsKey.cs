namespace Revalee.NetCore.StringKeys
{
    internal class SettingsKey
    {
        /// <summary>
        /// Revalee section name
        /// </summary>
        internal const string RevaleeSection = "Revalee";

        /// <summary>
        ///Client Setting sub-section of  <see cref="RevaleeSection"/>
        /// </summary>
        internal const string ClientSection = "ClientSettings";

        /// <summary>
        /// Authorization Key for <see cref="ClientSection"/> sub-section
        /// </summary>
        internal const string AuthorizationKey = "AuthorizationKey";

        /// <summary>
        ///  Request Timeout for <see cref="ClientSection"/> sub-section
        /// </summary>
        internal const string RequestTimeout = "RequestTimeout";

        /// <summary>
        ///  Service Base Uri for <see cref="ClientSection"/> sub-section
        /// </summary>
        internal const string ServiceBaseUri = "ServiceBaseUri";

        //Recurring

        /// <summary>
        ///Recurring Tasks sub-section of  <see cref="RevaleeSection"/>
        /// </summary>
        internal const string RecurringSection = "RecurringTasks";

        /// <summary>
        ///  Callback Base Uri for <see cref="RecurringSection"/> sub-section
        /// </summary>
        internal const string CallbackBaseUri = "CallbackBaseUri";

        /// <summary>
        ///  Tasks List for <see cref="RecurringSection"/> sub-section
        /// </summary>
        internal const string TasksList = "Tasks";

        /// <summary>
        ///  Task's Periodicity for <see cref="TasksList"/> in <see cref="RecurringSection"/> sub-section
        /// </summary>
        internal const string Periodicity = "Periodicity";

        /// <summary>
        ///  Task's Hour for <see cref="TasksList"/> in <see cref="RecurringSection"/> sub-section
        /// </summary>
        internal const string Hour = "Hour";

        /// <summary>
        ///  Task's Minute for <see cref="TasksList"/> in <see cref="RecurringSection"/> sub-section
        /// </summary>
        internal const string Minute = "Minute";

        /// <summary>
        ///  Task's Url for <see cref="TasksList"/> in <see cref="RecurringSection"/> sub-section
        /// </summary>
        internal const string Url = "Url";
    }
}
