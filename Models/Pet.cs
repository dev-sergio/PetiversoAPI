namespace PetiversoAPI.Models
{
    public class Pet
    {
        public Guid PetId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Species { get; set; } = null!;
        public string? Breed { get; set; }
        public string? Color { get; set; }
        public string Size { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relação com fotos
        public ICollection<PetPhoto> Photos { get; set; } = [];
    }
}
