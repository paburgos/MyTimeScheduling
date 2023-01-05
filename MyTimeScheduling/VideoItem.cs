namespace MyTimeScheduling
{
    public class VideoItem
    {
        public string Name { get; set; }
        public VideTypeoItem Type { get; set; }
        public TimeSpan Duration { get; set; }
        public Genere Genere { get; set; }
        public TimeSlot SelectedTimeSlot { get; private set; }

        public VideoItem(string name, VideTypeoItem type, TimeSpan duration, Genere genere)
        {
            Name = name;
            Type = type;
            Duration = duration;
            Genere = genere;
            SelectedTimeSlot = new TimeSlot(TimeSpan.Zero, (TimeSpan.Zero + Duration));
        }

        public TimeSlot ChangeTimeSlot(TimeSpan startTime) 
        {
            if (SelectedTimeSlot.StartTime == TimeSpan.Zero)
                this.SelectedTimeSlot = new TimeSlot(startTime, (startTime + Duration));
            else
                Console.WriteLine("alert!");
            return SelectedTimeSlot;
        }

        public VideoItem Clone()
        {
            var x = new VideoItem(this.Name,this.Type,this.Duration,this.Genere);           
            x.SelectedTimeSlot = this.SelectedTimeSlot;
            return x;
           
        }
    }

    public enum Genere
    {
        Thriller,
        Comedy,
        Romance,
        Documentary
    }

    public enum VideTypeoItem
    {
        Segment,
        AdSlide,
        Molinet,
        Intertisial,        
        OffSet,
    }
}