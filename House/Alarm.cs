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

        private List<Timer> timers = new List<Timer>();
        private List<Alarm> alarms = new List<Alarm>();

        private class Timer
        {
            public string name = "Timer";
            public TimeSpan duration;
            private System.Timers.Timer aTimer = new System.Timers.Timer();

            public void Set()
            {
                aTimer = new System.Timers.Timer(duration.TotalMilliseconds);
                // Hook up the Elapsed event for the timer. 
                aTimer.Elapsed += OnTimedEvent;
                aTimer.Enabled = true;
                aTimer.Start();
            }

            private void OnTimedEvent(object sender, ElapsedEventArgs e)
            {
                Speech.say(name);
                aTimer.Stop();
            }
        }

        private class Alarm
        {
            public string name = "";
            public DateTime alarmTime = new DateTime();
            public bool snoozed = false;
            public bool enabled = true;
            public bool acknowledged = false;
        }

        public void Init()
        {
            CreateAlarm(DateTime.Parse("9:17 pm"), "Good Morning");

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
            foreach (Alarm alarm in alarms)
            {
                TimeSpan CurrentTime = DateTime.Now.TimeOfDay;
                TimeSpan AlarmTime = alarm.alarmTime.TimeOfDay;

                bool AlarmIsInPast = (AlarmTime <= CurrentTime);
                double MinutesSinceAlarm = (CurrentTime - AlarmTime).TotalMinutes;

                if (alarm.enabled && AlarmIsInPast)
                {
                    if (!alarm.acknowledged && MinutesSinceAlarm <= 1)
                    {
                        Buzz(alarm);
                        alarm.acknowledged = true;
                        string Name = alarm.name.Length == 0 ? "Alarm" : alarm.name;
                    }
                    else if (alarm.acknowledged && MinutesSinceAlarm > 1)
                    {
                        alarm.acknowledged = false;
                    }
                }
            }
        }

        private void Buzz(Alarm alarm)
        {
            Speech.say(alarm.alarmTime.ToShortTimeString(), "Alarm!");
        }

        public void CreateAlarm(DateTime alarmTime, string name = "")
        {
            Alarm a = new Alarm();
            a.alarmTime = alarmTime;
            a.name = name;
            alarms.Add(a);
        }

        public void CreateTimer(TimeSpan duration, string name = "Timer")
        {
            Timer t = new Timer();
            t.duration = duration;
            t.name = name;
            timers.Add(t);
            t.Set();

            Speech.say("I've set the timer " + name + " for you");
        }
    }

    
}
