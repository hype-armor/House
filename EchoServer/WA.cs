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
using System.Linq;
using WolframAlphaNET;
using WolframAlphaNET.Objects;

namespace EchoServer
{
    class Wolfram
    {
        public static void Go(Guid guid, string question, MessageSystem messageSystem)
        {
            int ResponseTimeID = ResponseTime.Start(guid, QueryClassification.Actions.wolframAlpha, messageSystem);

            WolframAlpha wa = new WolframAlpha("API-KEY");
            QueryResult results = wa.Query(question);

            string ret = "";
            if (results.Error != null)
            {
                ret = results.Error.Message;
            }
            else if (results.Pods.Count <= 0)
            {
                ret = "Wolfram Alpha doesn't know how to interpret your input.";
            }
            else
	        {
                ret = results.Pods[1].SubPods[0].Plaintext;
	        }

            ResponseTime.Stop(QueryClassification.Actions.wolframAlpha, ResponseTimeID);
            messageSystem.Post(guid, Message.Type.output, ret.Replace(" | ", ", ")); ;
        }
    }
}
