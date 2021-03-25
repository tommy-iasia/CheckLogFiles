using CheckLogUtility.Logging;
using CheckLogUtility.Timing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLogWorker.Schedule
{
    public class Slot
    {
        public TimeExpression TimeExpression { get; set; }
        public void Next() => NextTime = TimeExpression.Next(NextTime);

        public DateTime NextTime { get; set; }
        public async Task WaitAsync()
        {
            while (true)
            {
                var now = DateTime.Now;
                if (now >= NextTime)
                {
                    return;
                }

                var thisSpan = NextTime - now;
                if (thisSpan.TotalSeconds > 5)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                else
                {
                    await Task.Delay(thisSpan);
                }
            }
        }

        public string[] Arguments { get; set; }
        public async Task RunAsync(Logger logger)
        {
            var startTime = DateTime.Now;
            await logger.InfoAsync($"Start worker with {string.Join(", ", Arguments)}");

            var cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(TimeSpan.FromMinutes(15));

            try
            {
                await Executor.RunAsync(Arguments, cancellationSource.Token);
            }
            catch (Exception e)
            {
                await logger.ErrorAsync($"Worker failed with {e}");
            }

            var endTime = DateTime.Now;
            await logger.InfoAsync($"Complete worker within {(endTime - startTime).TotalSeconds:#,##0}s");
        }
    }
}
