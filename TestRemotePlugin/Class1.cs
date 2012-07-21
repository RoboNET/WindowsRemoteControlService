using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using WindowsRemoteControlService;


namespace TestRemotePlugin
{

    [Export(typeof(IRemotePlugin))]
    [ExportMetadata("Name", "TestRemotePlugin")]
    public class Class1 : IRemotePlugin
    {
        private Guid guid;

        public void Initialize(Guid guid, string parameters)
        {

        }

        public void SendMessage(string toUser, string message)
        {
            throw new NotImplementedException();
        }

        public event OnMessageReceive OnMessageReceive;
    }
}
