using ProcessMonitor.ServiceHandler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitor.Service
{
    public partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();
        }

        protected override async void OnStart(string[] args)
        {
            await Agent.StartProcessing();
        }

        protected override async void OnStop()
        {
            await Agent.StopProcessing();
        }
    }
}
