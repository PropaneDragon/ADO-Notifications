using System.Timers;
using System;
using System.Threading.Tasks;

namespace ADO_Notifications.Listeners
{
    internal abstract class AbstractListener : IAbstractListener
    {
        private Timer _timer = null;

        public event EventHandler<Exception> OnError;

        public void StartListening(TimeSpan interval)
        {
            StopListening();

            _timer = new Timer(interval.TotalMilliseconds);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        public void StopListening()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= Timer_Elapsed;
                _timer = null;
            }
        }

        private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            _timer?.Stop();

            try
            {
                await TimeoutAsync();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
            finally
            {
                _timer?.Start();
            }
        }

        protected abstract Task TimeoutAsync();
    }
}
