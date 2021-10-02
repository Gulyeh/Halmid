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
                        {
                            entry.ExtractToFile(Path.Combine(zipPath, entry.FullName), true);
                        }
                    }
                }
                File.Delete(zipPath + "/Update_Pack.zip");
                return Task.CompletedTask;
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); return Task.CompletedTask; }
        }
    }
}
