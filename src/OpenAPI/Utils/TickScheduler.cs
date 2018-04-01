using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MiNET.Utils;

namespace OpenAPI.Utils
{
	public class TickScheduler
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(TickScheduler));
		
		private ConcurrentDictionary<ScheduledTick, ulong> _scheduledTicks = new ConcurrentDictionary<ScheduledTick, ulong>();
		private Timer Hpt { get; }

		public TickScheduler()
		{
			Hpt = new Timer(Action, new object(), 50, 50);
		}

		private object _tickLock = new object();
		private ulong _tick = 0;
		private void Action(object o)
		{
			try
			{
				foreach (var kv in _scheduledTicks.Where(x => x.Value <= _tick).OrderByDescending(x => x.Value))
				{
					ulong scheduledTick;
					if (_scheduledTicks.TryRemove(kv.Key, out scheduledTick))
					{
						if (!kv.Key.CancellationToken.IsCancellationRequested)
						{
							try
							{
								Task.Run(kv.Key.Action);
							}
							catch (Exception ex)
							{
								Log.Error("Error while executing scheduled tick!", ex);
							}

							if (kv.Key.Repeat)
							{
								if (!_scheduledTicks.TryAdd(kv.Key, _tick + kv.Key.TickInFuture))
								{
									Log.Warn("Failed to re-schedule tick!");
								}
							}
						}
					}
				}
			}
			finally
			{
				lock (_tickLock)
				{
					_tick++;
				}
			}
		}

		public void ScheduleTick(ulong ticksFromNow, Action onTick, CancellationToken cancellationToken, bool repeat = false)
		{
		/*	var period = TimeSpan.FromMilliseconds(50 * ticksFromNow);
			if (repeat)
			{
				Task.Run(async () =>
				{
					while (!cancellationToken.IsCancellationRequested)
					{
						await Task.Delay(period, cancellationToken);

						if (!cancellationToken.IsCancellationRequested)
							onTick();
					}
				}, cancellationToken);
			}
			else
			{
				Task.Delay(period, cancellationToken).ContinueWith((t) =>
				{
					onTick();
				}, cancellationToken);
			}*/

			var executionTime = _tick + ticksFromNow;
			ScheduledTick scheduledTick = new ScheduledTick(onTick, cancellationToken, repeat, ticksFromNow);
			if (!_scheduledTicks.TryAdd(scheduledTick, executionTime))
			{
				Log.Warn($"Failed to schedule tick!");
			}
			else
			{
				cancellationToken.Register(() =>
				{
					ulong scheduledTickTime;
					_scheduledTicks.TryRemove(scheduledTick, out scheduledTickTime);
				});
			}
			
			//return executionTime;
		}

		public void Close()
		{
			Hpt.Dispose();
		}

		private struct ScheduledTick
		{
			public ulong TickInFuture { get; }
			public Action Action { get; }
			public CancellationToken CancellationToken { get; }
			public bool Repeat { get; }
			public ScheduledTick(Action action, CancellationToken cancellationToken, bool repeat, ulong ticksInFuture)
			{
				Action = action;
				CancellationToken = cancellationToken;
				Repeat = repeat;
				TickInFuture = ticksInFuture;
			}
		}
	}
}
