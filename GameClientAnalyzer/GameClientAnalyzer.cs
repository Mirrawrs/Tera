using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;
using EasyHook;

namespace Tera.Analytics
{
    /// <summary>
    ///     Allows to analyze the client and acts as a remote interface for the IPC client to report results.
    /// </summary>
    public class GameClientAnalyzer : MarshalByRefObject
    {
        private readonly TaskCompletionSource<AnalysisResult> completionSource =
            new TaskCompletionSource<AnalysisResult>();

        /// <summary>
        ///     Starts the game client, injects the current assembly and analyzes it.
        /// </summary>
        /// <param name="gameClientPath">The path for the game executable.</param>
        /// <param name="token">
        ///     The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous analysis operation. The task's <see cref="Task{T}.Result" /> contains the
        ///     results of the analysis.
        /// </returns>
        public Task<AnalysisResult> Analyze(string gameClientPath, CancellationToken token = default(CancellationToken))
        {
            var channelName = default(string);
            RemoteHooking.IpcCreateServer(ref channelName, WellKnownObjectMode.Singleton, this);
            RemoteHooking.CreateAndInject(
                gameClientPath,
                string.Empty,
                0,
                Assembly.GetExecutingAssembly().Location,
                string.Empty,
                out int processId,
                InPassThruArgs: channelName);
            if (token.CanBeCanceled) token.Register(() => completionSource.TrySetCanceled());
            return completionSource.Task;
        }

        public void Complete(AnalysisResult result) => completionSource.TrySetResult(result);

        public void Error(Exception exception) => completionSource.TrySetException(exception);
    }
}