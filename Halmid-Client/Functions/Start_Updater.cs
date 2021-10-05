using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Halmid_Client.Functions
{
    class Start_Updater
    {
        public static void Start()
        {
            try
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = AppDomain.CurrentDomain.BaseDirectory + @"Halmid-Updater.exe";
                Process.Start(start);
                Environment.Exit(1);
            }catch(Exception) 
            { 
                MessageBox.Show("Error\nCannot open updater\nPlease check if root folder contains it."); 
            }
        }
    }
}
