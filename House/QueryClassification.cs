using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace OpenEcho
{

    // for information on saving the object to a file on change. https://msdn.microsoft.com/en-us/library/et91as27.aspx
    [Serializable()]
    class QueryClassification
    {
        const string saveLocation = "QueryClassification.bin";

        Dictionary<string, HashSet<string>> terms = new Dictionary<string, HashSet<string>>();

        public void Init()
        {
            terms = LoadDict(saveLocation);

            string action = "search";
            HashSet<string> verbs = new HashSet<string>();
            //searchVerbs.Add("", keyWord);
            try
            {
                verbs.Add("lookup");
                verbs.Add("look up");
                verbs.Add("search");
                verbs.Add("what is");
                terms.Add(action, verbs);
            }
            catch (Exception)
            {
                // searchVerbs are already added.
            }
            SaveDict();
        }

        public void AddVerbToAction(string verb, string action)
        {
            HashSet<string> verbs = terms[action];
            verbs.Add(verb);
            terms[action] = verbs;

            SaveDict();
        }

        private Dictionary<string, HashSet<string>> LoadDict(string name)
        {
            Dictionary<string, HashSet<string>> dict = new Dictionary<string, HashSet<string>>();

            if (File.Exists(saveLocation))
            {
                Stream FileStream = File.OpenRead(saveLocation);
                BinaryFormatter deserializer = new BinaryFormatter();
                dict = (Dictionary<string, HashSet<string>>)deserializer.Deserialize(FileStream);
                FileStream.Close();
            }
            return dict;
        }

        private void SaveDict()
        {
            if (File.Exists(saveLocation))
            {
                string newFileName = saveLocation.Replace(".bin", " ") + 
                    DateTime.Now + 
                    ".bin";
                newFileName = newFileName
                    .Replace('/', '.')
                    .Replace(':','.');
                System.IO.File.Move(saveLocation, newFileName);
            }
            Stream FileStream = File.Create(saveLocation);
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(FileStream, terms);
            FileStream.Close();
        }
    }

    class Query
    {
        string subject;
        string verb;
        Action t;
    }
}
