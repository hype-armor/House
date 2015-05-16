using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OpenEcho
{
    class ResponseTime
    {
        private static Dictionary<QueryClassification.Actions, Dictionary<int, Stopwatch>> actionTimes
            = new Dictionary<QueryClassification.Actions, Dictionary<int, Stopwatch>>();

        public static int Start(QueryClassification.Actions action)
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
            int id = times.Count();
            Dictionary<int, Stopwatch>.ValueCollection stopwatches = times.Values;
            
            long sum = 0;
            foreach (Stopwatch stopwatch in stopwatches)
            {
                sum += stopwatch.ElapsedMilliseconds;
            }
            double avg = sum / stopwatches.Count();
            
            if (avg >= 60000)
            {
                Speech.say("Please wait. I might take a while.");
            }
            else if (avg >= 5000)
            {
                Speech.say("Loading");
            }

            times.Add(id, sw);
            sw.Start();
            

            return id;
        }

        public static void Stop(QueryClassification.Actions action, int id)
        {
            Dictionary<int, Stopwatch> times = actionTimes[action];
            Stopwatch sw = times[id];
            sw.Stop();
        }
    }
}
