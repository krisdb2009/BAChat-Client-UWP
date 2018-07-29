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
        public static async Task<WebSocket> Connect(string host, bool secure = false, int port = 0)
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
            WebSocket ws = new WebSocket(scheme + host + sPort);
            WSClient = ws;
            await ws.OpenAsync();
            return ws;
        }
    }
}
