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

			
			Interlocked.Exchange(ref NumberOfDeniedConnectionRequestsPerSecond, 0);

			long packetSizeOut = Interlocked.Exchange(ref TotalPacketSizeOutPerSecond, 0);
			long packetSizeIn = Interlocked.Exchange(ref TotalPacketSizeInPerSecond, 0);

			double mbpsPerSecondOut = packetSizeOut * 8 / 1_000_000D;
			double mbpsPerSecondIn = packetSizeIn * 8 / 1_000_000D;

			long numberOfPacketsOutPerSecond = Interlocked.Exchange(ref NumberOfPacketsOutPerSecond, 0);
			long numberOfPacketsInPerSecond = Interlocked.Exchange(ref NumberOfPacketsInPerSecond, 0);

			AvgSizePerPacketIn = AvgSizePerPacketIn == 0 ? packetSizeIn * 100 : (long) ((AvgSizePerPacketIn * 99) + (packetSizeIn == 0 ? 0 : numberOfPacketsInPerSecond / ((double) packetSizeIn)));
			AvgSizePerPacketOut = AvgSizePerPacketOut == 0 ? packetSizeOut * 100 : (long) ((AvgSizePerPacketOut * 99) + (packetSizeOut == 0 ? 0 : numberOfPacketsOutPerSecond / ((double) packetSizeOut)));
			AvgSizePerPacketIn /= 100; // running avg of 100 prev values
			AvgSizePerPacketOut /= 100; // running avg of 100 prev values
			
			long numberOfAckIn = Interlocked.Exchange(ref NumberOfAckReceive, 0);
			long numberOfAckOut = Interlocked.Exchange(ref NumberOfAckSent, 0);
			long numberOfNakIn = Interlocked.Exchange(ref NumberOfNakReceive, 0);
			long numberOfResend = Interlocked.Exchange(ref NumberOfResends, 0);
			long numberOfFailed = Interlocked.Exchange(ref NumberOfFails, 0);

			var eventsDispatched = Interlocked.Exchange(ref EventsDispatchedPerSecond, 0);
			//var levels = Interlocked.Read(ref Levels);
			var levels = Api.LevelManager.LevelCount;

			var e = _stopwatch.ElapsedMilliseconds;
			if (e >= Interval - ((Interval / 100) * 5))
			{
				var message =
					$"Players {NumberOfPlayers}, " +
					$"Pkt in/out(#/s) {numberOfPacketsInPerSecond}/{numberOfPacketsOutPerSecond}, " +
					$"ACK(in-out)/NAK/RSND/FTO(#/s) ({numberOfAckIn}-{numberOfAckOut})/{numberOfNakIn}/{numberOfResend}/{numberOfFailed}, " +
					$"THR in/out(Mbps) {mbpsPerSecondIn:F}/{mbpsPerSecondOut:F}, " +
					$"PktSz Total in/out(B/s){packetSizeIn}/{packetSizeOut}, " +
					$"PktSz Avg(100s) in/out(B){AvgSizePerPacketIn}/{AvgSizePerPacketOut}";
				
				if (Config.GetProperty("EnableThroughput", true))
				{
					if (Config.GetProperty("ServerInfoInTitle", false))
					{
						Console.Title = message;
					}
					else
					{
						Log.InfoFormat(message);
					}
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

		public long AvgSizePerPacketOut { get; set; }

		public long AvgSizePerPacketIn { get; set; }

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
