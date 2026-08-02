using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MiNET.Utils;
using OpenAPI.Plugins;

namespace OpenAPI.Utils
{
	public class TickScheduler : IAssemblyPurgeable
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(TickScheduler));
		
		private ConcurrentDictionary<ScheduledTick, ulong> _scheduledTicks = new ConcurrentDictionary<ScheduledTick, ulong>();
		private Timer Hpt { get; }

		public TickScheduler() : this(null)
		{
		}

		/// <param name="openApi">
		///		When supplied, the scheduler registers itself so plugin teardown can reach it.
		///		Levels outlive plugin reloads, so anything scheduled here would otherwise keep
		///		the scheduling plugin's assembly alive.
		/// </param>
		public TickScheduler(OpenApi openApi)
		{
			Hpt = new Timer(Action, new object(), 50, 50);

			openApi?.RegisterPurgeable(this);
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
				// Keep the registration so it can be disposed. The callback closure captures
				// scheduledTick, which holds the caller's delegate, so an undisposed
				// registration leaves the callers's assembly reachable from the token even
				// after the tick has been removed from _scheduledTicks.
				scheduledTick.Registration = cancellationToken.Register(() => Remove(scheduledTick));
			}

			//return executionTime;
		}

		private bool Remove(ScheduledTick scheduledTick)
		{
			if (!_scheduledTicks.TryRemove(scheduledTick, out _))
				return false;

			scheduledTick.Registration.Dispose();
			return true;
		}

		/// <summary>
		///		Removes every scheduled tick whose callback belongs to <paramref name="assembly"/>.
		/// </summary>
		/// <remarks>
		///		A scheduler belongs to a level, and levels outlive plugin reloads, so anything a
		///		plugin scheduled here would otherwise keep its assembly alive. A repeating tick
		///		registered with <see cref="CancellationToken.None"/> has no other removal path
		///		at all.
		/// </remarks>
		public int PurgeAssembly(Assembly assembly)
		{
			int removed = 0;

			foreach (var scheduled in _scheduledTicks.ToArray())
			{
				var action = scheduled.Key.Action;

				bool belongsToAssembly =
					action?.Target?.GetType().Assembly == assembly
					|| action?.Method?.DeclaringType?.Assembly == assembly;

				if (belongsToAssembly && Remove(scheduled.Key))
					removed++;
			}

			return removed;
		}

		public void Close()
		{
			Hpt.Dispose();

			foreach (var scheduled in _scheduledTicks.ToArray())
			{
				Remove(scheduled.Key);
			}
		}

		/// <remarks>
		///		A class rather than a struct so the dictionary keys on identity and the
		///		<see cref="CancellationTokenRegistration"/> can be written back after
		///		registration without copying.
		/// </remarks>
		private sealed class ScheduledTick
		{
			public ulong TickInFuture { get; }
			public Action Action { get; }
			public CancellationToken CancellationToken { get; }
			public bool Repeat { get; }
			public CancellationTokenRegistration Registration { get; set; }

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
