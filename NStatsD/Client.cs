using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;

namespace NStatsD
{
    public class Client
    {
        private static Client _current;
        public static Client Current
        {
            get
            {
                if (_current == null)
                    _current = new Client();
                return _current;
            }
            set { _current = value; }
        }

        private StatsDConfigurationSection _config;
        public StatsDConfigurationSection Config
        {
            get
            {
                if (_config == null)
                    _config = (StatsDConfigurationSection)ConfigurationManager.GetSection("statsD");
                return _config;
            }
        }

        public void Timing(string stat, long time, double sampleRate = 1)
        {
            var data = new Dictionary<string, string> { { stat, string.Format("{0}|ms", time) } };

            Send(data, sampleRate);
        }

        public void Increment(string stat, double sampleRate = 1)
        {
            UpdateStats(stat, 1, sampleRate);
        }

        public void Decrement(string stat, double sampleRate = 1)
        {
            UpdateStats(stat, -1, sampleRate);
        }

        public void Guage(string stat, int value, double sampleRate = 1)
        {
            var data = new Dictionary<string, string> {{stat, string.Format("{0}|g", value)}};
            Send(data, sampleRate);
        }

        public void UpdateStats(string stat, int delta = 1, double sampleRate = 1)
        {
            var dictionary = new Dictionary<string, string> {{stat, string.Format("{0}|c", delta)}};
            Send(dictionary, sampleRate);
        }

        private void Send(Dictionary<string, string> data, double sampleRate = 1)
        {
            var random = new Random();
            var sampledData = new Dictionary<string, string>();
            if (sampleRate < 1 && random.Next(0, 1) <= sampleRate)
            {
                foreach (var stat in data.Keys)
                {
                    sampledData.Add(stat, string.Format("{0}|@{1}", data[stat], sampleRate));
                }
            }
            else
            {
                sampledData = data;
            }
            var host = Config.Server.Host;
            var port = Config.Server.Port;
            using (var client = new UdpClient(host, port))
            {
                foreach (var stat in sampledData.Keys)
                {
                    var encoding = new System.Text.ASCIIEncoding();
                    var stringToSend = string.Format("{0}:{1}", stat, sampledData[stat]);
                    var sendData = encoding.GetBytes(stringToSend);
                    client.BeginSend(sendData, sendData.Length, Callback, null);
                }
            }
        }

        private static void Callback(IAsyncResult result)
        {
            // dont really want to do anything here since, would rather miss metrics than cause a site/app failure
        }
    }
}
