using DeviceId;

namespace Halmid_Client.Windows.Login
{
    class GetID
    {
        public static string GetSerial()
        {
            string MachineSupportID = new DeviceIdBuilder()
                        .AddMacAddress(true, true)
                        .AddMotherboardSerialNumber()
                        .AddProcessorId()
                        .AddSystemDriveSerialNumber()
                        .ToString();
            return MachineSupportID.Substring(0, 34);
        }
    }
}
