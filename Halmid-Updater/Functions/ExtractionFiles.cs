using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Halmid_Updater.Functions
{
    class ExtractionFiles
    {
        public static Task Extract()
        {
            try
            {
                string zipPath = AppDomain.CurrentDomain.BaseDirectory;
                using (ZipArchive archive = ZipFile.OpenRead(zipPath + "/Update_Pack.zip"))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string dir = entry.FullName;
                        {
                            if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith(@"\"))
                            {
                                string path = zipPath + entry.FullName.Substring(0, entry.FullName.Length - 1);
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }
                                dir = entry.FullName.Substring(0, entry.FullName.Length - 1) + @"\Workaround.txt";
                                entry.ExtractToFile(Path.Combine(zipPath, dir), true);
                                await PermaDelete(Path.Combine(zipPath, dir));
                            }
                            else
                            {
                                entry.ExtractToFile(Path.Combine(zipPath, dir), true);
                            }
                        }
                    }
                }
                await PermaDelete(zipPath);
            }
            catch (Exception) { }
        }
        
        private static Task PermaDelete(string path)
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo("cmd.exe", "/c cd "+ path + " && del Update_Pack.zip")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            p.Start();
            p.WaitForExit();
            
            return Task.CompletedTask;
        }
    }
}
