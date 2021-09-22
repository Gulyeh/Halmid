using Halmid_Client.Variables;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Halmid_Client.Windows.User_Settings
{
    /// <summary>
    /// Logika interakcji dla klasy User_Settings.xaml
    /// </summary>
    public partial class User_Settings : Window
    {
        public User_Settings(double width, double height)
        {
            InitializeComponent();
            this.Width = width;
            this.Height = height;
        }
        private void ExitWindow(object sender, RoutedEventArgs e)
        {
            this.Owner.Top = this.Top;
            this.Owner.Left = this.Left;
            this.Owner.Show();
            Close();
        }
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Fullscreen();
            }
            else if (e.ClickCount == 1)
            {
                this.DragMove();
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Menu_List.SelectedIndex = 0;
        }
        private void Change_Menu_Selection(object sender, RoutedEventArgs e)
        {
            switch (Menu_List.SelectedIndex)
            {
                case 0:
                    MyAccount account = new MyAccount();
                    Main_Window.Content = account;
                    break;
                default:
                    Main_Window.Content = null;
                    break;
            }
        }
        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            Fullscreen();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (MainWindow_Data.Window_Height != this.Height || MainWindow_Data.Window_Width != this.Width || MainWindow_Data.Window_Top != this.Top || MainWindow_Data.Window_Left != this.Left)
            {
                this.Owner.Height = this.Height;
                this.Owner.Width = this.Width;
                this.Owner.Top = this.Top;
                this.Owner.Left = this.Left;
            }
        }
        public void Fullscreen()
        {
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(this).Handle);

            if (this.Height == screen.WorkingArea.Height && this.Width == screen.WorkingArea.Width)
            {
                this.Width = MainWindow_Data.Window_Width;
                this.Height = MainWindow_Data.Window_Height;
                this.Top = MainWindow_Data.Window_Top;
                this.Left = MainWindow_Data.Window_Left;
            }
            else
            {
                MainWindow_Data.Window_Height = this.Height;
                MainWindow_Data.Window_Width = this.Width;
                MainWindow_Data.Window_Top = this.Top;
                MainWindow_Data.Window_Left = this.Left;

                this.Height = screen.WorkingArea.Height;
                this.Width = screen.WorkingArea.Width;
                this.Top = (screen.WorkingArea.Height - this.Height) / 2 + screen.WorkingArea.Top;
                this.Left = (screen.WorkingArea.Width - this.Width) / 2 + screen.WorkingArea.Left;
            }
        }

    }
}
