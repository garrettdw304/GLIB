namespace GLIB.Scheduler
{
	/// <summary>
	/// 
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

		public void Update(T currentTime)
		{
			int checks = 0;
			Check(checks, currentTime);
		}

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

		public void SetMaxChecks(int maxChecks)
		{
			this.maxChecks = maxChecks;
		}

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
