namespace PetiversoAPI.DTOs
{
    public class RegisterResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid? UserId { get; set; }
    }
}
