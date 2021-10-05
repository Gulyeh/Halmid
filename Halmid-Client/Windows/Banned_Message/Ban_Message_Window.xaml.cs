using System;
using System.Windows;

namespace Halmid_Client.Windows.Banned_Message
{
    /// <summary>
    /// Logika interakcji dla klasy Ban_Message_Window.xaml
    /// </summary>
    public partial class Ban_Message_Window : Window
    {
        string ban_reason;
        string ban_duration;
        string ban_admin;
        public Ban_Message_Window(string reason, string duration, string admin_name)
        {
            InitializeComponent();
            ban_reason = reason;
            ban_duration = duration;
            ban_admin = admin_name;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
            Ban_Reason.Text = ban_reason;
            Ban_Duration.Text = ban_duration;
            Ban_Admin.Text = ban_admin;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            this.Close();
        }

    }
}
