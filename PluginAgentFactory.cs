using System;
using System.Collections.Generic;
using NewRelic.Platform.Sdk;

namespace Org.Healthwise.NewRelic.NTP
{
    class PluginAgentFactory : AgentFactory
    {
        /// <summary>
        /// Creates agents for each item in the plugin.json file
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public override Agent CreateAgentWithConfiguration(IDictionary<string, object> properties)
        {
            string name = (string)properties["name"];
            string ntpsource = (string)properties["ntpSourceServer"];
            var ntpServers = (List<Object>)properties["ntpServers"];

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(ntpsource) || ntpServers.Count <= 0)
            {
                throw new ArgumentNullException("'name', 'ntpSourceServer', and 'ntpServers' cannot be null or empty. Do you have a 'config/plugin.json' file?");
            }

            return new PluginAgent(name, ntpsource, ntpServers);
        }
    }
}
