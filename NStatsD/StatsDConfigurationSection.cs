using System.Configuration;

namespace NStatsD
{
    public class StatsDConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("enabled", DefaultValue = "true", IsRequired = false)]
        public bool Enabled
        {
            get { return (bool) this["enabled"]; }
            set { this["enabled"] = value; }
        }
        
        [ConfigurationProperty("server")]
        public ServerElement Server
        {
            get { return (ServerElement) this["server"]; }
            set { this["server"] = value; }
        }
    }

    
    public class ServerElement : ConfigurationElement
    {
        [ConfigurationProperty("host", DefaultValue = "localhost", IsRequired = true)]
        public string Host
        {
            get { return (string) this["host"]; }
            set { this["host"] = value; }
        }

        [ConfigurationProperty("port", DefaultValue = "8125", IsRequired = false)]
        public int Port
        {
            get { return (int) this["port"]; }
            set { this["port"] = value; }
        }
    }
}
