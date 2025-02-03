namespace PetiversoAPI.DTOs
{
    public class PetDto
    {
        public string Name { get; set; } = null!;
        public string Species { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public string? Breed { get; set; }
        public string? ColorPri { get; set; }
        public string? ColorSec { get; set; }
        public string Size { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
        public IFormFile? PhotoFile { get; set; }
    }

}
