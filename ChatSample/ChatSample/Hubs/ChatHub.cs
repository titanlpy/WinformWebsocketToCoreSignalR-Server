using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace ChatSample.Hubs
{
    public class ChatHub : Hub
    {
        public async Task Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            await Clients.All.SendAsync("broadcastMessage", name, message);
        }

        public void Log(string arg)
        {
            Console.WriteLine("client send:" + arg);
        }
    }
}