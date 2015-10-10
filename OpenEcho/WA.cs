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

using System.Linq;
using WolframAlphaNET;
using WolframAlphaNET.Objects;

namespace OpenEcho
{
    class Wolfram
    {
        
        public string Query(string id, string question, MessageSystem speech)
        {
            int ResponseTimeID = ResponseTime.Start(id, QueryClassification.Actions.wolframAlpha, speech);

            WolframAlpha wa = new WolframAlpha("API-KEY");
            QueryResult results = wa.Query(question);

            ResponseTime.Stop(QueryClassification.Actions.wolframAlpha, ResponseTimeID);

            string ret = "";
            if (results.Error != null)
            {
                return results.Error.Message;
            }
            else if (results.Pods.Count <= 0)
            {
                ret = "Wolfram Alpha doesn't know how to interpret your input.";
            }
            else
	        {
                ret = results.Pods[1].SubPods[0].Plaintext;
	        }
            
            return ret.Replace(" | ", ", ");
        }
    }
}
