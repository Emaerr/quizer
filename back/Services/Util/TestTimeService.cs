
namespace Quizer.Services.Util
{
    public class TestTimeService : ITimeService
    {
        DateTime _time;
        
        public DateTime GetDateTimeNow()
        {
            return _time;
        }

        public void SetDateTime(DateTime time)
        {
            _time = time;
        }
    }
}
