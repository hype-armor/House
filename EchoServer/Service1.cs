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

        protected override void OnStart(string[] args)
        {
            var t = new System.Threading.Thread(() =>
            {
                Program p = new Program();
                
            });
            t.Start();
        }

        protected override void OnStop()
        {
        }

        public static void Main(string[] args)
        {
            var t = new System.Threading.Thread(() =>
            {
                Program p = new Program();

            });
            t.Start();
        }
    }
}
