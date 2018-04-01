namespace OpenAPI.Events
{
	public interface ICancellable
	{
		bool IsCancelled { get; set; }
		void SetCancelled(bool value);
	}
}
