using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;

class Weather
{
    private static string ZIP = "74037";
    private static string _Temperature;
    public string Temperature { get { return _Temperature; } }
    private static string _Condition;
    public string Condition { get { return _Condition; } }
    private static string _Forecast;
    public string Forecast { get { return _Forecast; } }
    private static string _Hazards;
    public string Hazards { get { return _Hazards; } }

    static Weather()
	{
		string SavedLocation = "http://weather.yahooapis.com/forecastrss?z=" + ZIP;
		// Create a new XmlDocument
		XmlDocument Weather = new XmlDocument();
		string rss = "http://xml.weather.yahoo.com/ns/rss/1.0";
		Weather.Load(SavedLocation);

		// Set up namespace manager for XPath
		XmlNamespaceManager NameSpaceMgr = new XmlNamespaceManager(Weather.NameTable);

		NameSpaceMgr.AddNamespace("yweather", rss);

		// Get forecast with XPath
		XmlNodeList condition = Weather.SelectNodes("/rss/channel/item/yweather:condition", NameSpaceMgr);
		//XmlNodeList location = Weather.SelectNodes("/rss/channel/yweather:location", NameSpaceMgr);
		XmlNodeList forecast = Weather.SelectNodes("/rss/channel/item/yweather:forecast", NameSpaceMgr);

		_Condition = condition[0].Attributes["text"].Value;
		_Temperature = condition[0].Attributes["temp"].Value + " degrees.";
		string Fcast = "Today, " + forecast [0].Attributes ["text"].Value + " with a high a " +
			forecast [0].Attributes ["high"].Value + " and a low of " +
			forecast [0].Attributes ["low"].Value;
		_Forecast = Fcast;

	}

	private void GetHazards()
	{
        //http://alerts.weather.gov/cap/wwaatmget.php?x=OKZ060&y=0
        string SavedLocation = "http://alerts.weather.gov/cap/wwaatmget.php?x=OKZ060&y=0";
		XmlDocument XMLWeather = new XmlDocument();
		XMLWeather.Load(SavedLocation);
		XmlNamespaceManager NameSpaceMgr = new XmlNamespaceManager(XMLWeather.NameTable);
        XmlNodeList DocumentNode = XMLWeather.SelectNodes("//summary");
        _Hazards = DocumentNode[0].InnerText;
	}


    
}
