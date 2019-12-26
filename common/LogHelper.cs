using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OracleDataTools.common
{
    public class LogHelper
    {
        public static readonly log4net.ILog Loginfo = log4net.LogManager.GetLogger("loginfo");

        public static readonly log4net.ILog Logerror = log4net.LogManager.GetLogger("logerror");

        public static void WriteLog(string info)
        {
            try
            {
                if (Loginfo.IsInfoEnabled)
                {
                    Loginfo.Info(info);
                }
            }
            catch
            {

            }
        }

        public static void WriteLog(string info, Exception ex)
        {
            try
            {
                if (Logerror.IsErrorEnabled)
                {
                    Logerror.Error(info, ex);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
