using Seagull.Doctors.Helper.Scheduling.Demo;

namespace Seagull.Doctors.Helper.SignalR.Hub
{
    public interface ISeagullHub
    {
        void CheckNotify();
        void NotifyClients();
      
        void QuoteOfTheDay(QuoteOfTheDay _quoteOfTheDay);
    }
}
