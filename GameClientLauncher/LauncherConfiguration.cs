namespace Tera.Launcher
{
    /// <summary>
    ///     Contains parameters that will be used by a <see cref="Launcher" />.
    /// </summary>
    public class LauncherConfiguration
    {
        /// <summary>
        ///     Gets or sets the username that will be passed to the <see cref="LaunchInfoProvider" />.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     Gets or sets the password that will be passed to the <see cref="LaunchInfoProvider" />.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Gets or sets the path of TL.exe. It is commonly located in the TERA/Client directory.
        /// </summary>
        public string TeraLauncherPath { get; set; }

        /// <summary>
        ///     Gets or sets the object responsible of providing launch information.
        /// </summary>
        public ILaunchInfoProvider LaunchInfoProvider { get; set; }
    }
}