using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using NLog;

namespace WindowsRemoteControlService
{
    /// <summary>
    /// Представляет собой вызывающий плагин
    /// </summary>
    public class RemotePlugin
    {
        /// <summary>
        /// Плагин, полученный из MEF
        /// </summary>
        [XmlIgnore]
        public dynamic Plugin { get; set; }

        /// <summary>
        /// Id плагина
        /// </summary>
        [XmlIgnore]
        public Guid Id { get; set; }

        /// <summary>
        /// Список вызвывющих плагинов
        /// </summary>
        [XmlIgnore]
        public static List<RemotePlugin> Plugins = new List<RemotePlugin>();

        /// <summary>
        /// Название плагина
        /// </summary>
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Параметры инициализации плагина
        /// </summary>
        [XmlElement("InitParameters")]
        public string InitParameters { get; set; }

        /// <summary>
        /// Пользователь, которому отправляются результаты работы, если данный плагин был вызван другим пользователем или из другого плагина
        /// </summary>
        [XmlElement("BaseUserToCallback")]
        public string BaseUserToCallback { get; set; }

        /// <summary>
        /// Отправить сообщение пользователю
        /// </summary>
        /// <param name="toUser">Какому пользователю отправить</param>
        /// <param name="message">Сообщение</param>
        /// <param name="method">Вызываемый метод</param>
        /// <param name="log">Логгер</param>
        public void SendMessage(string toUser, string message, string method, Logger log)
        {
            Task.Factory.StartNew(() => Send(toUser, message, method)).ContinueWith((t) =>
            {
                foreach (var innerException in t.Exception.InnerExceptions)
                {
                    log.Error("В плагине {0} произошла необрабатываемая ошибка: {1}. Трассировка вызовов:\r\n{2}", Name, innerException.Message, innerException.StackTrace);
                }
            },
                                                                                       TaskContinuationOptions.OnlyOnFaulted);
        }

        void Send(string toUser, string message, string method)
        {
            Type calledType = Plugin.GetType();

            MethodInfo mi = calledType.GetMethod(method);

            mi.Invoke(Plugin, new object[] { toUser, message });
        }
    }

    [XmlRoot("Parameters")]
    public class RemotePluginsLoader
    {
        [XmlArray("RemotePlugins")]
        [XmlArrayItem("RemotePlugin")]
        public List<RemotePlugin> Plugins = new List<RemotePlugin>();

        public static void SavePlugins()
        {
            using (TextWriter writer = new StreamWriter(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\remoteplugins.xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RemotePluginsLoader));
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;

                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);

                XmlWriter xmlWriter = XmlWriter.Create(writer, settings);

                RemotePluginsLoader plugin = new RemotePluginsLoader();

                plugin.Plugins = RemotePlugin.Plugins;

                serializer.Serialize(xmlWriter, plugin, namespaces);
            }
        }

        public static void LoadPlugins()
        {
            using (FileStream fileStream = new FileStream(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\remoteplugins.xml", FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RemotePluginsLoader));
                RemotePluginsLoader remotePlugins = (RemotePluginsLoader)serializer.Deserialize(fileStream);
                RemotePlugin.Plugins = remotePlugins.Plugins;
            }
        }
    }
}
