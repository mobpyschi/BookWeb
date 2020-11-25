using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
namespace Project_BookStoreCT.Models.ServiceModels
{
    public class PaypalLogger
    {
        public static string LogDirectoryPath = Environment.CurrentDirectory;
        public static void Log(string massages)
        {
            try
            {
                StreamWriter strw = new StreamWriter(LogDirectoryPath + "\\PaypalError.log", true);
                strw.WriteLine("{0} --> {1}", DateTime.Now.ToString("MM//yyyy HH:mm:ss"), massages);
            }
            catch(Exception )
            {
                throw;
            }

        }
        
    }
}