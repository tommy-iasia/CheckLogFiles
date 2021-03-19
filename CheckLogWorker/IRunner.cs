using System.Threading.Tasks;

namespace CheckLogWorker
{
    public interface IRunner
    {
        Task<bool> PrepareAsync(Logger logger);

        Task RunAsync(Logger logger);
    }
}
