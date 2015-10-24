using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace EchoServer
{
    class Quartz
    {

        private List<Timer> timers = new List<Timer>();
        private static List<Alarm> alarms = new List<Alarm>();

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
                //Speech.say(name);
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

        static Quartz()
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

        public void Understand(string input)
        {
            WordsToNumbers wtn = new WordsToNumbers();

            List<string> words = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            string minutesStr = words.Contains("minutes") ? words[words.IndexOf("minutes") - 1] : "";

            int minutes = wtn.retInt(minutesStr); // does this work with "forty five" minutes...nope

            CreateTimer(new TimeSpan(0, minutes, 0));

            //Speech.say("I have created a timer for " + minutes.ToString() + " minutes.");
        }

        private static void CheckAlarms()
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

        private static void Buzz(Alarm alarm)
        {
            //Speech.say(alarm.alarmTime.ToShortTimeString());
        }

        public static void CreateAlarm(DateTime alarmTime, string name = "")
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

            //Speech.say("I've set the timer " + name + " for you");
        }
    }

    
}
