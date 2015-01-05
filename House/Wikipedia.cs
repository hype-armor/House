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
            string nodes = xml.SelectSingleNode("//text").InnerText;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(nodes);
            return doc;
        }
    }
}
