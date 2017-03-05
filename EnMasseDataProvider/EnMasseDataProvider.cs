using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tera.Launcher;

namespace Tera.EnMasse
{
    /// <summary>
    ///     Exposes methods to obtain EnMasse's TERA realm list and authentication ticket.
    /// </summary>
    public class EnMasseDataProvider : ILaunchInfoProvider
    {
        private static readonly HttpClient Client = new HttpClient();

        /// <summary>
        ///     Gets a list of realms.
        /// </summary>
        /// <returns>The list of realms.</returns>
        public async Task<IReadOnlyList<IRealmInfo>> GetRealms()
        {
            var html = await Client.GetStringAsync(ServerListUri);
            var serializer = new XmlSerializer(typeof(RealmList));
            using (var reader = new StringReader(html))
            {
                var realmList = (RealmList) serializer.Deserialize(reader);
                return realmList.Items.Cast<IRealmInfo>().ToList();
            }
        }

        /// <summary>
        ///     Authenticates on the EnMasse server and gets a one-time ticket used to log on a realm.
        /// </summary>
        /// <param name="username">The user's name.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The authentication ticket.</returns>
        public async Task<string> Authenticate(string username, string password)
        {
            var csrfToken = await GetCsrfToken();
            if (!await TryAuthenticate(csrfToken, username, password)) throw new Exception("Authentication error.");
            var ticketJson = await Client.GetStringAsync("https://account.enmasse.com/launcher/1/auth_ticket");
            return (string) JObject.Parse(ticketJson)["ticket"];
        }

        public string ServerListUri => "http://sls.service.enmasse.com:8080/servers/list.en";

        public async Task<TeraLaunchInfo> GetTeraLaunchInfo(string username, string password)
        {
            var ticket = await Authenticate(username, password);
            var infoJson = await Client.GetStringAsync("https://account.enmasse.com/launcher/1/account_server_info");
            var accountServerInfo = JsonConvert.DeserializeObject<TeraLaunchInfo>(infoJson);
            accountServerInfo.Ticket = ticket;
            return accountServerInfo;
        }

        private async Task<string> GetCsrfToken()
        {
            var signInPage = await Client.GetStringAsync("https://account.enmasse.com/launcher/1/signin");
            var csrfTokenMatch = Regex.Match(signInPage, @"<meta content=""(.+?)"" name=""csrf-token""");
            return csrfTokenMatch.Groups[1].Value;
        }

        private async Task<bool> TryAuthenticate(string csrfToken, string username, string password)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"authenticity_token", csrfToken},
                {"user[io_black_box]", "."},
                {"user[email]", username},
                {"user[password]", password}
            });
            var response = await Client.PostAsync("https://account.enmasse.com/launcher/1/authenticate", content);
            var authenticationResponse = await response.Content.ReadAsStringAsync();
            var match = Regex.Match(authenticationResponse, @"ACCOUNT_NAME = ""(.+?)"";");
            return match.Success;
        }
    }
}