namespace MyTimeScheduling
{
    public class Movie
    {
        public TimeSlot DesiredInitialSlot { get; set; }
        private Queue<VideoItem> Segments { get; set; } = new Queue<VideoItem>();
        public Genere Genere { get; set; }
        public string Tittle { get; set; } 

        public void AddSegment(VideoItem segment)
        {
            segment.Name =  "["+Tittle+"] Segment #:" + (Segments.Count + 1).ToString();
            Segments.Enqueue(segment);  
        }

        public bool HasMoreSegments()
        {
            return Segments.Count > 0;
        }

        public VideoItem GetNextSegment()
        {
            if(Segments.Count > 0)
                return Segments.Dequeue();
            return null;
        }
    }
}