using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace OpenEcho
{
    class Quartz
    {
        private static List<Timer> Timers = new List<Timer>();
        private static List<Alarm> Alarms = new List<Alarm>();

        static Quartz()
        {
            // load stored alarms and timers.

            // check if alarm time has passed.
            foreach (Alarm alarm in Alarms)
            {
                if (alarm.AlarmTime < DateTime.Now)
                {
                    
                }
            }

            // check if timer has elapsed.
        }

        public void CreateAlarm(DateTime AlarmTime, string Name = "")
        {
            Alarm a = new Alarm();
            a.AlarmTime = AlarmTime;
            a.Name = Name;
            Alarms.Add(a);
        }

        public void CreateTimer(int Duration, string Name = "")
        {
            Timer t = new Timer();
            t.Duration = Duration;
            t.Name = Name;
            Timers.Add(t);
        }
    }

    private class Timer
    {
        public string Name = "Timer";
        public int Duration = 0;
    }

    private class Alarm
    {
        // AlarmClock clock = new AlarmClock(someFutureTime);
        // clock.Alarm += (sender, e) => MessageBox.Show("Wake up!");
        //

        public string Name = "Alarm";
        public DateTime AlarmTime = new DateTime();
        public bool Snoozed = false;
        public bool Enabled = true;
    }
}
