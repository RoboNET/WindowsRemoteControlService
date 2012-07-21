using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Vasan;
using WindowsRemoteControlService;
using xNet.Collections;

namespace VKRemotePlugin
{

    [Export(typeof(IRemotePlugin))]
    [ExportMetadata("Name", "VKRemotePlugin")]
    public class Class1 : IRemotePlugin
    {
        Timer timer = new Timer(10000);

        VkApi vkApi = new VkApi();

        private Guid guid;

        public void Initialize(Guid guid, string parameters)
        {
            this.guid = guid;
            string[] param = parameters.Split(new string[] { ":" }, StringSplitOptions.None);
            vkApi.Authorization(2963021, param[0],
                    param[1], AccessRights.Messages);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        public void SendMessage(string toUser, string message)
        {
            vkApi.MessagesSend(uid: Int32.Parse(toUser), message: message);
           // throw new NotImplementedException();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var messages = vkApi.MessagesGet(filters: new int[] { 1 });

            if (!messages.IsEmpty())
            {
                foreach (var message in messages)
                {
                    //string answer = GetAnswer(message.Body);

                    string[] ms = message.Body.Split(new string[] { "{!}" }, StringSplitOptions.None);

                    vkApi.MessagesMarkAsRead(message.Id);
                    OnMessageReceive(guid, message.UserId.ToString(), ms[0], ms[1], ms[2]);
                   // MessageReceive(ms[0], "VkRemoteModule", message.UserId.ToString(), ms[1]);
                   //  vkApi.MessagesSend(uid: message.UserId, message: answer);

                }
            }

        }

        public event OnMessageReceive OnMessageReceive;
    }
}
