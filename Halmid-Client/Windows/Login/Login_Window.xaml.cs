using Halmid_Client.Connectors;
using Halmid_Client.Functions;
using Halmid_Client.Variables;
using Halmid_Client.Windows.Loading;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;

namespace Halmid_Client.Windows.Login
{
    /// <summary>
    /// Logika interakcji dla klasy Login_Window.xaml
    /// </summary>
    public partial class Login_Window : Window
    {
        bool open = false;
        
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        public const int SW_RESTORE = 9;

        public Login_Window()
        {
            InitializeComponent();
            if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
            {
                SwitchToCurrent();
                Process.GetCurrentProcess().Kill();
            }
            if(File.Exists(Directory.GetCurrentDirectory() + @"\Config.xml"))
            {
                ReadConfig();
            }
            else
            {
                CreateConfig();
            }
        }
        private void SaveConfig()
        {
            try
            {
                XDocument doc = XDocument.Load(Directory.GetCurrentDirectory() + @"\Config.xml");
                var data = doc.Root.Descendants("Login").FirstOrDefault();
                data.SetValue(user.Text);
                var data2 = doc.Root.Descendants("Key").FirstOrDefault();
                data2.SetValue(key.Text);
                var data3 = doc.Root.Descendants("LoginID").FirstOrDefault();
                data3.SetValue(id.Text);
                doc.Save(Directory.GetCurrentDirectory() + @"\Config.xml");
            }
            catch(Exception e) { MessageBox.Show(e.ToString()); }
        }
        private void ReadConfig()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(Directory.GetCurrentDirectory() + @"\Config.xml");
                if(doc.SelectSingleNode("//Config/UserData/Status").InnerText != String.Empty)
                {
                    Global_Variables.status = doc.SelectSingleNode("//Config/UserData/Status").InnerText;
                }
                var login = doc.SelectSingleNode("//Config/AutoLogin/Login").InnerText;
                var keys = doc.SelectSingleNode("//Config/AutoLogin/Key").InnerText;
                var logged = doc.SelectSingleNode("//Config/KeepLogged").InnerText;
                if (logged == "True")
                {
                    user.Text = login;
                    key.Text = keys;
                    KeepLogged.IsChecked = Boolean.Parse(logged);
                }
            }
            catch(Exception e) { MessageBox.Show(e.ToString()); CreateConfig(); }
        }
        private void AutoLogin()
        {
            if(KeepLogged.IsChecked == true)
            {
                this.Hide();
                Login();
            }
        }
        private void CreateConfig()
        {
            try
            {
                new XDocument(
                    new XElement("Config",
                        new XElement("AutoLogin",
                            new XElement("Login", ""),
                            new XElement("Key", ""),
                            new XElement("LoginID", "")
                        ),
                        new XElement("UserData",
                            new XElement("Status", "")
                        ),
                        new XElement("WindowData",
                            new XElement("Window_Height", ""), 
                            new XElement("Window_Width", "")
                        ),
                        new XElement("KeepLogged", "False")
                    )
                ).Save(Directory.GetCurrentDirectory() + @"\Config.xml");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        private void SwitchToCurrent()
        {
            IntPtr hWnd = IntPtr.Zero;
            Process process = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(process.ProcessName);
            foreach (Process _process in processes)
            {
                if (_process.Id != process.Id &&
                  _process.MainModule.FileName == process.MainModule.FileName &&
                  _process.MainWindowHandle != IntPtr.Zero)
                {
                    hWnd = _process.MainWindowHandle;

                    ShowWindowAsync(hWnd, SW_RESTORE);
                    break;
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            id.Text = GetID.GetSerial();
            AutoLogin();
        }
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void ExitWindow(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }
        private void AutoLogin_Checked()
        {
            XDocument doc = XDocument.Load(Directory.GetCurrentDirectory() + @"\Config.xml");
            var data = doc.Root.Descendants("KeepLogged").FirstOrDefault();
            data.SetValue(KeepLogged.IsChecked.ToString());
            doc.Save(Directory.GetCurrentDirectory() + @"\Config.xml");
        }
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }
        private async void Login()
        {
            await Task.Run(async () => {
                await this.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    errormsg.Text = "";
                    if (id.Text.Length > 20 && user.Text.Length > 0 && key.Text.Length > 0)
                    {
                        string LoginID = id.Text;
                        LoginButton.IsEnabled = false;
                        try
                        {
                            Create_HubConnection.NewConnection();
                            await Connect_Server.Connect();
                            await Connector.connection.SendAsync("EnterKey", toSha256.sha256(key.Text), Connector.connection.ConnectionId);
                            Connector.connection.On<bool>("CheckKey", async (data) =>
                            {
                                if (data == true)
                                {
                                    await Connector.connection.SendAsync("LoginAccount", LoginID, toSha256.sha256(user.Text), Global_Variables.status);
                                    Connector.connection.On<string, string>("LoginStatus", async (isLogged, status) =>
                                    {
                                        switch (isLogged)
                                        {
                                            case "logged":
                                                UserData.LoginID = LoginID;
                                                UserData.Username = user.Text;
                                                UserData.Status = status;
                                                AutoLogin_Checked();
                                                if (KeepLogged.IsChecked == true)
                                                {
                                                    SaveConfig();
                                                }
                                                Hide();
                                                if (!open)
                                                {
                                                    open = true;
                                                    Loading_Window main = new Loading_Window("Fill_Data");
                                                    main.Show();
                                                }
                                                this.Close();
                                                break;
                                            case "wrong_login":
                                                await Connector.connection.DisposeAsync();
                                                LoginButton.IsEnabled = true;
                                                errormsg.Text = "Wrong login";
                                                if (KeepLogged.IsChecked == true)
                                                {
                                                    this.Show();
                                                }
                                                break;
                                            case "db_offline":
                                                await Connector.connection.DisposeAsync();
                                                LoginButton.IsEnabled = true;
                                                errormsg.Text = "Database is offline";
                                                if (KeepLogged.IsChecked == true)
                                                {
                                                    this.Show();
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    });
                                }
                                else
                                {
                                    errormsg.Text = "Wrong entry key";
                                    await Connector.connection.StopAsync();
                                    LoginButton.IsEnabled = true;
                                    if (KeepLogged.IsChecked == true)
                                    {
                                        this.Show();
                                    }
                                }
                            });
                        }
                        catch (Exception)
                        {
                            await Connector.connection.DisposeAsync();
                            LoginButton.IsEnabled = true;
                            errormsg.Text = "Cannot reach the server";
                            if (KeepLogged.IsChecked == true)
                            {
                                this.Show();
                            }
                        }
                    }
                    else
                    {
                        errormsg.Text = "Incorrect data";
                        if (KeepLogged.IsChecked == true)
                        {
                            this.Show();
                        }
                    }
                }));
            });
        }
    }
}
