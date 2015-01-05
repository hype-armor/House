using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WolframAlphaNET;
using WolframAlphaNET.Objects;

namespace OpenEcho
{
    class Wolfram
    {
        
        public string Query(string question)
        {
            WolframAlpha wa = new WolframAlpha("API-KEY");
            QueryResult results = wa.Query(question);

            string ret = "";
            if (results.Error != null)
            {
                return results.Error.Message;
            }
            else if (results.Pods.Count <= 0)
            {
                ret = "Wolfram|Alpha doesn't know how to interpret your input.";
            }
            else
	        {
                ret = results.Pods[1].SubPods[0].Plaintext;
	        }
            
            return ret.Replace(" | ", ", ");
        }
    }
}
