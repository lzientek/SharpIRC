﻿#region License
// Copyright 2013 Tama Waddell <me@tama.id.au>
// 
// This file is a part of SharpIRC. <https://github.com/tamaw/SharpIRC>
//  
// This source is subject to the Microsoft Public License.
// <http://www.microsoft.com/opensource/licenses.mspx#Ms-PL>
//  All other rights reserved.
#endregion
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using IRC;
using SharpIRC.Views;

namespace SharpIRC.ViewModel
{
    public sealed class ServerViewModel : IIrcTabItemModel
    {
        public ObservableCollection<MessageDate> Messages { get; private set; }

        public ServerViewModel(Client ircClient)
        {
            _ircClient = ircClient;
            Messages = new ObservableCollection<MessageDate>();
        }

        public void Message(string message)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)(() => Messages.Add(new MessageDate()
                {
                    Message = message,
                    TimeStamps = String.Format("[{0:HH:mm:ss}]", DateTime.Now)
                })));
        }

        public void Clear()
        {
            Messages.Clear();
        }

        public string Header
        {
            get
            {
                return _ircClient.Server;
            }
        }

        public Type Type
        {
            get
            {
                return GetType();
            }
        }

        private Client _ircClient;
    }

}
