
namespace Quizer.Services.Util
{
    public class TimeService : ITimeService
    {
        public DateTime GetDateTimeNow()
        {
            return DateTime.Now;
        }
    }
}
