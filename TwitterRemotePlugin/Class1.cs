using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Timers;
using Twitterizer;
using WindowsRemoteControlService;

namespace TwitterRemotePlugin
{
    [Export(typeof(IRemotePlugin))]
    [ExportMetadata("Name", "TwitterRemotePlugin")]
    public class Class1 : IRemotePlugin
    {
        OAuthTokens tokens = new OAuthTokens();
        private Guid guid;
        public void Initialize(Guid guid, string parameters)
        {
            this.guid = guid;
            string[] pr=parameters.Split(new string[]{"{!}"},StringSplitOptions.None);
            tokens.ConsumerKey = pr[0]; 
            tokens.ConsumerSecret = pr[1];
            tokens.AccessToken = pr[2];
            tokens.AccessTokenSecret =pr[3];

            Timer timer = new Timer(Int32.Parse(parameters));
            timer.Elapsed += OnTick;
            timer.Start();
        }

        private void OnTick(object sender, ElapsedEventArgs e)
        {
            TwitterDirectMessageCollection collection = TwitterDirectMessage.DirectMessages(tokens).ResponseObject;
            foreach (TwitterDirectMessage message in collection)
            {
                string[] pm = message.Text.Split(new string[] {"{!}"}, StringSplitOptions.None);

                if(pm.Count()<3)
                    return;
                OnMessageReceive(guid, message.SenderId.ToString(), pm[0], pm[1], pm[2]);
                message.Delete(tokens, new OptionalProperties());
            }
        }

        public void SendMessage(string toUser, string message)
        {
            TwitterDirectMessage mess = new TwitterDirectMessage();
            TwitterDirectMessage.Send(tokens,decimal.Parse(toUser), message);
        }

        public event OnMessageReceive OnMessageReceive;
    }
}
