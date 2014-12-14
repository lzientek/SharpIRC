using System.Diagnostics;
using System.Windows;
using SharpIRC.Properties;

namespace SharpIRC.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow
    {

         public SettingsWindow(Window parent)
            : this()
        {
            Owner = parent;
        }

        private SettingsWindow()
        {
            InitializeComponent();

            Settings.Default.SettingChanging += (sender, args) => Debug.WriteLine("setting changing");
            Settings.Default.SettingsSaving += (sender, args) => Debug.WriteLine("settings saved");

            SaveButton.Click += (sender, args) =>
            {
                Save();
                Close();
            };

            CancelButton.Click += (sender, args) => Close();
        }

        private void Save()
        {
            Settings.Default.Nickname = NicknameTextBox.Text;
            Settings.Default.Nickname2 = SecondTextBox.Text;
            Settings.Default.Nickname3 = ThirdTextBox.Text;
            Settings.Default.RealName = RealNameTextBox.Text;
            Settings.Default.LeaveMessage = LeaveTextBox.Text;
            Settings.Default.Server = ServerTextBox.Text;
            Settings.Default.Port = PortTextBox.Text;
            Settings.Default.Save();
           
        }


    }
}
