using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

class News
{
    private static Dictionary<string, string> _NationalNews = new Dictionary<string, string>();
    public Dictionary<string, string> NationalNews { get { return _NationalNews;} }

    private static Dictionary<string, string> _LocalNews = new Dictionary<string, string>();
    public Dictionary<string, string> LocalNews { get { return _LocalNews; } }

    private static bool _Loaded = false;
    public bool Loaded { get { return _Loaded; } }
    static News()
    {
        Task.Factory.StartNew(() =>
        {
            while (true)
            {
                _Loaded = false;

                // National News
                //http://news.yahoo.com/rss/us
                string url = "http://news.yahoo.com/rss/us";
                // Create a new XmlDocument
                XmlDocument xDocument = new XmlDocument();
                xDocument.Load(url);
                XmlNodeList NewsStoryNodes = xDocument.SelectNodes("//item");

                
                foreach (XmlNode node in NewsStoryNodes)
                {
                    string title = node["title"].InnerText;
                    string story = RemoveHTMLTags(node["description"].InnerText);
                    _NationalNews.Add(title, story);
                }


                // Local News
                url = "http://www.newson6.com/category/208401/newson6com-news-rss?clienttype=rss";
                // Create a new XmlDocument
                xDocument = new XmlDocument();
                xDocument.Load(url);
                NewsStoryNodes = xDocument.SelectNodes("//item");

                foreach (XmlNode node in NewsStoryNodes)
                {
                    string title = node["title"].InnerText;
                    string story = RemoveHTMLTags(node["description"].InnerText);
                    _LocalNews.Add(title, story);
                }

                _Loaded = true;
                Thread.Sleep(900000);
            }
        });
    }

    public static string RemoveHTMLTags(string source)
    {
        return Regex.Replace(source, "<.*?>", string.Empty);
    }
}
