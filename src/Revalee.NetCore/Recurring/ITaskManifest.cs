using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revalee.NetCore.Recurring
{
    public interface ITaskManifest
    {
        /// <summary>
        /// Gets a <see cref="T:System.Boolean" /> value indicating the activation state of the recurring task module.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Gets a <see cref="T:System.Boolean" /> value indicating that there are no recurring tasks defined.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Uri" /> defining the base URL for callbacks.
        /// </summary>
        Uri CallbackBaseUri { get; set; }

        /// <summary>
        /// Gets the enumeration of defined <see cref="T:Revalee.Client.RecurringTasks.IRecurringTask" /> objects.
        /// </summary>
        IEnumerable<IRecurringTask> Tasks { get; }

        /// <summary>
        /// Creates a callback task with a daily recurrence.
        /// </summary>
        /// <param name="hour">A <see cref="T:System.Int32" /> value for the scheduled hour (0-23).</param>
        /// <param name="minute">A <see cref="T:System.Int32" /> value for the scheduled minute (0-59).</param>
        /// <param name="url">A <see cref="T:System.Uri" /> value defining the target of the callback.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="hour" /> is not between 0 and 23.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="minute" /> is not between 0 and 59.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="url" /> is not an absolute URL.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="url" /> contains an unsupported URL scheme.</exception>
        Task AddDailyTaskAsync(int hour, int minute, Uri url);

        /// <summary>
        /// Creates a callback task with a daily recurrence.
        /// </summary>
        /// <param name="minute">A <see cref="T:System.Int32" /> value for the scheduled minute (0-59).</param>
        /// <param name="url">A <see cref="T:System.Uri" /> value defining the target of the callback.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="minute" /> is not between 0 and 59.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="url" /> is not an absolute URL.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="url" /> contains an unsupported URL scheme.</exception>
        Task AddHourlyTaskAsync(int minute, Uri url);

        /// <summary>
        /// Creates a callback task with a daily recurrence.
        /// </summary>
        /// <param name="minute">A <see cref="T:System.Int32" /> value for the scheduled minute (0-59).</param>
        /// <param name="url">A <see cref="T:System.Uri" /> value defining the target of the callback.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="minute" /> is not between 0 and 59.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="url" /> is not an absolute URL.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="url" /> contains an unsupported URL scheme.</exception>
        Task AddWithinHourlyTaskAsync(int minute, Uri url);

        /// <summary>
        /// Creates a callback task with a daily recurrence.
        /// </summary>
        /// <param name="day">A <see cref="T:System.Int32" /> value for the scheduled hour (1-30).</param>
        /// <param name="hour">A <see cref="T:System.Int32" /> value for the scheduled hour (0-23).</param>
        /// <param name="minute">A <see cref="T:System.Int32" /> value for the scheduled minute (0-59).</param>
        /// <param name="url">A <see cref="T:System.Uri" /> value defining the target of the callback.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="minute" /> is not between 0 and 59.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="url" /> is not an absolute URL.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="url" /> contains an unsupported URL scheme.</exception>
        Task AddMonthlyTaskAsync(int day, int hour, int minute, Uri url);

        /// <summary>
        /// Removes a recurring callback task.
        /// </summary>
        /// <param name="identifier">The <see cref="Revalee.Client.RecurringTasks.IRecurringTask.Identifier" /> of the task to be removed.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="identifier" /> is null.</exception>
        Task RemoveTaskAsync(string identifier);

    }
}
