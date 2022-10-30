using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Common;

namespace MixchSitePlugin
{
    class MixchWebsocket : IMixchWebsocket
    {
        private Websocket _websocket;
        private readonly ILogger _logger;
        public event EventHandler<Packet> Received;

        public async Task ReceiveAsync(LiveUrlInfo liveUrlInfo, string userAgent, List<Cookie> cookies)
        {
            var origin = $"https://mixch.tv";

            _websocket = new Websocket();
            _websocket.Received += Websocket_Received;
            _websocket.Opened += Websocket_Opened;

            var host = "chat";
            var dir = "torte";
            if (liveUrlInfo.Environment != "torte")
            {
                host = "chat-dev";
                dir = "andagi";
            }
            var url = $"wss://{host}.mixch.tv/{dir}/room/{liveUrlInfo.LiveId}";
            Debug.WriteLine(url);
            await _websocket.ReceiveAsync(url, userAgent, origin);
            // 切断後処理
            _heartbeatTimer.Enabled = false;

        }

        private void Websocket_Opened(object sender, EventArgs e)
        {
            _heartbeatTimer.Enabled = true;
        }

        private void Websocket_Received(object sender, string e)
        {
            Packet packet = null;
            try
            {
                packet = JsonConvert.DeserializeObject<Packet>(e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            if (packet == null)
                return;
            Received?.Invoke(this, packet);
        }

        public async Task SendAsync(string s)
        {
            await _websocket.SendAsync(s);
        }
        System.Timers.Timer _heartbeatTimer = new System.Timers.Timer();
        public MixchWebsocket(ILogger logger)
        {
            _logger = logger;
        }

        public void Disconnect()
        {
            _websocket.Disconnect();
        }
    }
}
