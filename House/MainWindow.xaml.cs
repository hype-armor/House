/*
    House is a program to automate basic tasks at home all while being handsfree.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Threading;
using ExtensionMethods;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace OpenEcho
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static System.Timers.Timer aTimer;
        private Quartz quartz = new Quartz();
        private QueryClassification qc = new QueryClassification();

        public MainWindow()
        {
            InitializeComponent();
            
            Speech.micMute.UnMuteMic();
            aTimer = new System.Timers.Timer(1000); 
            aTimer.Elapsed += Timeout;
            txtInput.Focus();

            Speech.micMute.UnMuteMic();

            quartz.Init();
            qc.Init();
        }
        private void Timeout(object sender, System.Timers.ElapsedEventArgs e)
        {
            Speech.micMute.MuteMic();
            aTimer.Stop();

            ProcessInput();
        }

        private void ProcessInput()
        {
            string input = "";
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                input = txtInput.Text;

            }));
            
            do
            {
                Thread.Sleep(50); // make sure input has been populated.
            } while (input == "");
            input = input.Replace("  ", " ").Trim();
            int WordCount = input.Split(new char[] {' '}).Count();

            KeyValuePair<QueryClassification.Actions, string> term = qc.Classify(input);

            if (term.Key == QueryClassification.Actions.wikipedia)
            {
                Wikipedia wikipedia = new Wikipedia();
                input = input.Replace(term.Value, "").Trim();
                string ret = wikipedia.Search(input);
                Speech.say(ret);
            }
            else if (term.Key == QueryClassification.Actions.newAction)
            {
                // usage: add search term, x to y.
                input = input.Replace(term.Value, "").Trim();
                string[] words = input.Split(new string[] { " to " }, StringSplitOptions.RemoveEmptyEntries);

                string verb = words[0];
                QueryClassification.Actions action = 
                    (QueryClassification.Actions)Enum.Parse(typeof(QueryClassification.Actions), words[1], true);

                qc.AddVerbToAction(verb, action);
            }
        }

        private void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            aTimer.Stop();
            aTimer.Start();
        }

        private void cbSilent_Checked(object sender, RoutedEventArgs e)
        {
            Speech.Silent = cbSilent.IsChecked == true ? true : false; // Because of type 'bool?'.
        }

        private void cbSilent_Unchecked(object sender, RoutedEventArgs e)
        {
            Speech.Silent = cbSilent.IsChecked == true ? true : false;
        }

    }
}
