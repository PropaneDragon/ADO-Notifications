using System;

namespace ADO_Notifications.Listeners
{
    internal interface IAbstractListener
    {
        event EventHandler<Exception> OnError;

        void StartListening(TimeSpan interval);
        void StopListening();
    }
}