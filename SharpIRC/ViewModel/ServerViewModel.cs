#region License
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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using IRC;
using SharpIRC.Annotations;
using SharpIRC.Views;

namespace SharpIRC.ViewModel
{
    public sealed class ServerViewModel : IIrcTabItemModel,INotifyPropertyChanged
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

        public string Server
        {
            set { _ircClient.Server = value;
                OnPropertyChanged(); }
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
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
