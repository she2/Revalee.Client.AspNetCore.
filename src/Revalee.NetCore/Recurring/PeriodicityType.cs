namespace Revalee.NetCore.Recurring
{
    /// <summary>
	/// Represents the periodicity of a recurring task.
	/// </summary>
	public enum PeriodicityType
    {
        /// <summary>
        /// An recurrence schedule tha can occur several times within an hour
        /// </summary>
        WithinHourly,

        /// <summary>
        /// An hourly recurrence schedule.
        /// </summary>
        Hourly,

        /// <summary>
        /// A daily recurrence schedule.
        /// </summary>
        Daily,

        /// <summary>
        /// A monthly recurrence schedule.
        /// </summary>
        Monthly,
    }
}
