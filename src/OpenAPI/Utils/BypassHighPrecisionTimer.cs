using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MiNET.Utils;

namespace OpenAPI.Utils
{
    public class BypassHighPrecisionTimer : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(BypassHighPrecisionTimer));

		private CancellationTokenSource _cancelSource;
		private bool _running;
		private Thread _timerThread;
		private Action<object> _action;

		public AutoResetEvent AutoReset = new AutoResetEvent(true);

		public bool ContinueOnError { get; set; } = true;


		public long Spins = 0;
		public long Sleeps = 0;
		public long Misses = 0;
		public long Yields = 0;
		public long Avarage = 0;


		public BypassHighPrecisionTimer(int interval, Action<object> action, bool useSignaling = false, bool skipTicks = true)
		{
			// The following is dangerous code. It will increase windows timer frequence to interrupt
			// every 1ms instead of default 15ms. It is the same tech that games use to increase FPS and
			// media decoders to increase precision of movies (chrome does this).
			// We use this here to increase the precision of our thread scheduling, since this will have a big
			// impact on overall performance of the server. DO note that changing this is a global setting in
			// Windows and will affect every running process. It will automatically disable itself and reset
			// to default after closing the process.
			// It will have a major impact bettery and energy consumption in general. So don't go tell GreenPeace about
			// this, thanks.
			//
			// HERE BE DRAGONS!
#if !LINUX
			if (Environment.OSVersion.Platform != PlatformID.Unix && Environment.OSVersion.Platform != PlatformID.MacOSX)
			{
				WinApi.TimeBeginPeriod(1);
			}
			// END IS HERE. SAFE AGAIN ...
#endif
			Avarage = interval;
			_action = action;

			if (interval < 1)
				throw new ArgumentOutOfRangeException();

			_cancelSource = new CancellationTokenSource();

			var watch = Stopwatch.StartNew();
			long nextStop = interval;

			var task = new Task(() =>
			{
				_timerThread = Thread.CurrentThread;
				Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

				try
				{
					_running = true;

					while (!_cancelSource.IsCancellationRequested)
					{
						long msLeft = nextStop - watch.ElapsedMilliseconds;
						if (msLeft <= 0)
						{
							if (msLeft < -1) Misses++;
							//if (!skipTicks && msLeft < -4) Log.Warn($"We are {msLeft}ms late for action execution");

							long execTime = watch.ElapsedMilliseconds;
							try
							{
								_action?.Invoke(this);
							}
							catch (Exception)
							{
								if (!ContinueOnError) throw;
							}
							AutoReset.Reset();
							execTime = watch.ElapsedMilliseconds - execTime;
							Avarage = (Avarage * 9 + execTime) / 10L;

							if (skipTicks)
							{
								// Calculate when the next stop is. If we're too slow on the trigger then we'll skip ticks
								nextStop = (long) (interval * (Math.Floor(watch.ElapsedMilliseconds / (float) interval /*+ 1f*/) + 1));
							}
							else
							{
								long calculatedNextStop = (long) (interval * (Math.Floor(watch.ElapsedMilliseconds / (float) interval /*+ 1f*/) + 1));
								nextStop += interval;

								// Calculate when the next stop is. If we're very behind on ticks then we'll skip ticks
								if (calculatedNextStop - nextStop > 2 * interval)
								{
									//Log.Warn($"Skipping ticks because behind {calculatedNextStop - nextStop}ms. Too much");
									nextStop = calculatedNextStop;
								}
							}

							// If we can't keep up on execution time, we start skipping ticks until we catch up again.
							if (Avarage > interval) nextStop += interval;

							continue;
						}
						if (msLeft < 5)
						{
							Spins++;

							if (useSignaling)
							{
								AutoReset.WaitOne(50);
							}

							var stop = nextStop;
							if (watch.ElapsedMilliseconds < stop)
							{
								SpinWait.SpinUntil(() => watch.ElapsedMilliseconds >= stop);
							}

							continue;
						}

						if (msLeft < 16)
						{
							if (Thread.Yield())
							{
								Yields++;
								continue;
							}

							Sleeps++;
							Thread.Sleep(1);
							if (!skipTicks)
							{
								long t = nextStop - watch.ElapsedMilliseconds;
								if (t < -5) Log.Warn($"We overslept {t}ms in thread yield/sleep");
							}
							continue;
						}

						Sleeps++;
						Thread.Sleep(Math.Max(1, (int) (msLeft - 16)));
						if (!skipTicks)
						{
							long t = nextStop - watch.ElapsedMilliseconds;
							if (t < -5) Log.Warn($"We overslept {t}ms in thread sleep");
						}
					}
				}
				catch (Exception e)
				{
					Log.Error("Timer crashed out with uncaught exception leaving it in an undisposed state.", e);
					throw;
				}
				finally
				{
					_running = false;
					Dispose();
				}
			}, _cancelSource.Token, TaskCreationOptions.LongRunning);

			task.Start();
		}

		public void Dispose()
		{
			//Console.WriteLine("Called Disposed");

			_action = null; // Make sure this will not fire again

			if (_cancelSource == null) return;
			if (AutoReset == null) return;

			if (_timerThread != Thread.CurrentThread)
			{
				if (_running)
				{
					_cancelSource.Cancel();
					AutoReset?.Set();
				}

				while (_running) Thread.Yield();

				//Console.WriteLine("Disposed from other thread");
				return;
			}

			if (_running)
			{
				_cancelSource.Cancel();
				AutoReset?.Set();
			}
			else
			{
				_cancelSource?.Dispose();
				_cancelSource = null;

				AutoReset?.Dispose();
				AutoReset = null;
				//Console.WriteLine("Disposed from same thread");
			}
		}
	}
}