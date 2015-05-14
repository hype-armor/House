/*
    OpenEcho is a program to automate basic tasks at home all while being handsfree.
    Copyright (C) 2015  Gregory Morgan

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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OpenEcho
{
    class Speech
    {
        
        public static bool Mute
        {
            set
            {
                //public static WindowsMicMute micMute = new WindowsMicMute();
                // if true then mute else if false then unmute mic.
            }
        }

        public static string Understood;
        public static bool Silent = false;

        private static List<Action> q = new List<Action>();

        static Speech()
        {
            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        if (q.Count() > 0)
                        {
                            Action a = q.First();
                            a.Invoke();
                            q.Remove(a);
                        }
                        
                        Thread.Sleep(5);
                    }
                });
        }

        public static void say(string text, string title = "OpenEcho")
        {
            q.Add(new Action(() =>
                {
                    char[] alphaNumeric = new char[] 
                    { 
                        'a','b','c','d','e','f','g','h','i','j','k',
                        'l','m','n','o','p','q','r','s','t','u','v',
                        'w','x','y','z','1','2','3','4','5','6','7','8','9','0',
                        '.',',',' '
                    };
                    string result = "";
                    foreach (char c in text)
                    {
                        char letter = c.ToString().ToLower()[0];
                        if (alphaNumeric.Contains(letter))
                        {
                            result += c;
                        }
                        else
                        {
                            result += " ";
                        }
                    }

                    result = result.Replace("   ", " ").Replace("  ", " ");

                    if (Silent)
                    {
                        PrintMsg(text, title);
                    }
                    else
                    {
                        try
                        {
                            Console.WriteLine(text);
                        }
                        catch (Exception e)
                        {
                            Silent = true;
                            PrintMsg(e.Message, e.Source);
                            PrintMsg(text, title);
                        } 
                    }
                })
            );
        }

        private static void PrintMsg(string text, string title)
        {
            Console.WriteLine();
        }
    }
}
