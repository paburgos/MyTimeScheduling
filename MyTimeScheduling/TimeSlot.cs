
namespace MyTimeScheduling
{
    public class TimeSlot
    {
        public TimeSlot(TimeSpan startTime, TimeSpan endTime)
        {
            this.StartTime = startTime;
            this.EndTime = endTime;
        }

        public int Id { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}