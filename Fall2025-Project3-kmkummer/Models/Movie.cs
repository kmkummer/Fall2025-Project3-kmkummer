using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_kmkummer.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]

        public string Title { get; set; } = default!;

        public string IMDB { get; set; } = default!;

        public string Genre { get; set; } = default!;

        public int Year { get; set; }

        public byte[]? Poster { get; set; }

        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
