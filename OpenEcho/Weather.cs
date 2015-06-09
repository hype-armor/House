/*
    OpenEcho is a program to automate basic tasks at home all while being handsfree.
    Copyright (C) 2015 Gregory Morgan

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;

namespace OpenEcho
{
    class Weather
    {
        private string ZIP = "74037";
        private string _Temperature;
        public string Temperature { get { return _Temperature; } }
        private string _Condition;
        public string Condition { get { return _Condition; } }
        private string _Forecast;
        public string Forecast { get { return _Forecast; } }
        private string _Hazards;
        public string Hazards { get { return _Hazards; } }

        public void Update()
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
                _Temperature = condition[0].Attributes["temp"].Value + " degrees";
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
                _Hazards = hazards == null ? "" : hazards.InnerText;
            }
            catch (Exception e)
            {
                
            }
        }
    }
    
}