using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocket4Net;

namespace WebSocketWinform
{
    public partial class Form1 : Form
    {
        static string url = "ws://localhost:64617/chat?id=";
        bool isHand = false;
        WebSocket ws = null;
        public Form1()
        {
            InitializeComponent();

//            ws.OnMessage += (sender, e) =>
//            {

//                this.Invoke((Action)(() =>
//                {
//                    string s = "Receive Message: " + e.Data;
//                    ShowMsg(s);
//                }
//   ));
//            };
//            ws.OnOpen += (sender, e) =>
//            {
//                this.Invoke((Action)(() =>
//                {
//                    string s = "Connect Opened ";
//                    ShowMsg(s);
//                }
//               ));



//            };
//            ws.OnError += (sender, e) =>
//            {
//                this.Invoke((Action)(() =>
//                {
//                    string s = "Error Happened: " + e.Message;
//                    ShowMsg(s);
//                }
//));

//            };
//            ws.OnClose += (sender, e) =>
//            {
//                this.Invoke((Action)(() =>
//                {
//                    string s = "Connect Closed: " + e.Reason;
//                    ShowMsg(s);
//                }
//                    ));
//            };


        }

        private void ShowMsg(string s)
        {
            Console.WriteLine(s);
            //toolStripStatusLabel1.Text = s;

            textBox2.Text = s;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (ws == null || ws.State != WebSocketState.Open)
                {
                    isHand = false;
                    string connId = GetConnectionId();
                    ws = new WebSocket(url + connId, "protocol1", WebSocketVersion.None);
                    ws.Open();
                    Console.WriteLine("Connect success");
                    ws.MessageReceived += Ws_MessageReceived;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void Ws_MessageReceived(Object sender, MessageReceivedEventArgs e)
        {
            if (e.Message != null) 
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(Encoding.Default.GetString(RemoveSeparator(Encoding.Default.GetBytes(e.Message))));
                if (jo != null && jo.Count>0)
                {
                    if (jo["type"].ToString() == "1"&&jo["target"].ToString()== "broadcastMessage")
                    {
                        Console.WriteLine(jo["arguments"].ToString()) ;
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!isHand)
                {
                    IList<ArraySegment<byte>> segments = new List<ArraySegment<byte>>(
                        new ArraySegment<byte>[]
                        {
                            new ArraySegment<byte>(AddSeparator(Encoding.UTF8.GetBytes(@"{""protocol"":""json"", ""version"":1}")))
                        }
                        );
                    ws.Send(segments);//发送握手包
                    Console.WriteLine("Send success");
                    isHand = true;
                }
                Send();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static byte[] AddSeparator(byte[] data)
        {
            List<byte> t = new List<byte>(data) { 0x1e };//0x1e record separator
            return t.ToArray();
        }
        private static byte[] RemoveSeparator(byte[] data)
        {
            List<byte> t = new List<byte>(data);
            t.Remove(0x1e);
            return t.ToArray();
        }

        private void Send()
        {
            var bytes = Encoding.UTF8.GetBytes(@"{
                           ""type"": 1,
                            ""invocationId"":""1"",
                            ""streamIds"":[],
                           ""target"": ""Send"",
                           ""arguments"": [
                                ""Test"",""hello world""
                           ]}"")");
            IList<ArraySegment<byte>> segments = new List<ArraySegment<byte>>(
                    new ArraySegment<byte>[]
                    {
                            new ArraySegment<byte>(AddSeparator(bytes))
                    }
                    );
            ws.Send(segments);
            var buffer = new ArraySegment<byte>(new byte[1024]);

        }

        private string GetConnectionId()
        {
            string strURL = "http://localhost:64617/chat/negotiate";
            System.Net.HttpWebRequest request;
            request = (System.Net.HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            byte[] payload;
            System.IO.Stream writer = request.GetRequestStream();
            writer.Close();
            System.Net.HttpWebResponse response;
            response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
            JObject jo = (JObject)JsonConvert.DeserializeObject(myreader.ReadToEnd());
            return jo["connectionId"].ToString();
        }

    }
}
