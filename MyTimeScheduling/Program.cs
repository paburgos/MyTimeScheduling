using MyTimeScheduling;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

//List<string> allLinesText = File.ReadAllLines("input.csv").ToList();

Queue<Movie> movies = new Queue<Movie>();
List<VideoItem> AdSlides = new List<VideoItem>();
List<VideoItem> molinets = new List<VideoItem>();
List<VideoItem> interstitials = new List<VideoItem>();

//LoadFromFile(allLinesText, ref movies, ref AdSlides, ref molinets, ref interstitials);
Dummy(ref movies, ref AdSlides, ref molinets, ref interstitials);

LineUp LineUp = new LineUp(movies, AdSlides, molinets, interstitials);

var stack = new Stack<VideoItem>();
Movie currentmovie = null;

do
{
    stack = LineUp.ProgramDay(stack, TimeSpan.FromSeconds(10), ref currentmovie, null, TimeSpan.FromSeconds(129));
}
while (movies.Count > 0);

Print(stack);

static void LoadFromFile(List<string> allLinesText, ref Queue<Movie> movies, ref List<VideoItem> AdSlides, ref List<VideoItem> molinets, ref List<VideoItem> interstitials)
{
    string currentMovie = string.Empty;
    foreach (var videoItem in allLinesText)
    {
        var videoItemStructure = videoItem.Split(',');
        //0 Type
        //1 Tittle
        //2 Duration
        //3 Genere
        //4 Movie
        VideoItem item = null;
        switch (videoItemStructure[0])
        {
            case "Segment":
                CheckMovie(ref currentMovie, ref movies, videoItemStructure);
                break;
            case "AdSlide":
                item = new VideoItem(videoItemStructure[1], VideTypeoItem.AdSlide, TimeSpan.Parse(videoItemStructure[2]),  FindGenere(videoItemStructure[3]));
                AdSlides.Add(item);
                break;
            case "Molinet":
                item = new VideoItem(videoItemStructure[1], VideTypeoItem.Molinet, TimeSpan.Parse(videoItemStructure[2]), FindGenere(videoItemStructure[3]));
                molinets.Add(item);
                break;
            case "Intertisial":
                item = new VideoItem(videoItemStructure[1], VideTypeoItem.Intertisial, TimeSpan.Parse(videoItemStructure[2]), FindGenere(videoItemStructure[3]));
                interstitials.Add(item);
                break;
        }
    }
}

static void CheckMovie(ref string currentMovie, ref Queue<Movie> movies, string[] videoItemStructure)
{
    var selectedMovie = videoItemStructure[4];
    bool addmovie = false;
    Movie oneMovie = new Movie();
    if (selectedMovie != currentMovie)
    {
        addmovie = true;
        oneMovie = new Movie
        {
            Tittle = videoItemStructure[4],
            Genere = FindGenere(videoItemStructure[3])
        };
        currentMovie = videoItemStructure[4];
    }
    //find movie
    if (!addmovie)
    {
        oneMovie = movies.Where(x => x.Tittle == selectedMovie).FirstOrDefault();
    }
    //add Segment
    var oneMovieSegments_1 = new VideoItem(currentMovie, VideTypeoItem.Segment, TimeSpan.Parse(videoItemStructure[2]), FindGenere(videoItemStructure[3]));
    oneMovie.AddSegment(oneMovieSegments_1);

    if(addmovie)
        movies.Enqueue(oneMovie);
}

static Genere FindGenere(string v)
{
    switch (v)
    {
        case "Thriller": return Genere.Thriller;
        case "Comedy": return Genere.Comedy;
        case "Romance": return Genere.Romance;
        case "Documentary": return Genere.Documentary;
    }
    return Genere.Romance;
}

static void Dummy(ref Queue<Movie> movies, ref List<VideoItem> AdSlides, ref List<VideoItem> molinets, ref List<VideoItem> interstitials)
{   

    #region Movies
    Random randomMachine = new Random();
    for (int i = 0; i < 10; i++)
    {
        var oneMovie = new Movie
        {
            Tittle = "Movie " + i,
            Genere = Genere.Romance
        };

        for (int j = 0; j < 11; j++)
        {
            var oneMovieSegments_1 = new VideoItem(j.ToString(), VideTypeoItem.Segment, TimeSpan.FromMinutes(randomMachine.Next(5, 20)), Genere.Romance);
            oneMovie.AddSegment(oneMovieSegments_1);
        }

        movies.Enqueue(oneMovie);
    }
    #endregion

    for (int i = 0; i < 100; i++)
    {
        //var videoItem = new VideoItem("AdSlide "+i.ToString(), VideTypeoItem.AdSlide, TimeSpan.FromSeconds(randomMachine.Next(2, 4)), Genere.Romance);
        var videoItem = new VideoItem("AdSlide " + i.ToString(), VideTypeoItem.AdSlide, TimeSpan.FromSeconds(randomMachine.Next(30, 60)), Genere.Romance);
        AdSlides.Add(videoItem);
    }

    for (int i = 0; i < 100; i++)
    {
        var videoItem = new VideoItem("Molinet " + i.ToString(), VideTypeoItem.Molinet, TimeSpan.FromSeconds(randomMachine.Next(4, 34)), Genere.Romance);
        molinets.Add(videoItem);
    }

    for (int i = 0; i < 100; i++)
    {
        var videoItem = new VideoItem("interstetial " + i.ToString(), VideTypeoItem.Intertisial, TimeSpan.FromSeconds(randomMachine.Next(30, 35)), Genere.Romance);
        interstitials.Add(videoItem);
    }
}

static void Print(Stack<VideoItem> stack)
{
    foreach (var videoItem in stack)
    {
        Console.WriteLine($"Name:{videoItem.Name} Type:{Enum.GetName(typeof(VideTypeoItem), videoItem.Type)} Begining:{videoItem.SelectedTimeSlot.StartTime} Duration:{videoItem.Duration}");
    }
    Console.Read();
}