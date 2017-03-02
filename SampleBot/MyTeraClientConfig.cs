using Tera.Net;

namespace SampleBot
{
    //Since the configuration is dispatched to all the modules, it's a convenient place to put application-scope variables
    //without using a static class.
    internal class MyTeraClientConfig : TeraClientConfiguration
    {
        //Used by RealmEnterModule to automatically choose a character to enter the world with.
        public string CharacterName { get; set; }
    }
}