using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EasyHook;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Tera.Analytics
{
    /// <summary>
    ///     Injected through EasyHook; scans the memory of the process, sends it to an IPC server and terminates it.
    /// </summary>
    public class InjectionEntryPoint : IEntryPoint
    {
        //Needs to be static or it seemingly gets disposed.
        private static LocalHook themidaUnpackedHook;

        private readonly GameClientAnalyzer gameClientAnalyzer;

        public InjectionEntryPoint(RemoteHooking.IContext context, string ipcChannelName)
        {
            gameClientAnalyzer = RemoteHooking.IpcConnectClient<GameClientAnalyzer>(ipcChannelName);
        }

        public async void Run(RemoteHooking.IContext context, string ipcChannelName)
        {
            await UnpackThemida();
            var analyzer = new MemoryScanner();
            try
            {
                var result = analyzer.Analyze();
                gameClientAnalyzer.Complete(result);
            }
            catch (Exception e)
            {
                gameClientAnalyzer.Error(e);
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        /// <summary>
        ///     Creates a task that will complete when Themida finishes unpacking.
        /// </summary>
        private Task UnpackThemida()
        {
            var completionSource = new TaskCompletionSource<object>();

            void OnThemidaUnpacked(out FILETIME lpSystemTimeAsFileTime)
            {
                themidaUnpackedHook.Dispose();
                GetSystemTimeAsFileTime(out lpSystemTimeAsFileTime);
                completionSource.TrySetResult(null);
            }

            themidaUnpackedHook = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", nameof(GetSystemTimeAsFileTime)),
                new GetSystemTimeAsFileTimeDelegate(OnThemidaUnpacked),
                this);
            themidaUnpackedHook.ThreadACL.SetExclusiveACL(new int[0]); //Include all the threads.
            RemoteHooking.WakeUpProcess();
            return completionSource.Task;
        }

        /// <summary>
        ///     One of the first routines called by the client after being unpacked.
        /// </summary>
        [DllImport("kernel32.dll")]
        private static extern void GetSystemTimeAsFileTime(out FILETIME lpSystemTimeAsFileTime);

        private delegate void GetSystemTimeAsFileTimeDelegate(out FILETIME lpSystemTimeAsFileTime);
    }
}