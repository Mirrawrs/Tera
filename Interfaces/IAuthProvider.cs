using System.Threading.Tasks;

namespace Tera
{
    /// <summary>
    ///     Exposes methods to authenticate on the game publisher's server.
    /// </summary>
    public interface IAuthProvider
    {
        /// <summary>
        ///     Authenticates on the game publisher's server and gets a one-time ticket used to log on a realm.
        /// </summary>
        /// <param name="username">The user's name.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The authentication ticket.</returns>
        Task<string> Authenticate(string username, string password);
    }
}