using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using log4net;
using MiNET;
using MiNET.Net;
using MiNET.Net.NetherNet;
using MiNET.Utils;
using MiNET.Utils.Diagnostics;
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
		private NetherNetListener Listener { get; }

		/// <summary>
		///     The transport counters, read off the MiNET meter rather than off the connection.
		///     NetherNet publishes its statistics as OpenTelemetry instruments, so the only way to
		///     total them is to listen; each field below accumulates one instrument and is drained
		///     with an Interlocked.Exchange on every report, exactly as the old per-second fields were.
		/// </summary>
		private readonly MeterListener _meterListener;

		private long _messagesIn;
		private long _messagesOut;
		private long _bytesIn;
		private long _bytesOut;
		private long _acksOut;
		private long _retransmits;
		private long _drops;

		public OpenServerInfo(NetherNetListener listener, OpenApi api, LevelManager levelManager)
			: base(() => listener?.Sessions.Count ?? 0)
		{
			Listener = listener;

			Api = api;

			Interval = Config.GetProperty("InfoInterval", 1000);
			MaxNumberOfPlayers = Config.GetProperty("MaxNumberOfPlayers", 1000);
			MaxNumberOfConcurrentConnects =
				Config.GetProperty("MaxNumberOfConcurrentConnects", Config.GetProperty("MaxNumberOfPlayers", 1000));

			_meterListener = new MeterListener
			{
				InstrumentPublished = (instrument, l) =>
				{
					if (instrument.Meter.Name == TransportMetrics.MeterName) l.EnableMeasurementEvents(instrument);
				}
			};

			_meterListener.SetMeasurementEventCallback<long>(OnMeasurement);
			_meterListener.Start();
		}

		private void OnMeasurement(Instrument instrument, long measurement,
			ReadOnlySpan<KeyValuePair<string, object>> tags, object state)
		{
			switch (instrument.Name)
			{
				case "transport.messages.in":
					Interlocked.Add(ref _messagesIn, measurement);
					break;
				case "transport.messages.out":
					Interlocked.Add(ref _messagesOut, measurement);
					break;
				case "transport.bytes.in":
					Interlocked.Add(ref _bytesIn, measurement);
					break;
				case "transport.bytes.out":
					Interlocked.Add(ref _bytesOut, measurement);
					break;
				case "transport.sctp.sacks":
					Interlocked.Add(ref _acksOut, measurement);
					break;
				case "transport.retransmits":
					Interlocked.Add(ref _retransmits, measurement);
					break;
				case "transport.drops":
					Interlocked.Add(ref _drops, measurement);
					break;
			}
		}

		public EventHandler<MetricsEvent> OnMetricsReport;

		private void OnThroughPut(object state)
		{
			NumberOfPlayers = Listener?.Sessions.Count ?? 0;

			int availableWorkerThreads;
			int availablePortThreads;
			ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availablePortThreads);

			int maxWorkerThreads;
			int maxPortThreads;
			ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxPortThreads);

			long packetSizeOut = Interlocked.Exchange(ref _bytesOut, 0);
			long packetSizeIn = Interlocked.Exchange(ref _bytesIn, 0);

			double mbpsPerSecondOut = packetSizeOut * 8 / 1_000_000D;
			double mbpsPerSecondIn = packetSizeIn * 8 / 1_000_000D;

			long numberOfPacketsOutPerSecond = Interlocked.Exchange(ref _messagesOut, 0);
			long numberOfPacketsInPerSecond = Interlocked.Exchange(ref _messagesIn, 0);

			AvgSizePerPacketIn = AvgSizePerPacketIn == 0 ? packetSizeIn * 100 : (long) ((AvgSizePerPacketIn * 99) + (packetSizeIn == 0 ? 0 : numberOfPacketsInPerSecond / ((double) packetSizeIn)));
			AvgSizePerPacketOut = AvgSizePerPacketOut == 0 ? packetSizeOut * 100 : (long) ((AvgSizePerPacketOut * 99) + (packetSizeOut == 0 ? 0 : numberOfPacketsOutPerSecond / ((double) packetSizeOut)));
			AvgSizePerPacketIn /= 100; // running avg of 100 prev values
			AvgSizePerPacketOut /= 100; // running avg of 100 prev values

			// SCTP acknowledges with SACKs, which travel one way only, so there is no inbound-ack or
			// NAK counter to read any more. Both stay zero rather than being invented from the
			// outbound number.
			long numberOfAckIn = 0;
			long numberOfAckOut = Interlocked.Exchange(ref _acksOut, 0);
			long numberOfNakIn = 0;
			long numberOfResend = Interlocked.Exchange(ref _retransmits, 0);
			long numberOfFailed = Interlocked.Exchange(ref _drops, 0);

			var eventsDispatched = Interlocked.Exchange(ref EventsDispatchedPerSecond, 0);
			//var levels = Interlocked.Read(ref Levels);
			var levels = Api.LevelManager.LevelCount;

			var e = _stopwatch.ElapsedMilliseconds;
			if (e >= Interval - ((Interval / 100) * 5))
			{
				var message =
					$"Players {NumberOfPlayers}, " +
					$"Pkt in/out(#/s) {numberOfPacketsInPerSecond}/{numberOfPacketsOutPerSecond}, " +
					$"ACK(out)/RSND/DROP(#/s) {numberOfAckOut}/{numberOfResend}/{numberOfFailed}, " +
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

			// NetherNet refuses a connection inside the listener without counting it, so there is no
			// denied-connection number to report any more.
			long deniedConns = 0;

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
			// The base constructor starts its own once-a-second player-count line. This class reports
			// the same number with everything else attached, so the inherited timer is stopped here
			// and OnEnable installs ours in its place.
			if (ThroughPut != null)
			{
				ThroughPut.Change(Timeout.Infinite, Timeout.Infinite);
				ThroughPut.Dispose();

				ThroughPut = null;
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

			_meterListener?.Dispose();
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
			/// 	The amount of ack's received since last measurement. Always zero on NetherNet:
			/// 	SCTP acknowledges with SACKs, and only the outbound side is counted.
			/// </summary>
			public long AckIn { get; set; }

			/// <summary>
			/// 	The amount of ack's sent since last measurement
			/// </summary>
			public long AckOut { get; set; }

			/// <summary>
			/// 	The amount of nak's received since last measurement. Always zero on NetherNet,
			/// 	which reports loss as gap blocks inside a SACK rather than as a NAK of its own.
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
			/// 	The amount of connections that were denied since last measurement. Always zero on
			/// 	NetherNet, which does not count refusals.
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
