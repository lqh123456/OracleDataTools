using System.Text;

namespace OracleDataTools.common
{
    public class DBConnect
    {
        public static string GetConnectStr()
        {
            var iniFileHelper = new IniFileHelper();
            var sb = new StringBuilder(300);
            iniFileHelper.GetIniString("Database", "Service", "", sb, sb.Capacity);
            var serverIP = sb.ToString();
            iniFileHelper.GetIniString("Database", "Port", "", sb, sb.Capacity);
            var serverPort = sb.ToString();
            iniFileHelper.GetIniString("Database", "Instance", "", sb, sb.Capacity);
            var instance = sb.ToString();
            iniFileHelper.GetIniString("Database", "Username", "", sb, sb.Capacity);
            var username = sb.ToString();
            iniFileHelper.GetIniString("Database", "Password", "", sb, sb.Capacity);
            var password = sb.ToString();

            var connstr = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" + serverIP +
                          ") (PORT=" + serverPort + ")))(CONNECT_DATA=(SERVICE_NAME= " + instance + ")));User Id=" +
                          username + "; Password=" + password + "";


            return connstr;
        }
    }
}