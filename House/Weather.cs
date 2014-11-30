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
	public string ZIP { get; set; }
	public string Temperature = "";
	public string Condition = "";
	public string Forecast = "";

	public void Wunderground()
	{
		XmlDocument WUnderWeather = new XmlDocument();
		WUnderWeather.Load("http://api.wunderground.com/api/c0a06bca9f1999b7/conditions/q/" + ZIP + ".xml");
		XmlNamespaceManager NameSpaceMgr = new XmlNamespaceManager(WUnderWeather.NameTable);
		XmlNode WeatherNode = WUnderWeather.SelectNodes("/response/current_observation/weather", NameSpaceMgr)[0];

		string Condition = WeatherNode.InnerText;
	}

	public void Update()
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

		Condition = condition[0].Attributes["text"].Value;
		Temperature = condition[0].Attributes["temp"].Value + " degrees.";
		string Fcast = "Today, " + forecast [0].Attributes ["text"].Value + " with a high a " +
			forecast [0].Attributes ["high"].Value + " and a low of " +
			forecast [0].Attributes ["low"].Value;
		Forecast = Fcast;

	}

	public void Conditions()
	{
		Update ();
		string SavedLocation = "http://weather.yahooapis.com/forecastrss?z=" + ZIP;
		// Create a new XmlDocument
		XmlDocument Weather = new XmlDocument();
		string rss = "http://xml.weather.yahoo.com/ns/rss/1.0";
		string NewLine = Environment.NewLine;
		while (true)
		{
			try
			{
				// Load data
				Weather.Load(SavedLocation);

				// Set up namespace manager for XPath
				XmlNamespaceManager NameSpaceMgr = new XmlNamespaceManager(Weather.NameTable);

				NameSpaceMgr.AddNamespace("yweather", rss);

				// Get forecast with XPath
				XmlNodeList condition = Weather.SelectNodes("/rss/channel/item/yweather:condition", NameSpaceMgr);
				//XmlNodeList location = Weather.SelectNodes("/rss/channel/yweather:location", NameSpaceMgr);
				//XmlNodeList forecast = Weather.SelectNodes("/rss/channel/item/yweather:forecast", NameSpaceMgr);

				Condition = condition[0].Attributes["text"].Value;
				Temperature = condition[0].Attributes["temp"].Value + " degrees fahrenheit";
				break;
			}
			catch
			{
				// Try again.
				Thread.Sleep(5000);
				continue;
			}

		}
	}

	public string GetHazards()
	{
		while (true)
		{
			try
			{
				string SavedLocation = "http://alerts.weather.gov/cap/wwaatmget.php?x=" + GetZone() + "&y=0";
				XmlDocument XMLWeather = new XmlDocument();
				XMLWeather.Load(SavedLocation);
				XmlNamespaceManager NameSpaceMgr = new XmlNamespaceManager(XMLWeather.NameTable);
				XmlNode DocumentNode = XMLWeather.SelectNodes("/", NameSpaceMgr)[0]["feed"]["entry"]["title"];

				XMLWeather = null;
				GC.Collect();
				return DocumentNode.InnerText;
			}
			catch
			{
				// Try again.
				Thread.Sleep(5000);
				continue;
			} 
		}
	}

	private string GetZone()
	{
		while (true)
		{
			try
			{
				List<decimal> Coords = GetCoords();
				WebClient wc = new WebClient();
				byte[] buffer = wc.DownloadData("http://forecast.weather.gov/MapClick.php?textField1=" + Coords[0] + "&textField2=" + Coords[1]);
				string[] html = Encoding.UTF8.GetString(buffer, 0, buffer.Length).Split('>');

				for (int i = 0; i < html.Length; i++)
				{
					if (html[i].Contains("Zone Area Forecast for"))// Tulsa County, OK
					{
						//<a href=\"MapClick.php?zoneid=OKZ060\"
						string[] zone = html[i - 1].Split(new char[] { '"', '\\', '=' }, StringSplitOptions.RemoveEmptyEntries);
						return zone[zone.Length - 1];
					}
				}
			}
			catch
			{
				// Try again.
				Thread.Sleep(5000);
				continue;
			}
		}
	}

	private List<decimal> GetCoords()
	{
		while (true)
		{
			try
			{
				XmlDocument XMLCoords = new XmlDocument();
				XMLCoords.Load("http://query.yahooapis.com/v1/public/yql?q=select%20centroid%20from%20geo.places%20where%20text%3D\"" + ZIP + "\"");

				// Get forecast with XPath
				XmlNamespaceManager NameSpaceMgr = new XmlNamespaceManager(XMLCoords.NameTable);
				XmlNode Nodes = XMLCoords.SelectNodes("/query/results", NameSpaceMgr)[0].ChildNodes[0].ChildNodes[0];
				decimal latitude = Nodes.ChildNodes[0].InnerText.ToDecimal();
				decimal longitude = Nodes.ChildNodes[1].InnerText.ToDecimal();
				List<decimal> Coords = new List<decimal>();
				Coords.Add(latitude);
				Coords.Add(longitude);
				return Coords;
			}
			catch
			{
				// Try again.
				Thread.Sleep(5000);
				continue;
			} 
		}
	}
}
