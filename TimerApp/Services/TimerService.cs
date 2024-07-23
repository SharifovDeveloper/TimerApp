using Stl.CommandR;
using Stl.CommandR.Configuration;
using Stl.Fusion;
using System.Reactive;

namespace TimerApp.Services
{
    public record SetTimerDurationCommand(int Timer) : ICommand<Unit>;

    public class TimerService : IComputeService, IHostedService
    {
        public int LoadState = 0;
        public int TimerDuration;

        private List<Data.Timer> timerData = new List<Data.Timer>();
        private int remainingTime;
        private int elapsedTime;
        private DateTime startTime;
        private DateTime endTime;
        private bool timerIsRunning = false;
        private int elapsedPercentage;
       
        private bool timerIsPaused = false;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        [CommandHandler]
        public virtual async Task StartTimerAsync(SetTimerDurationCommand command, CancellationToken cancellationToken = default)
        {
            if (Computed.IsInvalidating())
            {
                _ = GetTimerInfo();
                return;
            }

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                TimerDuration = command.Timer;
                remainingTime = TimerDuration;
                elapsedTime = 0;
                timerIsRunning = true;
                LoadState = 1;
                startTime = DateTime.Now;
                endTime = startTime.AddSeconds(TimerDuration);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        [ComputeMethod]
        public virtual async Task<(int TimerDuration, DateTime StartTime, DateTime EndTime)> GetTimerInfo()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                return (TimerDuration, startTime, endTime);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        [ComputeMethod]
        public virtual Task<List<Data.Timer>> GetData()
        {
            return Task.FromResult(timerData);
        }

        [ComputeMethod(AutoInvalidationDelay = 1)]
        public virtual async Task<(bool IsRunning, int RemainingTime, int ElapsedTime, int ElapsedPercentage, bool IsPaused)> UpdateTimerStateAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (timerIsRunning)
                {
                    if (remainingTime <= 0)
                    {
                        Console.WriteLine("Timer has finished.");
                        ResetTimer();

                        if (LoadState == 2)
                        {
                            await SaveTimerDataAsync(startTime, endTime);
                        }
                    }
                    else
                    {
                        elapsedTime++;
                        remainingTime--; // Decrement remainingTime
                        Console.WriteLine($"Remaining Time: {remainingTime}");
                        elapsedPercentage = TimerDuration > 0 ? (elapsedTime * 100) / TimerDuration : 0;
                    }
                }

                return (timerIsRunning, remainingTime, elapsedTime, elapsedPercentage, timerIsPaused);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SaveTimerDataAsync(DateTime startTime, DateTime endTime)
        {
            //await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                var timerEntry = new Data.Timer
                {
                    Id = Guid.NewGuid(),
                    StartTime = startTime,
                    EndTime = endTime
                };

                timerData.Add(timerEntry);
                LoadState = 0;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public bool TogglePause()
        {
            if (timerIsPaused)
            {
                timerIsRunning = true;
                timerIsPaused = false;
            }
            else
            {
                timerIsRunning = false;
                timerIsPaused = true;
            }
            return timerIsPaused;
        }

        private void ResetTimer()
        {
            timerIsRunning = false;
            remainingTime = 0;
            elapsedTime = 0;
            elapsedPercentage = 0;
            LoadState = (LoadState == 1) ? 2 : 0;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Console.Out.WriteLineAsync("Started");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Console.Out.WriteLineAsync("Timer service stopping...");
            await Task.CompletedTask;
        }
    }
}
