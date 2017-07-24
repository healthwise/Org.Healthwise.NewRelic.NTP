# Org.Healthwise.NewRelic.NTP
New Relic Plugin for Monitoring Time Skew Between NTP Sources.

# Metrics
This plugin will query each NTP end-point and compare its time against the declared NTP source.  
The plugin provides a chart (unit: seconds) of the total time difference between the two NTP source.  Note that the value is ALWAYS positive, indicating the amount of skew, and NOT the direction (more/less) in comparision to the source.

# Requirements
1. .Net 4.5

# Configuration

```
{
  "agents": [
    {
      "name": "NTP Sentry",
      "ntpSourceServer": "time.nist.gov",
	      "ntpServers": [
		        "pool.ntp.org",
		        "plb-sxx-ad02.devsolo-products.local",
		        "time.windows.com"
	     ]
    }
  ]
}
```

### Configuration Elements
```
    name: "{String}"
```
   * Represents the name of the monitor being deployed

```
    "ntpSourceServer": "{String}"
```
* The NTP 'source' server.  Time skews are calculated based on this source.

```
	      "ntpServers": [
		        "{String}",
		        "{String}",
		        [..]
	     ]
```
* The list of NTP sources to query and compare against the declared source.

# Installation
1. Download release and unzip on machine to handle monitoring.
2. Edit Config Files
    rename newrelic.template.json to newrelic.json
    Rename plugin.template.json to plugin.json
    Update settings in both config files for your environment
3. Run plugin.exe from Command line

Use NPI to install the plugin to register as a service

1. Run Command as admin: npi install org.healthwise.newrelic.ntp
2. Follow on screen prompts
