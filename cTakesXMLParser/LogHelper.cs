using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cTakesXMLParser
{
    class LogHelper
    {
        public static void SaveLogInfo(string message, string trace)
        {
            string logFile = "notes_log.txt";
            string log_files_folder = ConfigurationManager.AppSettings["log_files_folder"];
            string log_path = System.IO.Path.Combine(log_files_folder, logFile);

            message = DateTime.Now.ToString() + " :\r\n" + message + "\r\n" + "Trace: " + "\r\n" + trace + "\r\n" + new string('-', 100) + "\r\n";
            File.AppendAllText(log_path, message);
        }
    }
}
