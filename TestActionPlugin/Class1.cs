using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using WindowsRemoteControlService;

namespace TestActionPlugin
{
    [Export(typeof(IActionPlugin))]
    [ExportMetadata("Name", "System")]
    public class Class1 : IActionPlugin
    {
        Timer sTimer;

        public void Initialize(Guid guid, string parameters)
        {

        }

        public void Run(Guid guid, string parameters)
        {
            Process.Start(parameters);
            OnActionCallback(guid, "Процесс запущен", true);
        }

        public void GetPerformance(Guid guid, string parameters)
        {
            OnActionCallback(guid, GetPerformance(), true);
        }

        public void StartGetPerformance(Guid guid, string parameters)
        {
            int tick = 0;
            if (Int32.TryParse(parameters, out tick))
                sTimer = new Timer(DoTick, guid, 0, tick);
        }

        private void DoTick(object state)
        {
            OnActionCallback((Guid)state, GetPerformance(), false);
        }

        public void StopGetPerformance(Guid guid, string parameters)
        {
            sTimer.Dispose();
        }

        private string GetPerformance()
        {
            PerformanceCounter cpuCounter;
            PerformanceCounter ramCounter;
            cpuCounter = new PerformanceCounter();

            // Добавляем счетчик производительности ОЗУ
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total"; 

            float RAMFree = ramCounter.NextValue();
            float CPULoad = cpuCounter.NextValue();

            return String.Format("Загрузка процессора {0}, свободно RAM: {1}", CPULoad.ToString(), RAMFree.ToString());
        }

        public void GetFolderInfo(Guid guid, string parameters)
        {
            IEnumerable<string> infos = Directory.EnumerateFileSystemEntries(parameters);

            string s = "";

            foreach (string fileSystemInfo in infos)
            {
                s += fileSystemInfo + "\r\n";
            }

            OnActionCallback(guid, s, true);
        }

        public event OnActionCallback OnActionCallback;
    }
}
