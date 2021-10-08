namespace OpenAPI.Events.Server
{
    /// <summary>
    ///     Gets dispatched when the server has fully started up.
    /// </summary>
    public class ServerReadyEvent : Event
    {
        /// <summary>
        ///     The server instance that started up
        /// </summary>
        public OpenServer Server { get; }

        public ServerReadyEvent(OpenServer server)
        {
            Server = server;
        }
    }
    
    /// <summary>
    ///     Gets dispatched when the server is shutting down
    /// </summary>
    public class ServerClosingEvent : Event
    {
        /// <summary>
        ///     The server instance that is shutting down
        /// </summary>
        public OpenServer Server { get; }

        public ServerClosingEvent(OpenServer server)
        {
            Server = server;
        }
    }
}