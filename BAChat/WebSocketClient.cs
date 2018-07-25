using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using WebSocket4Net;
using System.Diagnostics;

namespace BAChat
{
    class WebSocketClient
    {
        public static WebSocket WSClient = null;
        public static Task<bool> Connect(string host, bool secure = false, int port = 0)
        {
            string sPort = "";
            string scheme = "ws://";
            if (port != 0)
            {
                sPort = ":" + port.ToString();
            }
            if (secure)
            {
                scheme = "wss://";
            }
            WSClient = new WebSocket(scheme + host + sPort);
            WSClient.MessageReceived += WSClient_MessageReceived;
            WSClient.Closed += WSClient_Closed;
            return WSClient.OpenAsync();
        }

        private static void WSClient_Closed(object sender, EventArgs e)
        {
           
        }

        private static void WSClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            
        }

        public static void Login(string username_or_token, string password = "")
        {
            if (username_or_token.Length == 32)
            {
                WSClient.Send(username_or_token);
            }
            
        }
    }
}
