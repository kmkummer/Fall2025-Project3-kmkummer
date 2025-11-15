using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_kmkummer.Models
{
    public class Actor

    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = default!;

        public string Gender { get; set; } = default!;

        public string Link { get; set; } = default!;

        public int Age { get; set; }

        public byte[]? Photo { get; set; }

        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
