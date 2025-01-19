namespace PetiversoAPI.Services
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string password);
    }

    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            // Aqui usamos um método simplificado para demonstração. Recomendado usar técnicas modernas, como bcrypt.
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            return hashedPassword == Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}