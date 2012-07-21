using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace WindowsRemoteControlService
{
    class PluginTaskSystem
    {
        [ImportMany(typeof(IActionPlugin))]
        private IEnumerable<Lazy<IActionPlugin, IModuleMetadata>> actionModules;

        [ImportMany(typeof(IRemotePlugin))]
        private IEnumerable<Lazy<IRemotePlugin, IModuleMetadata>> remoteModules;

        private CompositionContainer container;

        Dictionary<Guid, ActionPluginTask> actionPluginTasks = new Dictionary<Guid, ActionPluginTask>();
       // Dictionary<Guid, RemotePluginTask> remotePluginTasks = new Dictionary<Guid, RemotePluginTask>();

        private Logger logger = LogManager.GetCurrentClassLogger();

        public void Start()
        {
            //Загружаем плагины с помощью MEF
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(
                new DirectoryCatalog(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\Plugins"));

            container = new CompositionContainer(catalog);
            try
            {
                container.ComposeParts(this);
            }
            catch (CompositionException ex)
            {
            }

            //Загружаем настройки выполняющих плагинов
            ActionPluginsLoader.LoadPlugins();
           
            foreach (ActionPlugin actionPlugin in ActionPlugin.Plugins)
            {
                var plugin = actionModules.SingleOrDefault(pl => pl.Metadata.Name == actionPlugin.Name);

                if (plugin != null)
                {
                    actionPlugin.Plugin = plugin.Value;
                    actionPlugin.Id = Guid.NewGuid();
                    AttachEventHandler(actionPlugin.Plugin, "OnActionCallback", "OnAction");
                    try
                    {
                        actionPlugin.Plugin.Initialize(actionPlugin.Id, actionPlugin.InitParameters);
                    }
                    catch (Exception ex)
                    {


                    }
                }
            }

            //Загружаем настройки вызывающих плагинов
            RemotePluginsLoader.LoadPlugins();
               
            foreach (RemotePlugin remotePlugin in RemotePlugin.Plugins)
            {
                var plugin = remoteModules.SingleOrDefault(pl => pl.Metadata.Name == remotePlugin.Name);

                if (plugin != null)
                {
                    remotePlugin.Plugin = plugin.Value;
                    remotePlugin.Id = Guid.NewGuid();
                    AttachEventHandler(remotePlugin.Plugin, "OnMessageReceive", "OnMessage");
                    try
                    {
                        remotePlugin.Plugin.Initialize(remotePlugin.Id, remotePlugin.InitParameters);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                    }
                }
            }
        }

        private void AttachEventHandler(dynamic request, string eventName, string eventHandlerMethodName)
        {
            //Подписываемся на события плагинов
            EventInfo eventInfo = request.GetType().GetEvent(eventName);
            var methodInfo = GetType().GetMethod(eventHandlerMethodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            var eventHandler = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);
            eventInfo.AddEventHandler(request, eventHandler);
        }

        private void OnAction(Guid id, string result, bool canClose)
        {
            //Действие плагина завершилось
            ActionPluginTask task = actionPluginTasks[id];

            //Рассылаем всем пользователям сообщение с результатом
            foreach (var plugin in task.CallbackPlugins)
            {
                try
                {
                    plugin.Plugin.SendMessage(plugin.User, result, "SendMessage", logger);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }
            }

            //Если плагин больше не будет отправлять результаты по данному заданию, удаляем его
            if (canClose)
                actionPluginTasks.Remove(id);
        }

        private void OnMessage(Guid guid, string user, string plugin, string method, string parameters)
        {
            //Мы получили сообщение

            //Получаем плагины
            ActionPlugin plug = ActionPlugin.Plugins.SingleOrDefault(pl => pl.Name == plugin);
            if (plug == null)
                return;

            RemotePlugin curPlugin = RemotePlugin.Plugins.SingleOrDefault(pl => pl.Id == guid);

            if (curPlugin == null)
                return;

            //Проверяем доступ плагина
            PluginAccess pluginAccess = plug.PluginAccesses.SingleOrDefault(pl => pl.Plugin == curPlugin.Name);

          

            if (plug.PluginAccesses.Count != 0)
            {
             if (pluginAccess.Plugin==null)
                {
                return;
                }
                if (!pluginAccess.Users.Contains(user) && pluginAccess.Users.Count != 0)
                {
                    return;
                }
            }
            Guid guid1 = Guid.NewGuid();

            List<CallbackPlugin> cPlugins = new List<CallbackPlugin>();

            //Добавляем текущий плагин в список плагинов, которым нужно будет отправить результат

            cPlugins.Add(new CallbackPlugin() { Plugin = curPlugin, User = user });

            //Добавляем плагины, заданные в конфиге
            foreach (var callbackPlugin in plug.CallbackPlugins)
            {
                if (!(callbackPlugin == curPlugin.Name && user == curPlugin.BaseUserToCallback))
                {
                    RemotePlugin cPlugin = RemotePlugin.Plugins.SingleOrDefault(pl => pl.Name == callbackPlugin);
                    if (cPlugin != null)
                        cPlugins.Add(new CallbackPlugin() { Plugin = cPlugin, User = cPlugin.BaseUserToCallback });
                }
            }

            //Создаём новое задание и запускаем его
            actionPluginTasks.Add(guid1, new ActionPluginTask()
            {
                Id = guid1,
                CallbackPlugins = cPlugins,
                ActionPlugin = plug,
                Method = method,
                Parameters = parameters
            });

            try
            {
                plug.CallMethod(guid1, method, parameters, logger);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        public struct ActionPluginTask
        {
            public Guid Id { get; set; }
            public List<CallbackPlugin> CallbackPlugins { get; set; }
            public ActionPlugin ActionPlugin { get; set; }
            public string Method { get; set; }
            public string Parameters { get; set; }
        }

        public struct RemotePluginTask
        {
            public Guid Id { get; set; }
            public RemotePlugin RemotePlugin { get; set; }
            public List<string> ToUser { get; set; }
        }

        public struct CallbackPlugin
        {
            public RemotePlugin Plugin { get; set; }
            public string User { get; set; }
        }
    }
}