namespace GLIB.Scheduler
{
	/// <summary>
	/// Keeps track of a list of events to be ran at specific times. Update() must be called on a regular interval to allow the scheduler to call events that need executed.
	/// <para/>This class is not thread safe.
	/// </summary>
	/// <typeparam name="T">The data structure of time. Could be float for use with Unity's time system or DateTime etc, as long as it implements IComparable.</typeparam>
	public class Scheduler<T> where T : IComparable
	{
		private int maxChecks = 1000;

		private List<Event> schedule;
		private ulong nextEventId;
		private T nextEventsTimeToExecute;

		public Scheduler()
		{
			schedule = new List<Event>();
			nextEventId = 0;
		}

		/// <summary>
		/// To be called on a regular interval. Unity's Update method or Godot's _Process method are good candidates.
		/// </summary>
		/// <param name="currentTime">The current time to compare against queued events' execute times.</param>
		public void Update(T currentTime)
		{
			int checks = 0;
			Check(checks, currentTime);
		}

		/// <summary>
		/// Schedules an action to be executed at a specified time.
		/// </summary>
		/// <param name="timeToExecute">The time at which the action should be executed.</param>
		/// <param name="action">The action to be executed.</param>
		/// <returns>A ulong used to cancel the action with Cancel().</returns>
		public ulong Schedule(T timeToExecute, Action action)
		{
			ulong eventId = nextEventId++;

			int i;
			for (i = 0; i < schedule.Count; i++)
			{
				if (schedule[i].timeToExecute.CompareTo(timeToExecute) > 0) // [i].timeToExecute > timeToExecute
					break;
			}

			schedule.Insert(i, new Event(timeToExecute, action, eventId));

			nextEventsTimeToExecute = schedule[0].timeToExecute;

			return eventId;
		}

		/// <summary>
		/// Cancels the action associated with the proveded event id.
		/// </summary>
		/// <param name="eventId">The event id that was provided when scheduling the event with Schedule().</param>
		/// <returns>True if the event has not been run yet and was canceled. False if the event has already ran.</returns>
		public bool Cancel(ulong eventId)
		{
			for (int i = 0; i < schedule.Count; i++)
				if (schedule[i].id == eventId)
				{
					schedule.RemoveAt(i);
					if (schedule.Count > 0)
						nextEventsTimeToExecute = schedule[0].timeToExecute;

					return true;
				}

			return false;
		}

		/// <summary>
		/// Replaces an action associated with the event id.
		/// </summary>
		/// <param name="newAction">The action to replace the old action with.</param>
		/// <param name="eventId">The event id of the event to replace the action of.</param>
		/// <returns>True if the event has not been run yet and the action was replaced. False if the action has already ran.</returns>
		public bool ReplaceAction(Action newAction, ulong eventId)
		{
			foreach (Event e in schedule)
			{
				if (e.id == eventId)
				{
					e.action = newAction;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Updates the maximum checks performed in one call to Update.
		/// <para/>A single check is an execution of one event. If there are more events to be executed than maxChecks, it is likely an event is queuing itself to be ran again right away when executed, causing an infinite loop. If maxChecks is reached, the schedule is cleared to fix the endless loop and MaxChecksReachedException is thrown.
		/// </summary>
		/// <param name="maxChecks"></param>
		public void SetMaxChecks(int maxChecks)
		{
			this.maxChecks = maxChecks;
		}

        /// <summary>
        /// Checks if it is time to execute the next event, and if so, executes it. Calls itself if the event was executed to see if it is also time to execute the next event.
        /// <para/>Due to the recursiveness of this method, infinite loops are bound to pop up during development, potentially crashing the game engine's editor (which is exactly what happens in the Unity editor in the event of an endless loop during in-editor game testing). maxChecks defines a maximum number of times this method can be called recursively before an endless loop is assumed. If maxChecks is reached, the schedule is cleared to stop the endless loop and an exception is thrown. The maxChecks can be increased if it is expected that the default number of 1000 checks per frame is to be exceeded.
        /// </summary>
        /// <param name="checks"></param>
        /// <param name="currentTime"></param>
        /// <exception cref="MaxChecksReachedException"/>
        /// <exception cref="Exception"></exception>
        private void Check(int checks, T currentTime)
		{
			if (checks == maxChecks)
			{
				schedule.Clear();
				throw new MaxChecksReachedException("Possible endless loop detected! schedule cleared. If this was not an error, increase the max checks.");
			}

			if (schedule.Count == 0 || currentTime.CompareTo(nextEventsTimeToExecute) < 0)
				return;

			Event e = schedule[0];
			schedule.RemoveAt(0);

			nextEventsTimeToExecute = schedule[0].timeToExecute;
			
			try
			{
				e.action();
			}
			catch(Exception ex)
			{
				throw new Exception("Error while trying to execute event's action.", ex);
			}
			finally
			{
				if (schedule.Count > 0)
					Check(++checks, currentTime);
			}
		}

		/// <summary>
		/// A structure holding some info on a queued event.
		/// </summary>
		private class Event
		{
			public readonly ulong id;
			public Action action;
			public T timeToExecute;

			public Event(T timeToExecute, Action action, ulong id)
			{
				this.id = id;
				this.action = action;
				this.timeToExecute = timeToExecute;
			}
		}

		/// <summary>
		/// An exception thrown when maxChecks has been reached.
		/// </summary>
		public class MaxChecksReachedException : Exception
		{
			public MaxChecksReachedException(string message) : base(message) { }
		}
	}
}
