﻿using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Speech.Synthesis;
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
        try
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
            string Fcast = "Today, " + forecast[0].Attributes["text"].Value + " with a high a " +
                forecast[0].Attributes["high"].Value + " and a low of " +
                forecast[0].Attributes["low"].Value;
            _Forecast = Fcast;

            // Hazards
            SavedLocation = "http://alerts.weather.gov/cap/wwaatmget.php?x=OKZ060&y=0";
            Weather = new XmlDocument();
            Weather.Load(SavedLocation);
            NameSpaceMgr = new XmlNamespaceManager(Weather.NameTable);

            XmlElement hazards = Weather.DocumentElement["entry"]["summary"]; //Weather.SelectNodes("//feed/entry/summary", NameSpaceMgr);
            _Hazards = hazards.InnerText;
        }
        catch (Exception e)
        {
            say(e.Message);
        }
	}

    private static void say(string text)
    {
        // Initialize a new instance of the SpeechSynthesizer.
        SpeechSynthesizer synth = new SpeechSynthesizer();

        // Configure the audio output. 
        synth.SetOutputToDefaultAudioDevice();

        // Speak a string.
        synth.Speak(text);
    }
}
