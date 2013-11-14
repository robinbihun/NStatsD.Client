using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;

namespace NStatsD
{
    public sealed class Client
    {
        static Client() { }

        public static Client Current
        {
            get { return CurrentClient.Instance; }
        }

        class CurrentClient
        {
            static CurrentClient() {  }

            internal static readonly Client Instance = new Client();
        }
        
        private StatsDConfigurationSection _config;
        public StatsDConfigurationSection Config
        {
            get {
                return _config ?? (_config = (StatsDConfigurationSection) ConfigurationManager.GetSection("statsD"));
            }
        }

        public void Timing(string stat, long time, double sampleRate = 1, AsyncCallback callback = null)
        {
            var data = new Dictionary<string, string> { { stat, string.Format("{0}|ms", time) } };

            Send(data, sampleRate, callback);
        }

        public void Increment(string stat, double sampleRate = 1, AsyncCallback callback = null)
        {
            UpdateStats(stat, 1, sampleRate, callback);
        }

        public void Decrement(string stat, double sampleRate = 1, AsyncCallback callback = null)
        {
            UpdateStats(stat, -1, sampleRate, callback);
        }

        public void Gauge(string stat, int value, double sampleRate = 1, AsyncCallback callback = null)
        {
            var data = new Dictionary<string, string> {{stat, string.Format("{0}|g", value)}};
            Send(data, sampleRate, callback);
        }

        public void UpdateStats(string stat, int delta = 1, double sampleRate = 1, AsyncCallback callback = null)
        {
            var dictionary = new Dictionary<string, string> {{stat, string.Format("{0}|c", delta)}};
            Send(dictionary, sampleRate, callback);
        }

        private readonly Random _random = new Random();

        private void Send(Dictionary<string, string> data, double sampleRate, AsyncCallback callback)
        {
            if (Config == null)
            {
              return;
            }

            Dictionary<string, string> sampledData;
            var nextRand = _random.NextDouble();
            if (sampleRate < 1 && nextRand <= sampleRate)
            {
                sampledData = data.Keys.ToDictionary(stat => stat, stat => string.Format("{0}|@{1}", data[stat], sampleRate));
            }
            else
            {
                sampledData = data;
            }

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
