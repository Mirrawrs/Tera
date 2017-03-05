using System.Threading.Tasks;

namespace Tera.Launcher
{
    /// <summary>
    ///     Exposes methods to obtain launch information for a game publisher.
    /// </summary>
    public interface ILaunchInfoProvider : IAuthProvider
    {
        /// <summary>
        ///     Gets a URI that points to an XML list of realms.
        /// </summary>
        string ServerListUri { get; }

        /// <summary>
        ///     Obtains launch information for the specified user, using the password to authenticate if necessary.
        /// </summary>
        /// <param name="username">The user to obtain launch information about.</param>
        /// <param name="password">The password that will be used to authenticate the user, if necessary.</param>
        /// <returns>The launch information about the specified user.</returns>
        Task<TeraLaunchInfo> GetTeraLaunchInfo(string username, string password);
    }
}