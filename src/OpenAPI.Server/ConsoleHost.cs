using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAPI.Server
{
	/// <summary>
	///		Taken from https://stackoverflow.com/questions/41454563/how-to-write-a-linux-daemon-with-net-core
	/// </summary>
	public static class ConsoleHost
	{
		public static void WaitForShutdown()
		{
			WaitForShutdownAsync().GetAwaiter().GetResult();
		}

		public static void Wait()
		{
			WaitAsync().GetAwaiter().GetResult();
		}

		public static async Task WaitAsync(CancellationToken token = default(CancellationToken))
		{
			if (token.CanBeCanceled)
			{
				await WaitAsync(token, shutdownMessage: null);
				return;
			}

			var done = new ManualResetEventSlim(false);
			using (var cts = new CancellationTokenSource())
			{
				AttachCtrlcSigtermShutdown(cts, done, shutdownMessage: "Application is shutting down...");
				await WaitAsync(cts.Token, "Application running. Press Ctrl+C to shut down.");
				done.Set();
			}
		}

		public static async Task WaitForShutdownAsync(CancellationToken token = default(CancellationToken))
		{
			var done = new ManualResetEventSlim(false);
			using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
			{
				AttachCtrlcSigtermShutdown(cts, done, shutdownMessage: string.Empty);
				await WaitForTokenShutdownAsync(cts.Token);
				done.Set();
			}
		}

		private static async Task WaitAsync(CancellationToken token, string shutdownMessage)
		{
			if (!string.IsNullOrEmpty(shutdownMessage))
			{
				Console.WriteLine(shutdownMessage);
			}
			await WaitForTokenShutdownAsync(token);
		}


		private static void AttachCtrlcSigtermShutdown(CancellationTokenSource cts, ManualResetEventSlim resetEvent, string shutdownMessage)
		{
			Action ShutDown = () => {
				if (!cts.IsCancellationRequested)
				{
					if (!string.IsNullOrWhiteSpace(shutdownMessage))
					{
						Console.WriteLine(shutdownMessage);
					}
					try
					{
						cts.Cancel();
					}
					catch (ObjectDisposedException) { }
				}

				resetEvent.Wait();
			};

			AppDomain.CurrentDomain.ProcessExit += delegate { ShutDown(); };
			Console.CancelKeyPress += (sender, eventArgs) => {
				ShutDown();

				eventArgs.Cancel = true;
			};
		}

		private static async Task WaitForTokenShutdownAsync(CancellationToken token)
		{
			var waitForStop = new TaskCompletionSource<object>();
			token.Register(obj => {
				var tcs = (TaskCompletionSource<object>)obj;
				tcs.TrySetResult(null);
			}, waitForStop);
			await waitForStop.Task;
		}
	}
}
