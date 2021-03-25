using CheckLogWorker.Schedule;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLogWorker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.FirstOrDefault() == nameof(Schedule))
            {
                var arguments = args.Skip(1).ToArray();

                await Scheduler.RunAsync(arguments);
            }
            else
            {
                await Executor.RunAsync(args, CancellationToken.None);
            }
        }
    }
}
