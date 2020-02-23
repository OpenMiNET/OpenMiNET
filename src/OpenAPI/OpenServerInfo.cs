using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Threading;
using log4net;
using MiNET;
using MiNET.Utils;
using OpenAPI.World;
using Timer = System.Threading.Timer;

namespace OpenAPI
{
	public class OpenServerInfo : ServerInfo
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ServerInfo));

		public long EventsDispatchedPerSecond;
		public long Levels;
		private OpenApi Api { get; }
		private int Interval { get; } = 1000;
		private Stopwatch _stopwatch = Stopwatch.StartNew();
		public OpenServerInfo(OpenApi api, ConcurrentDictionary<IPEndPoint, PlayerNetworkSession> playerSessions, LevelManager levelManager) : base(levelManager, playerSessions)
		{
			Api = api;

			Interval = Config.GetProperty("InfoInterval", 1000);
			MaxNumberOfPlayers = Config.GetProperty("MaxNumberOfPlayers", 1000);
		    MaxNumberOfConcurrentConnects =
		        Config.GetProperty("MaxNumberOfConcurrentConnects", Config.GetProperty("MaxNumberOfPlayers", 1000));
		}

		private void OnThroughPut(object state)
		{
			NumberOfPlayers = PlayerSessions.Count;
			
			int availableWorkerThreads;
			int availablePortThreads;
			ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availablePortThreads);

			int maxWorkerThreads;
			int maxPortThreads;
			ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxPortThreads);

			double kbitPerSecondOut = Interlocked.Exchange(ref TotalPacketSizeOut, 0) * 8 / 1000000D;
			double kbitPerSecondIn = Interlocked.Exchange(ref TotalPacketSizeIn, 0) * 8 / 1000000D;

			var numbPacketsOutPerSeconds = Interlocked.Exchange(ref NumberOfPacketsOutPerSecond, 0);
			var acks = Interlocked.Exchange(ref NumberOfAckReceive, 0);
			var packetsIn = Interlocked.Exchange(ref NumberOfPacketsInPerSecond, 0);
			var nak = Interlocked.Exchange(ref NumberOfNakReceive, 0);
			var resends = Interlocked.Exchange(ref NumberOfResends, 0);
			var fails = Interlocked.Exchange(ref NumberOfFails, 0);
			var acksSent = Interlocked.Exchange(ref NumberOfAckSent, 0);

			var eventsDispatched = Interlocked.Exchange(ref EventsDispatchedPerSecond, 0);
			//var levels = Interlocked.Read(ref Levels);
			var levels = Api.LevelManager.LevelCount;

			var e = _stopwatch.ElapsedMilliseconds;
			if (e >= Interval - ((Interval / 100) * 5))
			{
				if (Config.GetProperty("EnableThroughput", true))
				{
					Log.InfoFormat(
						"{5} Pl(s) Pkt(#/s) (Out={0} In={2}) ACK/NAK/RESD/FTO(#/s) ({1}-{14})/{11}/{12}/{13} Tput(Mbit/s) ({3:F} {7:F}) Avail {8}kb Threads {9} Compl.ports {10}",
						numbPacketsOutPerSeconds,
						acks,
						packetsIn,
						kbitPerSecondOut,
						0 /*_level.LastTickProcessingTime*/,
						NumberOfPlayers,
						Latency,
						kbitPerSecondIn,
						AvailableBytes / 1000,
						availableWorkerThreads,
						availablePortThreads,
						nak,
						resends,
						fails,
						acksSent
					);
				}
				else if (Config.GetProperty("EnableOpenServerInfo", false))
				{
					Log.Info(
						$"Players: {NumberOfPlayers} " +
						$"Compl.ports: {maxPortThreads - availablePortThreads}/{maxPortThreads} " +
						$"Threads: {maxWorkerThreads - availableWorkerThreads}/{maxWorkerThreads} " +
						$"Events: {eventsDispatched} " +
						$"Levels: {levels} ");
				}

				_stopwatch.Restart();
			}

			Interlocked.Exchange(ref NumberOfDeniedConnectionRequestsPerSecond, 0);
		}

	    public void Init()
	    {
	        if (ThroughPut != null)
	        {
	            ThroughPut.Change(Timeout.Infinite, Timeout.Infinite);
	            ThroughPut.Dispose();
	        }
        }

		public void OnEnable()
		{
			ThroughPut = new Timer(OnThroughPut, null, 1000, 1000);
		}

		public void OnDisable()
		{
			if (ThroughPut != null)
			{
				ThroughPut.Change(Timeout.Infinite, Timeout.Infinite);
				ThroughPut.Dispose();

				ThroughPut = null;
			}
		}
	}
}
