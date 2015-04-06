using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenEcho
{
    class SearchEng
    {
        public string Search(string Question)
        {
            if (string.IsNullOrWhiteSpace(Question))
            {
                return "No question";
            }
            WebClient client = new WebClient();

            // Add a user agent header in case the  
            // requested URI contains a query.

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            string BaseURL = "https://www.google.com/search?q=" + Question.Replace(" ", "+");
            Stream data = client.OpenRead(BaseURL);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();

            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

            // There are various options, set as needed
            htmlDoc.OptionFixNestedTags = true;

            // filePath is a path to a file containing the html
            htmlDoc.LoadHtml(s);

            // Use:  htmlDoc.LoadHtml(xmlString);  to load from a string (was htmlDoc.LoadXML(xmlString)

            // ParseErrors is an ArrayList containing any errors from the Load statement
            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
            {
                // Handle any parse errors as required

            }
            else
            {

                if (htmlDoc.DocumentNode != null)
                {
                    HtmlAgilityPack.HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");

                    if (bodyNode != null)
                    {
                        // Do something with bodyNode

                        StreamWriter sr = new StreamWriter("C:\\Users\\greg\\Documents\\webpage.html");
                        sr.Write(s);
                        sr.Close();
                        

                        var className = s.Split(new string[] {"."}, StringSplitOptions.None);
                        for (int i = 0; i < className.Length; i++)
                        {
                            if (className[i].Contains("#212121!important"))
                            {
                                string d = className[i];
                            }
                        }

                        // select="//*[contains(@address,'Downing')]"
                        var pd = bodyNode.SelectNodes("//div[@id='search']//ol//li//div");
                        foreach (var item in pd)
                        {
                            foreach (var n in item.ChildNodes)
                            {
                                string text = n.FirstChild.InnerHtml;
                            }
                        }
                        var p = bodyNode.SelectNodes("//div[@id='search']")[0].InnerText.Split(new string[] {"    "}, StringSplitOptions.None);
                        
                    }
                }
            }
            data.Close();
            reader.Close();

            return "Unable to find the answer you've requested.";
        }
    }

}