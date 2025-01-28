using System.ComponentModel.DataAnnotations;

namespace PetiversoAPI.Models
{
    public class PetPhoto
    {
        [Key]
        public Guid PhotoId { get; set; } = Guid.NewGuid();
        public Guid PetId { get; set; }
        public string FilePath { get; set; } = null!;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Relação com o Pet
        public Pet Pet { get; set; } = null!;
    }

}
