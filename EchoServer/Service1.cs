using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer
{
    public partial class Echo : ServiceBase
    {
        public Echo()
        {
            InitializeComponent();
        }

        static WebServer ws = new WebServer();
        protected override void OnStart(string[] args)
        {
            // start web server...
            var t = new System.Threading.Thread(() =>
            {
                ws.StartListening();
            });
            t.Start();
        }

        protected override void OnStop()
        {
            // start web server...
            ws.StopListenting();
        }

        public static void Main(string[] args)
        {
            ws.StartListening();
            Console.WriteLine("Done, about to exit.");
            Console.ReadLine();
            ws.StopListenting();
        }
    }
}
