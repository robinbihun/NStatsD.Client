# NStatsD.Client

A .NET 4.0 client for [Etsy](http://etsy.com)'s [StatsD](https://github.com/etsy/statsd) server.

This client will let you fire stats at your StatsD server from a .NET application. Very useful for mixed technology systems that you would like to keep near real-time stats on.

## Requirements
.NET 4.0 (Websocket support)

## Installation

### Nuget

	Nuget Install NStatsD.Client

### Manually

Just include the Client.cs and the StatsDConfigurationSection.cs files in your project. 
Add the following to your config's configSections node.
```xml
<section name="statsD" type="NStatsD.StatsDConfigurationSection, NStatsD.Client" />
```
Then add the following to your app config's configuration node.
```xml
<statsD>
	<server host="localhost" port="8125" />
</statsD>
```
## Usage
```csharp
NStatsD.Client.Current.Increment("testing.increment");
NStatsD.Client.Current.Increment("testing.increment", 0.5); // Optional Sample Rate included on all methods
NStatsD.Client.Current.Decrement("testing.decrement");
NStatsD.Client.Current.Timing("testing.timing", 2345);
NStatsD.Client.Current.Guage("testing.guage", 45);
```
# License

NStatsD.Client is licensed under the MIT license.
