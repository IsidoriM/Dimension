using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;


namespace MenuPinProvisioning
 
{
    public class Logger
    {
        //private static readonly string logName = HttpContext.Current.Server.MapPath("~/log.txt");
        private static readonly string logPath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + @"App_data\";
        //private static readonly string logPath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;


        private static readonly string logName = logPath + "loggerMenuPin.txt";
        private const int maxFileSize = 40000000;

        public static void Append(string txt)
        {
            lock (logName)
            {
                Checksize();
                File.AppendAllText(logName, string.Format("{0}|{1}{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), txt, Environment.NewLine));
            }
        }

        private static void Checksize()
        {
            FileInfo fi = new FileInfo(logName);
            if (fi.Exists && fi.Length > maxFileSize)
            {
                File.Move(logName, string.Format("{0}loggerMenuPin{1}.txt", logPath, DateTime.Now.ToString("_yyyyMMdd_HHmmss")));
            }
        }



        public static void DeleteLog()
        {
            FileInfo fi = new FileInfo(logName);
            if (fi.Exists)
            {
                fi.Delete();
            }            

        }

        public static String ReadFileLog()
        {
            FileInfo fi = new FileInfo(logName);
            if (fi.Exists)
            {
                return File.ReadAllText(logName);
            }else
            {
                return null;
            }
        }

    }
}