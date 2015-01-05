using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace OpenEcho
{
    class Quartz
    {
        private static List<Timer> Timers = new List<Timer>();
        private static List<Alarm> Alarms = new List<Alarm>();

        static Quartz()
        {
            // Create temp alarm and timer for testing.
            CreateAlarm(new DateTime(1), "Test Alarm");
            CreateTimer(15000, "Test Timer");

            // load stored alarms and timers.

            // check if alarm time has passed.
            foreach (Alarm alarm in Alarms)
            {
                if (alarm.Enabled == true && 
                    alarm.AlarmTime.TimeOfDay < DateTime.Now.TimeOfDay &&
                    (alarm.AlarmTime.TimeOfDay - DateTime.Now.TimeOfDay) < TimeSpan.FromMinutes(1)
                    ) // just check for time for now.
                {
                    // buzz
                    string Name = alarm.Name.Length == 0 ? "Alarm" : alarm.Name;
                    MessageBox.Show(Name + " has elapsed.");
                }
            }

            // check if timer has elapsed.
        }

        public static void CreateAlarm(DateTime AlarmTime, string Name = "")
        {
            Alarm a = new Alarm();
            a.AlarmTime = AlarmTime;
            a.Name = Name;
            Alarms.Add(a);
        }

        public static void CreateTimer(int Duration, string Name = "")
        {
            Timer t = new Timer();
            t.Duration = Duration;
            t.Name = Name;
            Timers.Add(t);
        }
    }

    class Timer
    {
        public string Name = "Timer";
        public int Duration = 0;
    }

    class Alarm
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
