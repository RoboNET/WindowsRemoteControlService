using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace WindowsRemoteControlService
{
    public partial class WindowsRemoteControlService : ServiceBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public WindowsRemoteControlService()
        {
            InitializeComponent();
           // StartPluginSystem();
        }

        protected override void OnStart(string[] args)
        {
            //Запускаем службу плагинов
            StartPluginSystem();
        }

        void StartPluginSystem()
        {
            try
            {
                PluginTaskSystem pluginTaskSystem = new PluginTaskSystem();
                pluginTaskSystem.Start();
            }
            catch (Exception ex)
            {
                logger.Fatal("В системе плагинов произошла критическая ошибка. Проверьте, верны ли записи в файлах конфигурации {0} . Система перезапускается",ex.Message);
                StartPluginSystem();
            }
        }

        protected override void OnStop()
        {
        }
    }
}
