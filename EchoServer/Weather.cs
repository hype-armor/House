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

using System;
using System.Xml;
using ExtensionMethods;

namespace EchoServer
{
    class Weather
    {
        public static void Go(Guid guid, string input, MessageSystem messageSystem)
        {
            string ZIP = "74037";
            string Temperature;
            string Condition;
            string Forecast;
            string Hazards;

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
                XmlNodeList forecast = Weather.SelectNodes("/rss/channel/item/yweather:forecast", NameSpaceMgr);

                Condition = condition[0].Attributes["text"].Value;
                Temperature = condition[0].Attributes["temp"].Value + " degrees";
                string Fcast = "Today, " + forecast[0].Attributes["text"].Value + " with a high a " +
                    forecast[0].Attributes["high"].Value + " and a low of " +
                    forecast[0].Attributes["low"].Value;
                Forecast = Fcast;

                // Hazards
                SavedLocation = "http://alerts.weather.gov/cap/wwaatmget.php?x=OKZ060&y=0";
                Weather = new XmlDocument();
                Weather.Load(SavedLocation);
                NameSpaceMgr = new XmlNamespaceManager(Weather.NameTable);

                XmlElement hazards = Weather.DocumentElement["entry"]["summary"]; //Weather.SelectNodes("//feed/entry/summary", NameSpaceMgr);
                Hazards = hazards == null ? "" : hazards.InnerText;

                if (input.CleanText().Contains("forcast"))
                {
                    messageSystem.Post(guid, Forecast);
                }
                else
                {
                    messageSystem.Post(guid, "It is currently, " + Temperature + " and " + Condition);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message); 
            }
        }
    }
}