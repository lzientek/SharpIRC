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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using IRC;
using LucasHelpers.Wpf.Prompt;
using MahApps.Metro.Controls;
using SharpIRC.ViewModel;
using SharpIRC.Views;

namespace SharpIRC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly App _app = (App)Application.Current;
        private readonly ClientViewModel _clientViewModel;
        private IIrcTabItemModel _currentChannelViewModel;
        private ServerViewModel _serverTab;

        public MainWindow()
        {
            InitializeComponent();
            _clientViewModel = new ClientViewModel();
            DataContext = _clientViewModel;
        }

        private void AddServerChannel()
        {
            _serverTab = new ServerViewModel(_app.IRCClient);
            _app.IRCClient.Logger += _serverTab.Message;
            _clientViewModel.IrcTabItems.Add(_serverTab);
            ChannelTabControl.SelectedIndex = 0;

            _serverTab.Message("Welcome to SharpIRC.");
            _serverTab.Message("See README for more information.");
            _serverTab.Message("Type /help to begin.");
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            AddServerChannel();
            AddUserCommandlets();

            SettingsButton.Click += (o, args) => new SettingsWindow(this).Show();
        }



        private void AddUserCommandlets()
        {
            UserCommand.AddCommandlet("help", parameters => _currentChannelViewModel.Message(
                "Commands: /connect, /join, /leave, /exit, /clear, /say, /nick, /help"));
            UserCommand.AddCommandlet("exit", parameters => _app.Shutdown(0));
            UserCommand.AddCommandlet("clear", parameters => _currentChannelViewModel.Clear());
            UserCommand.AddCommandlet("nick", parameters =>
            {
                if (parameters.Length == 1 && !string.IsNullOrEmpty(parameters[0]))
                    _clientViewModel.Nickname = parameters[0];
            });
            UserCommand.AddCommandlet("connect", parameters =>
            {
                if (parameters.Length == 1)
                    _app.IRCClient.Server = parameters[0];

                // TODO say connecting to x
                new Thread(() => _app.IRCClient.Connect()).Start();
            });

            UserCommand.AddCommandlet("join", parameters =>
            {
                if (parameters.Length != 1 || string.IsNullOrEmpty(parameters[0])) return;

                // create a channel based off of the first parameter
                Channel channel = _app.IRCClient.CreateChannel(parameters[0]);
                // create a representation of the channel in the view
                var cvm = new ChannelViewModel(channel);
                // add the channel to the tabs list
                _clientViewModel.IrcTabItems.Add(cvm);
                // select the newly created tab
                ChannelTabControl.SelectedIndex = ChannelTabControl.Items.IndexOf(cvm);
                cvm.Channel.NamesList += (sender, list) => Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    var userListBox = FindVisualChildByName<ListBox>(ChannelTabControl, "UsersListView");
                    if (userListBox == null)
                        Debug.WriteLine("not found: null");
                    else
                        userListBox.Items.SortDescriptions.Add(new SortDescription("Nick", ListSortDirection.Ascending));
                }));
                // join the channel
                channel.Join();
            });

            UserCommand.AddCommandlet("leave", parameters =>
            {
                var cvm = ChannelTabControl.SelectedItem as ChannelViewModel;
                if (cvm == null) return;

                if (cvm.Channel.IsConnected)
                {
                    if (parameters.Length == 1 && !string.IsNullOrEmpty(parameters[0]))
                        cvm.Channel.Leave(parameters[0]);
                    else
                        cvm.Channel.Leave();
                }
                _clientViewModel.IrcTabItems.Remove(cvm);
            });
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            var tBox = (TextBox)sender;

            // if it starts in a slash process the command
            if (tBox.Text.StartsWith(UserCommand.CommandStart))
            {
                UserCommand.Cook(tBox.Text);
            }
            else // without a slash this is a user say command
            {
                var channel = _currentChannelViewModel as ChannelViewModel;
                if (channel != null)
                {
                    channel.Say(tBox.Text);
                    channel.Message(tBox.Text);
                }
            }

            tBox.Text = string.Empty;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentChannelViewModel = ChannelTabControl.SelectedItem as IIrcTabItemModel;

            if (_currentChannelViewModel != null)
                Debug.WriteLine("Selection Changed: " + _currentChannelViewModel.Server);
        }

        private T FindVisualChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            T child = default(T);
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var ch = VisualTreeHelper.GetChild(parent, i);
                Debug.WriteLine(ch.ToString());
                child = ch as T;
                if (child != null && child.Name == name)
                    break; // found

                child = FindVisualChildByName<T>(ch, name);

                if (child != null) break; // nothing left
            }
            return child;
        }

        private void Leave_OnCloseClick(object sender, RoutedEventArgs args)
        {
            var cvm = ChannelTabControl.SelectedItem as ChannelViewModel;
            if (cvm == null) return;

            if (cvm.Channel.IsConnected)
            {
                cvm.Channel.Leave();
            }
            _clientViewModel.IrcTabItems.Remove(cvm);
        }

        private void Connect_OnClick(object sender, RoutedEventArgs e)
        {
            PromptChamp prompt = new PromptChamp(new Champ()
            {
                ChampUn = "server : ",
                TextBouton = "Connect",
                TextUn = _serverTab.Server,
                Title = "Connect to the server"
            });

            prompt.Valided += (o, args) =>
            {
                if (!string.IsNullOrEmpty(prompt.Champ.TextUn))
                    _serverTab.Server = prompt.Champ.TextUn;

                new Thread(() => _app.IRCClient.Connect()).Start();
            };

            prompt.Show();
        }

        private void Join_OnClick(object senders, RoutedEventArgs e)
        {

            PromptChamp promptChamp = new PromptChamp(new Champ()
            {
                TextBouton = "Join",
                ChampUn = "Channel :",
                Title = "Join a channel"
            });
            promptChamp.Valided += (send, args) =>
            {
                // create a channel based off of the first parameter
                Channel channel = _app.IRCClient.CreateChannel(promptChamp.Champ.TextUn);
                // create a representation of the channel in the view
                var cvm = new ChannelViewModel(channel);
                // add the channel to the tabs list
                _clientViewModel.IrcTabItems.Add(cvm);
                // select the newly created tab
                ChannelTabControl.SelectedIndex = ChannelTabControl.Items.IndexOf(cvm);
                cvm.Channel.NamesList +=
                    (sender, list) => Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        var userListBox = FindVisualChildByName<ListBox>(ChannelTabControl, "UsersListView");
                        if (userListBox == null)
                            Debug.WriteLine("not found: null");
                        else
                            userListBox.Items.SortDescriptions.Add(new SortDescription("Nick",
                                ListSortDirection.Ascending));
                    }));
                // join the channel
                channel.Join();
            };

            promptChamp.Show();
        }

        private void Clear_OnClick(object sender, RoutedEventArgs e)
        {
            _currentChannelViewModel.Clear();
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var scroll = sender as ScrollViewer;
            if (scroll != null)
            {
                scroll.ScrollToEnd();
            }
        }

        private void Kick_OnClick(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item != null)
            {
                var user = item.DataContext as User;
                var cvm = ChannelTabControl.SelectedItem as ChannelViewModel;
                if (cvm != null && user != null)
                {
                    cvm.Channel.Kick(user);

                }
            }

        }


        private void Invite_OnClick(object sender, RoutedEventArgs e)
        {


            var cvm = ChannelTabControl.SelectedItem as ChannelViewModel;
            if (cvm != null)
            {

                PromptChamp name = new PromptChamp(new Champ()
                {
                    ChampUn = "Nick name",
                    TextBouton = "Invite",
                    Title = "Invite to the channel"
                });

                name.Valided += (o, args) =>
                {
                    if (name.Champ.TextUn != null)
                    {
                        cvm.Channel.Invite(new User(_app.IRCClient, name.Champ.TextUn));
                    }
                };

                name.Show();

            }

        }
    }
}
