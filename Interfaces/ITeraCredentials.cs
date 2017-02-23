namespace Tera.Interfaces
{
    /// <summary>
    ///     Represents the credentials required for a user to log on a realm.
    /// </summary>
    public interface ITeraCredentials
    {
        /// <summary>
        ///     Gets the user's email address.
        /// </summary>
        string Email { get; }

        /// <summary>
        ///     Gets the user's password.
        /// </summary>
        string Password { get; }
    }
}