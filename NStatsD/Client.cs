using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;

namespace NStatsD
{
    public sealed class Client
    {
        Client() { }

        public static Client Current
        {
            get { return CurrentClient.Instance; }
        }

        class CurrentClient
        {
            static CurrentClient() { }

            internal static readonly Client Instance = new Client();
        }

        private StatsDConfigurationSection _config;
        public StatsDConfigurationSection Config
        {
            get
            {
                if (_config == null)
                {
                    _config = (StatsDConfigurationSection) ConfigurationManager.GetSection("statsD");
                }

                if (_config == null)
                    throw new ConfigurationErrorsException("statsD Configuration is not present.");

                return _config;
            }
        }

        /// <summary>
        /// Sends timing statistics.
        /// </summary>
        /// <param name="stat">Name of statistic being updated.</param>
        /// <param name="time">The timing it took to complete.</param>
        /// <param name="sampleRate">Tells StatsD how often to sample this value. Defaults to 1 (send all values).</param>
        /// <param name="callback">A callback for when the send is complete. Defaults to null.</param>
        public void Timing(string stat, long time, double sampleRate = 1, AsyncCallback callback = null)
        {
            var data = new Dictionary<string, string> { { stat, string.Format("{0}|ms", time) } };

            Send(data, sampleRate, callback);
        }

        /// <summary>
        /// Increments a counter
        /// </summary>
        /// <param name="stat">Name of statistic being updated.</param>
        /// <param name="sampleRate">Tells StatsD how often to sample this value. Defaults to 1 (send all values).</param>
        /// <param name="callback">A callback for when the send is complete. Defaults to null.</param>
        public void Increment(string stat, double sampleRate = 1, AsyncCallback callback = null)
        {
            UpdateStats(stat, 1, sampleRate, callback);
        }

        /// <summary>
        /// Decrements a counter
        /// </summary>
        /// <param name="stat">Name of statistic being updated.</param>
        /// <param name="sampleRate">Tells StatsD how often to sample this value. Defaults to 1 (send all values).</param>
        /// <param name="callback">A callback for when the send is complete. Defaults to null.</param>
        public void Decrement(string stat, double sampleRate = 1, AsyncCallback callback = null)
        {
            UpdateStats(stat, -1, sampleRate, callback);
        }

        /// <summary>
        /// Updates a counter by an arbitrary amount
        /// </summary>
        /// <param name="stat">Name of statistic being updated.</param>
        /// <param name="value">The value of the metric.</param>
        /// <param name="sampleRate">Tells StatsD how often to sample this value. Defaults to 1 (send all values).</param>
        /// <param name="callback">A callback for when the send is complete. Defaults to null.</param>
        public void Gauge(string stat, int value, double sampleRate = 1, AsyncCallback callback = null)
        {
            var data = new Dictionary<string, string> { { stat, string.Format("{0}|g", value) } };
            Send(data, sampleRate, callback);
        }

        /// <summary>
        /// Updates a counter by an arbitrary amount
        /// </summary>
        /// <param name="stat">Name of statistic(s) being updated.</param>
        /// <param name="delta">The amount to adjust the counter</param>
        /// <param name="sampleRate">Tells StatsD how often to sample this value. Defaults to 1 (send all values).</param>
        /// <param name="callback">A callback for when the send is complete. Defaults to null.</param>
        public void UpdateStats(string stat, int delta = 1, double sampleRate = 1, AsyncCallback callback = null)
        {
            var dictionary = new Dictionary<string, string> { { stat, string.Format("{0}|c", delta) } };
            Send(dictionary, sampleRate, callback);
        }

        private readonly Random _random = new Random();

        private void Send(Dictionary<string, string> data, double sampleRate, AsyncCallback callback)
        {
            if (sampleRate < 1)
            {
                var nextRand = _random.NextDouble();
                if (nextRand <= sampleRate)
                {
                    var sampledData = data.Keys.ToDictionary(stat => stat,
                        stat => string.Format("{0}|@{1}", data[stat], sampleRate));
                    SendToStatsD(sampledData, callback);
                }
            }
            else
            {
                SendToStatsD(data, callback);
            }
        }

        private void SendToStatsD(Dictionary<string, string> sampledData, AsyncCallback callback)
        {
            var host = Config.Server.Host;
            var port = Config.Server.Port;
            using (var client = new UdpClient(host, port))
            {
                foreach (var sendData in from stat in sampledData.Keys
                    let encoding = new System.Text.ASCIIEncoding()
                    let stringToSend = string.Format("{0}:{1}", stat, sampledData[stat])
                    select encoding.GetBytes(stringToSend))
                {
                    client.BeginSend(sendData, sendData.Length, callback, null);
                }
            }
        }
    }
}
