namespace Fall2025_Project3_kmkummer.Models.ViewModels
{
    public class MovieDetailsViewModel
    {
        public Movie Movie { get; set; }
        public List<string> AIReviews { get; set; } = default!;
        public List<double> ReviewSentiments { get; set; } = default!;
        public double AverageSentiment { get; set; }
        public List<Actor> Actors { get; set; } = new List<Actor>();
    }
}
