using System;
using System.Linq;
using System.Threading.Tasks;
using Lotus.Dispatching.Attributes;
using Tera.Packets;

namespace SampleBot
{
    //Module responsible of entering the world from the character selection state.
    internal class RealmEnterModule : MyModuleBase
    {
        [Listener]
        public async Task ChooseCharacterOnLogon(S_LOGIN_ACCOUNT_INFO packet)
        {
            await Client.Send(new C_GET_USER_LIST());
            var userListPacket = await Dispatcher.Next<S_GET_USER_LIST>();
            var charName = Configuration.CharacterName;
            var chosenUser = userListPacket.Users.SingleOrDefault(user => user.Name == charName);
            if (chosenUser == null) throw new MyTeraException($"There is no character named {charName}.");
            await Client.Send(new C_SELECT_USER {UserId = chosenUser.Id});
        }

        //In order to access certain features (and, of course, actually spawn in-game), the client must
        //inform the realm when it finishes loading the map.
        [Listener]
        public Task FinishLoadingWorld(S_LOAD_TOPO packet)
        {
            Console.WriteLine("Entering world.");
            return Client.Send(new C_LOAD_TOPO_FIN());
        }
    }
}