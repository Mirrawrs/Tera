using Lotus.Dispatching;
using Lotus.Dispatching.Attributes;
using Tera.Net;

namespace SampleBot
{
    //Base class inherited by all modules in this sample, containing a few properties that are populated
    //at the beginning of TeraClient.Run.
    internal abstract class MyModuleBase
    {
        //Since property getters and setters are methods, they can be decorated with ListenerAttribute.
        protected TeraClient Client { get; [Listener] set; }
        protected Dispatcher Dispatcher { get; [Listener] set; }

        //Note that as long as DispatcherConfiguration.ExactTypeOnlyNotifications is false, base types will
        //also qualify as listeners. So, while an instance of MyTeraClientConfig is passed to the TeraClient,
        //this listener would also be called if it was of type TeraClientConfiguration.
        protected MyTeraClientConfig Configuration { get; [Listener] set; }
    }
}