using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Threading;
using log4net;
using MiNET;
using MiNET.Net.RakNet;
using MiNET.Utils;
using OpenAPI.World;
using Timer = System.Threading.Timer;

namespace OpenAPI
{
	public class OpenServerInfo : ConnectionInfo
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(OpenServerInfo));

		public long EventsDispatchedPerSecond;
		public long Levels;
		private OpenApi Api { get; }
		private int Interval { get; } = 1000;
		private Stopwatch _stopwatch = Stopwatch.StartNew();
		private RakConnection Connection { get; }
		public OpenServerInfo(RakConnection connection, OpenApi api, ConcurrentDictionary<IPEndPoint, RakSession> playerSessions, LevelManager levelManager) : base(playerSessions)
		{
			Connection = connection;
			connection?.ConnectionInfo?.ThroughPut?.Change(Timeout.Infinite, Timeout.Infinite);
			
			Api = api;

			Interval = Config.GetProperty("InfoInterval", 1000);
			MaxNumberOfPlayers = Config.GetProperty("MaxNumberOfPlayers", 1000);
		    MaxNumberOfConcurrentConnects =
		        Config.GetProperty("MaxNumberOfConcurrentConnects", Config.GetProperty("MaxNumberOfPlayers", 1000));
		}

		public EventHandler<MetricsEvent> OnMetricsReport;

		private void OnThroughPut(object state)
		{
			var connectionInfo = Connection.ConnectionInfo;
			NumberOfPlayers = RakSessions.Count;
			
			int availableWorkerThreads;
			int availablePortThreads;
			ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availablePortThreads);

			int maxWorkerThreads;
			int maxPortThreads;
			ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxPortThreads);

			
			//long numberOfDeniedConnectionsPerSecond = Interlocked.Exchange(ref ConnectionInfo.NumberOfDeniedConnectionRequestsPerSecond, 0);

			
			long packetSizeOut = Interlocked.Exchange(ref connectionInfo.TotalPacketSizeOutPerSecond, 0);
			long packetSizeIn = Interlocked.Exchange(ref connectionInfo.TotalPacketSizeInPerSecond, 0);

			double mbpsPerSecondOut = packetSizeOut * 8 / 1_000_000D;
			double mbpsPerSecondIn = packetSizeIn * 8 / 1_000_000D;

			long numberOfPacketsOutPerSecond = Interlocked.Exchange(ref connectionInfo.NumberOfPacketsOutPerSecond, 0);
			long numberOfPacketsInPerSecond = Interlocked.Exchange(ref connectionInfo.NumberOfPacketsInPerSecond, 0);

			AvgSizePerPacketIn = AvgSizePerPacketIn == 0 ? packetSizeIn * 100 : (long) ((AvgSizePerPacketIn * 99) + (packetSizeIn == 0 ? 0 : numberOfPacketsInPerSecond / ((double) packetSizeIn)));
			AvgSizePerPacketOut = AvgSizePerPacketOut == 0 ? packetSizeOut * 100 : (long) ((AvgSizePerPacketOut * 99) + (packetSizeOut == 0 ? 0 : numberOfPacketsOutPerSecond / ((double) packetSizeOut)));
			AvgSizePerPacketIn /= 100; // running avg of 100 prev values
			AvgSizePerPacketOut /= 100; // running avg of 100 prev values
			
			long numberOfAckIn = Interlocked.Exchange(ref connectionInfo.NumberOfAckReceive, 0);
			long numberOfAckOut = Interlocked.Exchange(ref connectionInfo.NumberOfAckSent, 0);
			long numberOfNakIn = Interlocked.Exchange(ref connectionInfo.NumberOfNakReceive, 0);
			long numberOfResend = Interlocked.Exchange(ref connectionInfo.NumberOfResends, 0);
			long numberOfFailed = Interlocked.Exchange(ref connectionInfo.NumberOfFails, 0);

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
				
				if (Config.GetProperty("EnableOpenServerInfo", false))
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

			var deniedConns = Interlocked.Exchange(ref connectionInfo.NumberOfDeniedConnectionRequestsPerSecond, 0);
			
			OnMetricsReport?.Invoke(this, new MetricsEvent()
			{
				Players = NumberOfPlayers,
				PacketsIn = numberOfPacketsInPerSecond,
				PacketsOut = numberOfPacketsOutPerSecond,
				NetworkDownloadBps = packetSizeIn,
				NetworkUploadBps = packetSizeOut,
				Failures = numberOfFailed,
				Resends = numberOfResend,
				AckIn = numberOfAckIn,
				AckOut = numberOfAckOut,
				DeniedConnections = deniedConns,
				EventsDispatched = eventsDispatched,
				NakIn = numberOfNakIn,
				PortThreads = maxPortThreads - availablePortThreads,
				WorkerThreads = maxWorkerThreads - availableWorkerThreads,
				AvgSizePerPacketIn = AvgSizePerPacketIn,
				AvgSizePerPacketOut = AvgSizePerPacketOut
			});
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

		public class MetricsEvent
		{
			/// <summary>
			/// 	The amount of packets that have come in since last measurement
			/// </summary>
			public long PacketsIn { get; set; }
			
			/// <summary>
			/// 	The amount of packets that have gone out since last measurement
			/// </summary>
			public long PacketsOut { get; set; }
			
			public long AvgSizePerPacketIn { get; set; }
			public long AvgSizePerPacketOut { get; set; }
			
			/// <summary>
			/// 	The amount of bytes received since last measurement
			/// </summary>
			public long NetworkDownloadBps { get; set; }
			
			/// <summary>
			/// 	The amount of bytes uploaded since last measurement
			/// </summary>
			public long NetworkUploadBps { get; set; }
			
			/// <summary>
			/// 	The amount of ack's received since last measurement
			/// </summary>
			public long AckIn { get; set; }
			
			/// <summary>
			/// 	The amount of ack's sent since last measurement
			/// </summary>
			public long AckOut { get; set; }
			
			/// <summary>
			/// 	The amount of nak's received since last measurement
			/// </summary>
			public long NakIn { get; set; }
			
			/// <summary>
			/// 	The amount of packet re-sends  since last measurement
			/// </summary>
			public long Resends { get; set; }
			
			/// <summary>
			/// 	The amount of packet failures since last measurement
			/// </summary>
			public long Failures { get; set; }
			
			/// <summary>
			/// 	The amount of events dispatched since last measurement
			/// </summary>
			public long EventsDispatched { get; set; }
			
			/// <summary>
			/// 	The amount of connections that were denied since last measurement
			/// </summary>
			public long DeniedConnections { get; set; }
			
			/// <summary>
			/// 	The amount of completion ports used at time of measurement
			/// </summary>
			public long PortThreads { get; set; }
			
			/// <summary>
			/// 	The amount of worker threads used at time of measurement
			/// </summary>
			public long WorkerThreads { get; set; }
			
			/// <summary>
			/// 	The amount of players connected to the server at time of measurement
			/// </summary>
			public long Players { get; set; }
		}
	}
}
