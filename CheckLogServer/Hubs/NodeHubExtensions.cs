using CheckLogServer.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CheckLogServer.Hubs
{
    public static class NodeHubExtensions
    {
        public static async Task NodeAsync(this IHubContext<NodeHub> nodeHub, Node node) => await NodeHub.NodeAsync(node, nodeHub.Clients.All);
    }
}
