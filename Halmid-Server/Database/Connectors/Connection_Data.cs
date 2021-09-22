using System;

namespace Halmid_Server.Database.Connectors
{
    public class Connection_Data
    {
        public static string server = "localhost";
        public static string database = "helmid_db";
        public static string user = "root";
        public static string password = "root";
        public static string port = "3306";
        public static string sslM = "Required";
        public static string connectionString = String.Format("server={0};port={1};user id={2}; password={3}; database={4}; SslMode={5}; Allow User Variables=True; Pooling=True;", server, port, user, password, database, sslM);
    }
}
