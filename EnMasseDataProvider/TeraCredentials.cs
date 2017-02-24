namespace Tera.EnMasse
{
    /// <summary>
    ///     Represents the credentials required for a user to log on a realm.
    /// </summary>
    public class TeraCredentials : ITeraCredentials
    {
        /// <summary>
        ///     Initialies a new instance of the <see cref="TeraCredentials" /> class.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        public TeraCredentials(string email, string password)
        {
            Email = email;
            Password = password;
        }

        /// <summary>
        ///     Gets the user's email address.
        /// </summary>
        public string Email { get; }

        /// <summary>
        ///     Gets the user's password.
        /// </summary>
        public string Password { get; }
    }
}