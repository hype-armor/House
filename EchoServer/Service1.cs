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

        WebServer ws = new WebServer();
        protected override void OnStart(string[] args)
        {
            // start web server...
            var t = new System.Threading.Thread(() =>
            {
                ws.Start(IPAddress.Any, 8080, "/");
            });
            t.Start();
        }

        protected override void OnStop()
        {
            // start web server...
            ws.Stop();
        }
    }
}
