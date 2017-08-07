using System;
using System.Collections.Generic;
using NewRelic.Platform.Sdk;
using NewRelic.Platform.Sdk.Utils;
using System.Globalization;

namespace org.healthwise.newrelic.ntp
{
    class PluginAgent : Agent
    {
        // Name of Agent
        private string name;
        // Provides logging for Plugin
        private Logger log = Logger.GetLogger(typeof(PluginAgent).Name);

        private NTP ntpServer;
        private String ntpSourceServer;
        private List<Object> ntpServers;
        TimeSpan dangerDrift;  // A maximum of 5 minutes of drift can occur before authenication mechanisms break down.

        /// <summary>
        /// Constructor for Agent Class
        /// Accepts name and other parameters from plugin.json file
        /// </summary>
        /// <param name="name"></param>
        public PluginAgent(string name, string ntpsource, List<Object>ntpServers)
        {
            this.name = name;
            this.ntpSourceServer = ntpsource;
            this.ntpServers = ntpServers;
            ntpServer = new NTP();
            dangerDrift = TimeSpan.Zero;
        }

        #region "NewRelic Methods"
        /// <summary>
        /// Provides the GUID which New Relic uses to distiguish plugins from one another
        /// Must be unique per plugin
        /// </summary>
        public override string Guid
        {
            get
            {
                return "Org.Healthwise.NewRelic.NTP";
            }
        }

        /// <summary>
        /// Provides the version information to New Relic.
        /// Uses the 
        /// </summary>
        public override string Version
        {
            get
            {
                return typeof(PluginAgent).Assembly.GetName().Version.ToString();
            }
        }

        /// <summary>
        /// Returns a human-readable string to differentiate different hosts/entities in the New Relic UI
        /// </summary>
        public override string GetAgentName()
        {
            return this.name;
        }

        /// <summary>
        /// This is where logic for fetching and reporting metrics should exist.  
        /// Call off to a REST head, SQL DB, virtually anything you can programmatically 
        /// get metrics from and then call ReportMetric.
        /// </summary>
        public override void PollCycle()
        {
            DateTime timeSource = new DateTime();
            Boolean ntpSourceAvailable = true;

            // Query the trusted NTP source
            try
            {
                timeSource = this.ntpServer.GetNetworkTime(this.ntpSourceServer);
                ntpSourceAvailable = true;
            }
            catch
            {
                log.Error("Unable to contact source NTP server: ({0}).", this.ntpSourceServer);
                ntpSourceAvailable = false;
            }

            // Report on failed attempts to get NTP data from NTP source.
            if (ntpSourceAvailable)
            {
                log.Info("plugin/ntp/source: (0)");
                ReportMetric("plugin/ntp/source", "count", 0);
            }
            else
            {
                log.Info("plugin/ntp/source: (1)");
                ReportMetric("plugin/ntp/source", "count", 1);
            }

            // Query the other NTP end-points, if the source is available.
            int memberNTPFailures = 0;
            dangerDrift = TimeSpan.Zero;
            for (var i = 0; i < ntpServers.Count; i++)
            {
                try
                {
                    DateTime ntpTime = this.ntpServer.GetNetworkTime((String)this.ntpServers[i]);

                    if (ntpSourceAvailable)  // Only compute the delta if the source is also available.
                    {
                        TimeSpan ntpDelta = (timeSource - ntpTime).Duration();

                        // determine if we need to update the dangerDrift value
                        if (ntpDelta > dangerDrift)
                        {
                            dangerDrift = ntpDelta;
                        }

                        log.Info("plugin/ntp/" + (String)this.ntpServers[i] + "/skew: ({0})", (float)ntpDelta.TotalSeconds);
                        ReportMetric("plugin/ntp/" + (String)this.ntpServers[i] + "/skew", "seconds", (float)ntpDelta.TotalSeconds);
                    }
                }
                catch
                {
                    log.Error("Unable to contact NTP server: ({0}).", (String)this.ntpServers[i]);
                    memberNTPFailures++;
                }
            }

            // Report on the of NTP member servers we did NOT contact.
            log.Info("plugin/ntp/member: ({0})", memberNTPFailures);
            ReportMetric("plugin/ntp/member", "count", memberNTPFailures);

            // Report the updated dangerDrift Value
            log.Info("plugin/ntp/dangerdrift: ({0})", (float)dangerDrift.TotalSeconds);
            ReportMetric("plugin/ntp/dangerdrift", "seconds", (float)dangerDrift.TotalSeconds);
        }
        #endregion
    }
}

