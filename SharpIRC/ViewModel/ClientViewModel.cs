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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using IRC;
using SharpIRC.Annotations;
using SharpIRC.Views;

namespace SharpIRC.ViewModel
{
    // TODO this is more like a whole IRC viewModel rather serverViewModel is more the client
    public class ClientViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<IIrcTabItemModel> IrcTabItems { get; set; } // TODO possibly rename to irc tabs

        public string Nickname {
            get { return Client.Nickname; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    Client.Nickname = value;
                    OnPropertyChanged();
                }
            }
        }

        public Client Client {
            get { return _client ?? (_client = ((App) Application.Current).IRCClient); }
        }

        public ClientViewModel()
        {
            IrcTabItems = new ObservableCollection<IIrcTabItemModel>();
        }



        private Client _client;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
