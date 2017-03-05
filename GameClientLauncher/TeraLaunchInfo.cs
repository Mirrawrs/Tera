using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tera.Launcher
{
    /// <summary>
    ///     Represents the information that is sent to the game on startup to notify it about realms offered by the publisher.
    /// </summary>
    public class TeraLaunchInfo
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TeraLaunchInfo" /> class.
        /// </summary>
        public TeraLaunchInfo()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeraLaunchInfo" /> class using the specified account name and auth
        ///     ticket.
        /// </summary>
        /// <param name="accountName">The master account name.</param>
        /// <param name="ticket">A one-time authentication ticket.</param>
        public TeraLaunchInfo(string accountName, string ticket)
        {
            AccountName = accountName;
            Ticket = ticket;
        }

        [JsonProperty("last_connected_server_id")]
        public int LastConnectedServerId { get; set; }

        [JsonProperty("chars_per_server")]
        public IList<ServerCharactersInfo> CharactersPerServer { get; set; } = new List<ServerCharactersInfo>();

        [JsonProperty("account_bits")]
        public string AccountBits { get; set; }

        [JsonProperty("result-message")]
        public string ResultMessage { get; set; }

        [JsonProperty("result-code")]
        public int ResultCode { get; set; } = 200;

        [JsonProperty("access_level")]
        public int AccessLevel { get; set; }

        [JsonProperty("user_permission")]
        public int UserPermission { get; set; }

        [JsonProperty("game_account_name")]
        public string GameAccountName { get; set; } = "TERA";

        [JsonProperty("master_account_name")]
        public string AccountName { get; set; }

        [JsonProperty("ticket")]
        public string Ticket { get; set; }
    }
}