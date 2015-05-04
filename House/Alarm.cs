using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Media;

namespace OpenEcho
{
    class Quartz
    {

        private List<Timer> Timers = new List<Timer>();
        private List<Alarm> Alarms = new List<Alarm>();

        private class Timer
        {
            public string Name = "Timer";
            public TimeSpan Duration;
            private System.Timers.Timer aTimer = new System.Timers.Timer();

            public void Set()
            {
                aTimer = new System.Timers.Timer(Duration.TotalMilliseconds);
                // Hook up the Elapsed event for the timer. 
                aTimer.Elapsed += OnTimedEvent;
                aTimer.Enabled = true;
                aTimer.Start();
            }

            private void OnTimedEvent(object sender, ElapsedEventArgs e)
            {
                Speech.say(Name);
                aTimer.Stop();
            }
        }

        private class Alarm
        {
            public string Name = "";
            public DateTime AlarmTime = new DateTime();
            public bool Snoozed = false;
            public bool Enabled = true;
            public bool Acknowledged = false;
        }

        public void Init()
        {
            CreateAlarm(DateTime.Parse("9:17 pm"), "Good Morning");

            //CreateTimer(15000, "Test Timer");

            // check if alarm(s) should sound.
            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        CheckAlarms();
                        Thread.Sleep(100);
                    }
                });
        }

        private void CheckAlarms()
        {
            foreach (Alarm alarm in Alarms)
            {
                TimeSpan CurrentTime = DateTime.Now.TimeOfDay;
                TimeSpan AlarmTime = alarm.AlarmTime.TimeOfDay;

                bool AlarmIsInPast = (AlarmTime <= CurrentTime);
                double MinutesSinceAlarm = (CurrentTime - AlarmTime).TotalMinutes;

                if (alarm.Enabled && AlarmIsInPast)
                {
                    if (!alarm.Acknowledged && MinutesSinceAlarm <= 1)
                    {
                        Buzz(alarm);
                        alarm.Acknowledged = true;
                        string Name = alarm.Name.Length == 0 ? "Alarm" : alarm.Name;
                    }
                    else if (alarm.Acknowledged && MinutesSinceAlarm > 1)
                    {
                        alarm.Acknowledged = false;
                    }
                }
            }
        }

        private void Buzz(Alarm alarm)
        {
            Speech.say(alarm.AlarmTime.ToShortTimeString(), "Alarm!");
        }

        public void CreateAlarm(DateTime AlarmTime, string Name = "")
        {
            Alarm a = new Alarm();
            a.AlarmTime = AlarmTime;
            a.Name = Name;
            Alarms.Add(a);
        }

        public void CreateTimer(TimeSpan Duration, string Name = "")
        {
            Timer t = new Timer();
            t.Duration = Duration;
            t.Name = Name;
            Timers.Add(t);
            t.Set();
        }
    }

    
}
