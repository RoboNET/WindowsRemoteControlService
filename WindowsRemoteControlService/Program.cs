using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WindowsRemoteControlService
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main()
        {
         /*   PluginTaskSystem pluginTaskSystem = new PluginTaskSystem();
            pluginTaskSystem.Start();

           string s= Console.ReadLine();
            */
            
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new WindowsRemoteControlService() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
