namespace PetiversoAPI.DTOs
{
    public class PetDto
    {
        public string Name { get; set; } = null!;
        public string Species { get; set; } = null!;
        public string? Breed { get; set; }
        public string? Color { get; set; }
        public string Size { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
    }

}
