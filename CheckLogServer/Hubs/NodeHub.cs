using CheckLogServer.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CheckLogServer.Hubs
{
    public class NodeHub : Hub
    {
        public async Task NodeAsync(Node node) => await NodeAsync(node, Clients.All);
        public static async Task NodeAsync(Node node, IClientProxy clients) => await clients.SendAsync(nameof(NodeAsync), node);
    }
}
