using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading;
using System.IO;
using System.Configuration;

namespace Percepciones.WPF.Entidades
{
    public static class LogApp
    {
        private static readonly object Locker = new object();
        private static XmlDocument _doc = new XmlDocument();

        public static void IniciarArchivo()
        {
            if (File.Exists(ConfigurationManager.AppSettings.Get("carpetaOutput") + ConfigurationManager.AppSettings.Get("archivoLog")))
                _doc.Load(ConfigurationManager.AppSettings.Get("carpetaOutput") + ConfigurationManager.AppSettings.Get("archivoLog"));
            else
            {
                var root = _doc.CreateElement("hosts");
                _doc.AppendChild(root);
            }
            for (int i = 0; i < 100; i++)
            {
                new Thread(new ThreadStart(DoSomeWork)).Start();
            }
        }

        public static void LogError(string proceso, string error)
        {
            lock (Locker)
            {
                var el = (XmlElement)_doc.DocumentElement.AppendChild(_doc.CreateElement("log"));
                el.SetAttribute("proceso", proceso);
                el.AppendChild(_doc.CreateElement("Error")).InnerText = error;
                _doc.Save(ConfigurationManager.AppSettings.Get("carpetaOutput") + ConfigurationManager.AppSettings.Get("archivoLog"));
            }
        }

        public static void LogInfo(string proceso, string info)
        {
            lock (Locker)
            {
                var el = (XmlElement)_doc.DocumentElement.AppendChild(_doc.CreateElement("log"));
                el.SetAttribute("proceso", proceso);
                el.AppendChild(_doc.CreateElement("info")).InnerText = info;
                _doc.Save(ConfigurationManager.AppSettings.Get("carpetaOutput") + ConfigurationManager.AppSettings.Get("archivoLog"));
            }
        }

        public static void DoSomeWork()
        {
            /*

             * Here you will build log messages

             */
            //LogP("192.168.1.15", "alive");
        }
        //public static void Log(string hostname, string state)
        //{
        //    lock (Locker)
        //    {
        //        var el = (XmlElement)_doc.DocumentElement.AppendChild(_doc.CreateElement("host"));
        //        el.SetAttribute("Hostname", hostname);
        //        el.AppendChild(_doc.CreateElement("State")).InnerText = state;
        //        _doc.Save(ConfigurationManager.AppSettings.Get("carpetaOutput") + ConfigurationManager.AppSettings.Get("archivoLog"));
        //    }
        //}
    }
}
