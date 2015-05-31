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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace OpenEcho
{
    class Wikipedia
    {
        string url = "";
        public string Search(string Subject, bool Short = false)
        {
            url = FormatURL(Subject);

            HtmlDocument doc = GetDocument(url);
            if (!doc.DocumentNode.HasChildNodes)
            {
                return "Wikipedia did not return a valid result.";
            }
            string p = doc.DocumentNode.SelectSingleNode("/p").InnerText;

            Regex parenths = new Regex("\\([^()]*\\)");
            while (parenths.IsMatch(p))
            {
                p = parenths.Replace(p, "");
            }
            Regex bracket = new Regex("\\[.*?\\]");
            p = bracket.Replace(p, "");

            if (Short)
            {
                p = p.Split(new char[] { '.' }).First();
            }

            return p;
        }

        private string FormatURL(string Subject, int Section = 0)
        {
            return "http://en.wikipedia.org/w/api.php?action=parse&page=" + Subject + "&format=xml&prop=text&section=" +
                Section.ToString() + "&redirects";
        }

        private static HtmlDocument GetDocument(string url)
        {
            WebClient wc = new WebClient();
            string html = wc.DownloadString(url);
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(html);

            if (html.Contains("error code=\"missingtitle\""))
            {
                // the page does not exist.
                return new HtmlDocument();
            }

            string nodes = xml.SelectSingleNode("//text").InnerText;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(nodes);
            return doc;
        }
    }
}
