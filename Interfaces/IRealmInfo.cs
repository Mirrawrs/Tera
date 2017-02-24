namespace Tera
{
    /// <summary>
    ///     Represents the connection information for a realm.
    /// </summary>
    public interface IRealmInfo
    {
        /// <summary>
        ///     Gets the host of the realm's server.
        /// </summary>
        string Host { get; }

        /// <summary>
        ///     Gets the port the realm's server is listening on.
        /// </summary>
        int Port { get; }

        /// <summary>
        ///     Gets the realm's name.
        /// </summary>
        string Name { get; }
    }
}