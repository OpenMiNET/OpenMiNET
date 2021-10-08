using System;

namespace OpenAPI.Events
{
	public interface INotifyExecution
	{
		EventHandler<Event.EventTaskCompleted> OnEventExecution { get; set; }
		void OnComplete(object source);
	}
	
	public abstract class ExecuteableEventBase : Event, INotifyExecution
	{		
		/// <summary>
		///		Invoked when an event has executed it's task.
		/// </summary>
		public EventHandler<Event.EventTaskCompleted> OnEventExecution { get; set; }

		protected ExecuteableEventBase()
		{
			
		}

		void INotifyExecution.OnComplete(object source)
		{
			OnEventExecution?.Invoke(this, new Event.EventTaskCompleted(this, source));
		}
	}
}