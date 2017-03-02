using System;
using System.Collections.Generic;

namespace Tera.Net
{
    /// <summary>
    ///     Allows to specify parameters that will be used by a <see cref="TeraClient" />.
    /// </summary>
    public class TeraClientConfiguration
    {
        /// <summary>
        ///     Gets or sets the object responsible of authenticating the client.
        /// </summary>
        public IAuthProvider AuthProvider { get; set; }

        /// <summary>
        ///     Gets or sets the client's username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     Gets or sets the client's password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Gets or sets the realm the client will connect to.
        /// </summary>
        public IRealmInfo Realm { get; set; }

        /// <summary>
        ///     Gets or sets the dictionary mapping opcodes to packet names.
        /// </summary>
        public IReadOnlyDictionary<ushort, string> PacketNamesByOpcode { get; set; }

        /// <summary>
        ///     Gets or sets the ordered list of system message types.
        /// </summary>
        public IList<ISystemMessageTypeInfo> SystemMessageTypes { get; set; }

        /// <summary>
        ///     Gets or sets the build version that the client is running on. It must be up to date in order to log in.
        /// </summary>
        public int BuildVersion { get; set; }

        /// <summary>
        ///     Gets or sets a delegate that is invoked when a component listening to the client's dispatcher throws an exception
        ///     that isn't user-handled.
        /// </summary>
        public Action<Exception> UnhandledExceptionHandler { get; set; }
    }
}