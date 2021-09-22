using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Halmid_Client.Functions
{
    class Save_Windowsize
    {
        public static void Save(double width, double height)
        {
            XDocument doc = XDocument.Load(Directory.GetCurrentDirectory() + @"\Config.xml");
            var data = doc.Root.Descendants("Window_Width").FirstOrDefault();
            var data1 = doc.Root.Descendants("Window_Height").FirstOrDefault();
            data.SetValue(width);
            data1.SetValue(height);
            doc.Save(Directory.GetCurrentDirectory() + @"\Config.xml");
        }
    }
}
