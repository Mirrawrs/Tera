using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tera.Launcher
{
    /// <summary>
    ///     Allows to launch the TERA game client.
    /// </summary>
    /// <remarks>
    ///     If a folder named ".svn" is located in the same directory as TL.exe, a debug window will be displayed.
    /// </remarks>
    public class Launcher
    {
        private readonly LauncherConfiguration configuration;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Launcher" /> class using the specified configuration.
        /// </summary>
        /// <param name="configuration">The launcher's configuration.</param>
        public Launcher(LauncherConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        ///     Starts the game client.
        /// </summary>
        /// <param name="cancellationToken">
        ///     The token to monitor for cancellation requests. The default value is
        ///     <see cref="CancellationToken.None" />.
        /// </param>
        /// <returns>An asynchronous task that represents the game startup process.</returns>
        /// <remarks>
        ///     Local functions are used intensively to guarantee that every game start request will use separate unmanaged
        ///     resources without creating a new class for the closure on some local variables in <see cref="Start" />.
        /// </remarks>
        public async Task Start(CancellationToken cancellationToken = default(CancellationToken))
        {
            //Initialize variables used in the startup process.
            var accountInfo = await configuration.LaunchInfoProvider.GetTeraLaunchInfo(
                configuration.Username,
                configuration.Password);
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            //TL.exe communicates with its parent process through Windows messages, specifically WM_COPYDATA. In order to guarantee
            //that these messages are dispatched to the launcher without depending on Winforms, use the Single Thread Apartment model.
            var thread = new Thread(StartGameOnSTAThread);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            await tcs.Task;

            void StartGameOnSTAThread()
            {
                //TL.exe performs a set of checks, including ensuring that the process that created it contains a window with 
                //class equals to the one specified below. Since class name cannot be programmatically set on Forms, use PInvoke
                //to register the class and use it to create an unmanaged window.
                const string launcherClassName = "EME.LauncherWnd";
                var wndClass = new WNDCLASSEX
                {
                    cbSize = (uint) Marshal.SizeOf<WNDCLASSEX>(),
                    lpszClassName = launcherClassName,
                    lpfnWndProc = WndProc
                };
                RegisterClassEx(ref wndClass);
                var unmanagedWindowHandle = CreateWindowEx(0, launcherClassName, null, 0, 0, 0, 0, 0, IntPtr.Zero,
                    IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                //Now that the message pump is running, start the TL.exe process.
                Process.Start(configuration.TeraLauncherPath);
                //WaitAny instead of Task.Wait to avoid throwing an exception if the task is in Faulted state.
                Task.WaitAny(tcs.Task);
                //Release unmanaged resources.
                const uint wmClose = 0x0010;
                DefWindowProc(unmanagedWindowHandle, wmClose, IntPtr.Zero, IntPtr.Zero);
                UnregisterClass(launcherClassName, IntPtr.Zero);
            }

            IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
            {
                //If it's not a WM_COPYDATA message, use the default window procedure.
                const uint wmCopyData = 0x004A;
                if (msg != wmCopyData) return DefWindowProc(hWnd, msg, wParam, lParam);

                /*  Request information:
                 *  wParam: handle for the requesting thread in TL.exe.
                 *  lParam: structure containing the following fields:
                 *      - dwData: represents the requested job.
                 *      - cbData: length in bytes of lpData, including the null-string terminator.
                 *      - lpData: an ANSI, null-terminated string containing the requested job's name.
                 *      
                 *  Response information:
                 *  wParam: handle for the current window.
                 *  lParam: structure containing the following fields:
                 *      - dwData: represents the completed job, stays the same as the request.
                 *      - cbData: length in bytes of lpData, including the null-string terminator.
                 *      - lpData: an ANSI, null-terminated string containing the job's result.
                 */

                var copyData = Marshal.PtrToStructure<COPYDATASTRUCT>(lParam);
                var jobId = (int) copyData.dwData;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    const int handshakeJobId = 0x0DBADB0A;
                    const int slsurlJobId = 2;
                    const int gamestrJobId = 3;
                    string response;
                    switch (jobId)
                    {
                        case handshakeJobId:
                            response = "Hello!!";
                            break;
                        case slsurlJobId:
                            response = configuration.LaunchInfoProvider.ServerListUri;
                            break;
                        case gamestrJobId:
                            response = JsonConvert.SerializeObject(accountInfo);
                            tcs.TrySetResult(null);
                            break;
                        default:
                            throw new Exception("Failed to launch the game.");
                    }
                    copyData.lpData = response;
                    copyData.cbData = response.Length + 1; //Account for null-terminator in length.
                    var outgoingDataPointer = Marshal.AllocHGlobal(Marshal.SizeOf<COPYDATASTRUCT>());
                    //SendMessage needs to run asynchronously because TL.exe doesn't expect a response until the SendMessage call returns.
                    Task.Run(() =>
                    {
                        Marshal.StructureToPtr(copyData, outgoingDataPointer, false);
                        SendMessage(wParam, msg, hWnd, outgoingDataPointer);
                        Marshal.FreeHGlobal(outgoingDataPointer);
                    }, cancellationToken);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
                return new IntPtr(1);
            }
        }

        #region PInvoke

        [DllImport("user32.dll")]
        private static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern ushort RegisterClassEx([In] ref WNDCLASSEX lpWndClass);

        [DllImport("user32.dll")]
        private static extern IntPtr CreateWindowEx(
            uint dwExStyle,
            string lpClassName,
            string lpWindowName,
            uint dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam
        );

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct WNDCLASSEX
        {
            public uint cbSize;
            public readonly uint style;
            [MarshalAs(UnmanagedType.FunctionPtr)] public WndProcDelegate lpfnWndProc;
            public readonly int cbClsExtra;
            public readonly int cbWndExtra;
            public readonly IntPtr hInstance;
            public readonly IntPtr hIcon;
            public readonly IntPtr hCursor;
            public readonly IntPtr hbrBackground;
            public readonly string lpszMenuName;
            public string lpszClassName;
            public readonly IntPtr hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public readonly IntPtr dwData;
            public int cbData;
            public string lpData;
        }

        #endregion
    }
}