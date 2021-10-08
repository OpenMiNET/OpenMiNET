using System;
using System.Threading.Tasks;

namespace OpenAPI.Events
{
	/// <summary>
	/// 	The base class all OpenApi Events implement
	/// </summary>
	public abstract class Event
	{
		/// <summary>
		///		True if the current event was cancelled.
		/// </summary>
		public bool IsCancelled { get; set; }

		public Event()
		{
			IsCancelled = false;
		}

		/// <summary>
		///		Set's the Cancelled flag.
		/// </summary>
		/// <param name="isCanceled"></param>
		[Obsolete("Should set IsCancelled directly.")]
		public void SetCancelled(bool isCanceled)
		{
			IsCancelled = isCanceled;
		}

	    public class EventTaskCompleted : EventArgs
	    {
		    /// <summary>
		    ///		The event that finished execution
		    /// </summary>
		    public Event CompletedEvent { get; }
		    
		    /// <summary>
		    ///		The object that executed it.
		    /// </summary>
		    public object Source { get; }
		    
		    internal EventTaskCompleted(Event completedEvent, object source)
		    {
			    CompletedEvent = completedEvent;
			    Source = source;
		    }
	    }
    }
}
