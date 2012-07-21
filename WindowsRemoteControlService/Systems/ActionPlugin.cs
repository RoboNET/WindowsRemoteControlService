using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using NLog;

namespace WindowsRemoteControlService
{

    /// <summary>
    /// Структура для сохранения информации о доступе к плагину
    /// </summary>
    public struct PluginAccess
    {
        [XmlElement("Plugin")]
        public string Plugin { get; set; }

        [XmlArray("Users")]
        [XmlArrayItem("User")]
        public List<string> Users { get; set; }
    }

    /// <summary>
    /// Представляет класс выполняющего плагина 
    /// </summary>
    public class ActionPlugin
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
        /// Список выполняющих плагинов
        /// </summary>
        [XmlIgnore]
        public static List<ActionPlugin> Plugins = new List<ActionPlugin>();

        /// <summary>
        /// Название плагина
        /// </summary>
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Список плагинов, которые могут вызывать данный плагин
        /// </summary>
        [XmlArray("PluginAccesses")]
        [XmlArrayItem("PluginAccess")]
        public List<PluginAccess> PluginAccesses { get; set; }

        /// <summary>
        /// Список плагинов, в которые будет отправляться результат работы плагина
        /// </summary>
        [XmlArray("CallbackPlugins")]
        [XmlArrayItem("CallbackPlugin")]
        public List<string> CallbackPlugins { get; set; }

        /// <summary>
        /// Параметры инициализации плагина
        /// </summary>
        [XmlElement("InitParameters")]
        public string InitParameters { get; set; }

        /// <summary>
        /// Вызвать метод плагина
        /// </summary>
        /// <param name="id">Id метода</param>
        /// <param name="method">Название метода</param>
        /// <param name="parameters">Параметры метода</param>
        /// <param name="log">Логгер</param>
        public void CallMethod(Guid id, string method, string parameters, Logger log)
        {
            Task.Factory.StartNew(() => DoAction(id, method, parameters)).ContinueWith((t) =>
            {
                foreach (var innerException in t.Exception.InnerExceptions)
                {
                    log.Error("В плагине {0} произошла необрабатываемая ошибка: {1}. Трассировка вызовов:\r\n{2}", Name, innerException.Message,innerException.StackTrace);
                }
            },
                                                                                       TaskContinuationOptions.OnlyOnFaulted);
        }

        private void DoAction(Guid id, string method, string parameters)
        {
            Type calledType = Plugin.GetType();

            MethodInfo mi = calledType.GetMethod(method);

            mi.Invoke(Plugin, new object[] { id, parameters });
        }
    }

    [XmlRoot("Parameters")]
    public class ActionPluginsLoader
    {
        [XmlArray("ActionPlugins")]
        [XmlArrayItem("ActionPlugin")]
        public List<ActionPlugin> Plugins = new List<ActionPlugin>();

        public static void SavePlugins()
        {
            using (TextWriter writer = new StreamWriter(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\actionplugins.xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ActionPluginsLoader));
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;

                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);

                XmlWriter xmlWriter = XmlWriter.Create(writer, settings);

                ActionPluginsLoader plugin = new ActionPluginsLoader();

                plugin.Plugins = ActionPlugin.Plugins;

                serializer.Serialize(xmlWriter, plugin, namespaces);
            }
        }

        public static void LoadPlugins()
        {
            using (FileStream fileStream = new FileStream(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\actionplugins.xml", FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ActionPluginsLoader));
                ActionPluginsLoader actionPlugins = (ActionPluginsLoader)serializer.Deserialize(fileStream);
                ActionPlugin.Plugins = actionPlugins.Plugins;
            }
        }
    }
}
