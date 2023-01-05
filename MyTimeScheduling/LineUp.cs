using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTimeScheduling
{
    public class LineUp
    {
        private Random randomMachine = new Random();

        private TimeSpan accumulatedDuration = TimeSpan.Zero;
        public Queue<Movie> Movies { get; set; }
        public List<VideoItem> AdSlides { get; set; }
        public List<VideoItem> Molinets { get; set; }
        public List<VideoItem> Interstitials { get; set; }
        public List<VideoItem> BigPool { get; set; } = new List<VideoItem>();

        public LineUp(Queue<Movie> movies, List<VideoItem> adSlides, List<VideoItem> molinets, List<VideoItem> intertisials)
        {
            this.Movies = movies;
            this.AdSlides = adSlides;
            this.Molinets = molinets;
            this.Interstitials = intertisials;

            for (int i = 0; i < 83; i++)
            {
                BigPool = BigPool.Union(adSlides).ToList();
            }
            for (int i = 0; i < 17; i++)
            {
                BigPool = BigPool.Union(Interstitials).ToList();
            }
        }

        //si el segmento es de mas de 9.30min debe tener 2min de adSlide
        //        //en la hora deben haber 8min de addslade y 2min de interstitial
        //        //pero si no hay interstitial no se pone la prioirdad es el addslade
        //        //la pelicula debe comenzar con un mullinet o un mullinet y un stationid juntos
        //        //las peliculas empiezan en multiples de 5 o 0 en la hora

        //        //un video item pude cruzar entre timeslots
        public Stack<VideoItem> ProgramDay(Stack<VideoItem> videoItemsScheduled, TimeSpan colita, ref Movie currentMovie, VideoItem currentSegment, TimeSpan Offset)
        {
            if (videoItemsScheduled.Count <= 0)
            {
                videoItemsScheduled = new Stack<VideoItem>();
                if(Offset != TimeSpan.Zero)
                {
                    videoItemsScheduled.Push(new VideoItem("OffSet ", VideTypeoItem.OffSet, Offset, Genere.Thriller));
                    accumulatedDuration += Offset;
                }
            }

            if ((TimeSpan.FromDays(1) - accumulatedDuration) < colita)
            {
                return videoItemsScheduled;
            }
            else
            {
                VideoItem lastVideoItemAdded = null;
                bool hasValidVideo = videoItemsScheduled.TryPeek(out lastVideoItemAdded);
                if (hasValidVideo && lastVideoItemAdded.Type != VideTypeoItem.OffSet)
                {
                    if (lastVideoItemAdded.Type == VideTypeoItem.Segment)
                    {
                        List<VideoItem> twoMinutesOfAddSlides = new List<VideoItem>();
                        var remainingDuration = TimeSpan.Zero;
                        bool success = false;
                        do
                        {
                            success = GetRandomAdSlideOrInterstitial(TimeSpan.FromMinutes(2.0), ref remainingDuration, ref twoMinutesOfAddSlides, TimeSpan.Zero, false);
                        }
                        while (!success);

                        foreach (var item in twoMinutesOfAddSlides)
                        {
                            AddVideoItemToSchedule(ref videoItemsScheduled, item);
                        }
                    }
                    else
                    {
                        if (currentMovie.HasMoreSegments())
                        {
                            currentSegment = currentMovie.GetNextSegment();
                            AddVideoItemToSchedule(ref videoItemsScheduled, currentSegment);
                        }
                        else
                        {
                            if (Movies.Count > 0)
                                AddNewMovie(ref videoItemsScheduled, out currentMovie, out currentSegment);
                            return videoItemsScheduled;
                        }
                    }
                }
                else
                {
                    if (Movies.Count > 0)
                        AddNewMovie(ref videoItemsScheduled, out currentMovie, out currentSegment);
                    return videoItemsScheduled;
                }
            }
            return ProgramDay(videoItemsScheduled, colita, ref currentMovie, currentSegment, Offset);
        }

        private void AddNewMovie(ref Stack<VideoItem> videoItemsScheduled, out Movie currentMovie, out VideoItem currentSegment)
        {
            currentMovie = Movies.Dequeue();
            Console.WriteLine($"Adding new Movie {currentMovie.Tittle}");
            currentSegment = currentMovie.GetNextSegment();
            AddBeginingOfTheMovie(currentSegment, currentMovie, ref videoItemsScheduled);

            if (currentSegment.Duration < TimeSpan.FromMinutes(9.3))
            {
                //si el segmento es menor a 9.3s se debe unir con el anterior
                currentSegment = currentMovie.GetNextSegment();
                AddVideoItemToSchedule(ref videoItemsScheduled, currentSegment);
                Console.WriteLine("Segments joined");
            }
        }

        private bool GetRandomAdSlideOrInterstitial(TimeSpan durationInMinutes, ref TimeSpan remainingduration, ref List<VideoItem> possibleVideoItems, TimeSpan accumulatedLocalDuration, bool allowTolerance)
        {
            TimeSpan tolerance = TimeSpan.FromSeconds(15);

            if(allowTolerance)
                tolerance = TimeSpan.FromSeconds(59);

            var currentDurationOfPoosibleItems = accumulatedLocalDuration;
            if (possibleVideoItems == null || possibleVideoItems.Count == 0)
            {
                remainingduration = durationInMinutes;
                possibleVideoItems = new List<VideoItem>();
                accumulatedLocalDuration = TimeSpan.Zero;
            }

            if (!allowTolerance && currentDurationOfPoosibleItems > durationInMinutes - tolerance && currentDurationOfPoosibleItems < durationInMinutes + tolerance)
            {
                return true;
            }
            else if (allowTolerance && currentDurationOfPoosibleItems >= durationInMinutes && currentDurationOfPoosibleItems < durationInMinutes + tolerance)
            {
                return true;
            }
            else if (currentDurationOfPoosibleItems > durationInMinutes + tolerance)
            {
                return false;
            }
            else
            {
                //filter by remaining duration

                var filter = remainingduration + tolerance;               

                var selectedFromPool = BigPool.Where(it => it.Duration <= filter);

                if (selectedFromPool.Count() > 0)
                {
                    int videoItemsAvailableElements = selectedFromPool.Count() - 1;

                    int selectedVideoIndex = randomMachine.Next(0, videoItemsAvailableElements);
                    var selectedVideoItem = selectedFromPool.ElementAt(selectedVideoIndex);
                    remainingduration = remainingduration - selectedVideoItem.Duration;
                    accumulatedLocalDuration += selectedVideoItem.Duration;
                    possibleVideoItems.Add(selectedVideoItem);
                }
                else if(filter == TimeSpan.Zero)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return GetRandomAdSlideOrInterstitial(durationInMinutes, ref remainingduration, ref possibleVideoItems, accumulatedLocalDuration, allowTolerance);
        }

        private void AddBeginingOfTheMovie(VideoItem currentSegment, Movie currentMovie, ref Stack<VideoItem> videoItemsScheduled)
        {
            //add mulinet
            VideoItem selectedMulinet = FindMulinet(currentMovie.Genere);
            var seeIfItCanStart = accumulatedDuration + selectedMulinet.Duration;

            if (!((seeIfItCanStart.Minutes == 5 || seeIfItCanStart.Minutes == 0) && seeIfItCanStart.Seconds == 0 && seeIfItCanStart.Milliseconds == 0))
            //if (!(seeIfItCanStart.Minutes == 5 || seeIfItCanStart.Minutes == 0))
            {
                int fix = seeIfItCanStart.Minutes;
                if (seeIfItCanStart.Minutes == 0)
                    fix = 1;
                double nextUpperMinutes = Math.Ceiling(fix / 5.0) * 5;
                TimeSpan limit = new TimeSpan(seeIfItCanStart.Hours, (int)nextUpperMinutes, 0);

                //find the difference of seeIfItCanStart yel proximo minuto 00 o 05                
                var holePatches = new List<VideoItem>();
                var remainingDuration = TimeSpan.Zero;
                bool success = false;
                do
                {
                    success = GetRandomAdSlideOrInterstitial(limit - seeIfItCanStart, ref remainingDuration, ref holePatches, TimeSpan.Zero, true);
                }
                while (!success);

                foreach (var videoItem in holePatches)
                {
                    AddVideoItemToSchedule(ref videoItemsScheduled, videoItem);
                }
            }

            AddVideoItemToSchedule(ref videoItemsScheduled, selectedMulinet);
            //add first segment of movie
            AddVideoItemToSchedule(ref videoItemsScheduled, currentSegment);
        }

        private void AddVideoItemToSchedule(ref Stack<VideoItem> videoItemsScheduled, VideoItem videoItemToAdd)
        {
            var newVideoItemToAdd = videoItemToAdd.Clone();
            var selectedTimeSlot = newVideoItemToAdd.ChangeTimeSlot(accumulatedDuration);
            Console.WriteLine($"{accumulatedDuration.ToString()} | {selectedTimeSlot.StartTime} [|<{Enum.GetName(typeof(VideTypeoItem), newVideoItemToAdd.Type)}>|] Tittle: {newVideoItemToAdd.Name} ::: {newVideoItemToAdd.Duration.ToString()}");
            accumulatedDuration += newVideoItemToAdd.Duration;
            videoItemsScheduled.Push(newVideoItemToAdd);
        }

        private VideoItem FindMulinet(Genere genere)
        {
            var validMulinet = Molinets.Where(mo => mo.Genere == genere);
            int numberOfValidMolinets = validMulinet.Count();
            if (numberOfValidMolinets > 1)
            {
                int selectedIndex = randomMachine.Next(0, numberOfValidMolinets - 1);
                return validMulinet.ElementAt(selectedIndex);
            }
            return validMulinet.FirstOrDefault();
        }

        public TimeSpan GetCurrentDuration(IEnumerable<VideoItem> videoItemsScheduled)
        {
            TimeSpan timeSpent = TimeSpan.Zero;
            foreach (var item in videoItemsScheduled)
            {
                timeSpent += item.Duration;
            }
            return timeSpent;
        }
    }
}

