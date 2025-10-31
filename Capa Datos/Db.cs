using System.Configuration;
using MySqlConnector;
namespace Bases_de_datos_II
{
    public static class Db
    {
        public static string ConnStr =>
            ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;

        #if USE_MYSQLCONNECTOR
        public static MySqlConnection GetConn() => new MySqlConnection(ConnStr);
            #else
        public static MySqlConnection GetConn() => new MySqlConnection(ConnStr);
    #endif
    }
}
