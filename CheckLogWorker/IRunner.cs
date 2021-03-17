using System.Threading.Tasks;

namespace CheckLogWorker
{
    public interface IRunner
    {
        Task RunAsync(Logger logger);
    }
}
