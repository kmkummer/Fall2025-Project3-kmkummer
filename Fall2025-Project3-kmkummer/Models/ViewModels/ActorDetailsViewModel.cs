namespace Fall2025_Project3_kmkummer.Models.ViewModels
{
    public class ActorDetailsViewModel
    {
        public Actor Actor { get; set; }
        public List<string> AITweets { get; set; } = default!;
        public List<double> TweetSentiments { get; set; } = default!;
        public double AverageSentiment { get; set; }

        public List<Movie> Movies { get; set; } = new List<Movie>();
    }
}
