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

using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System;

namespace EchoServer
{
    class ResponseTime
    {
        private Dictionary<QueryClassification.Actions, Dictionary<int, Stopwatch>> actionTimes
            = new Dictionary<QueryClassification.Actions, Dictionary<int, Stopwatch>>();

        public int Start(Guid guid, QueryClassification.Actions action, MessageSystem messageSystem)
        {
            Stopwatch sw = new Stopwatch();
            if (!actionTimes.ContainsKey(action))
            {
                Dictionary<int, Stopwatch> temp = new Dictionary<int, Stopwatch>();
                
                temp.Add(0, sw);
                actionTimes.Add(action, temp);

                return 0;
            }

            Dictionary<int, Stopwatch> times = actionTimes[action];
            int timerID = times.Count();
            Dictionary<int, Stopwatch>.ValueCollection stopwatches = times.Values;
            
            long sum = 0;
            foreach (Stopwatch stopwatch in stopwatches)
            {
                sum += stopwatch.ElapsedMilliseconds;
            }
            double avg = sum / stopwatches.Count();
            
            if (avg >= 60000)
            {
                messageSystem.Post(guid, Message.Type.tempResponse, "Please wait. I might take a while.");
            }
            else if (avg >= 20000)
            {
                messageSystem.Post(guid, Message.Type.tempResponse, "Please wait.");
            }
            else if (avg >= 10000)
            {
                messageSystem.Post(guid, Message.Type.tempResponse, "hmmmmmm, okay, hold on a second.");
            }
            else if (avg >= 5000)
            {
                messageSystem.Post(guid, Message.Type.tempResponse, "Loading");
            }

            times.Add(timerID, sw);
            sw.Start();
            

            return timerID;
        }

        public void Stop(QueryClassification.Actions action, int id)
        {
            Dictionary<int, Stopwatch> times = actionTimes[action];
            Stopwatch sw = times[id];
            sw.Stop();
        }
    }
}
