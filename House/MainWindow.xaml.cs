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

            string wiki = "search wikipedia for";
            string addAction = "add search term";
            if (input.Contains(wiki))
            {
                Wikipedia wikipedia = new Wikipedia();
                string term = input.Replace(wiki, "");
                string ret = wikipedia.Search(term);
                Speech.say(ret);
            }
            else if (input.Contains(addAction))
            {
                // usage: add search term, x to y.
                string tInput = input.Replace(addAction, "")
                    .Trim();
                string[] words = tInput.Split(new string[] { " to " }, StringSplitOptions.RemoveEmptyEntries);

                string verb = words[0];
                string action = words[1];

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
