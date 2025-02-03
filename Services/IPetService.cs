using Microsoft.EntityFrameworkCore;
using PetiversoAPI.DTOs;
using PetiversoAPI.Models;

namespace PetiversoAPI.Services
{
    public interface IPetService
    {
        Task<ServiceResponse<Guid>> AddPetAsync(Guid userId, PetDto petDto);
        Task<ServiceResponse<IEnumerable<Pet>>> GetUserPetsAsync(Guid userId);
        Task<ServiceResponse<string>> UploadPhotoAsync(Guid petId, IFormFile photoFile);
        Task<ServiceResponse<List<string>>> GetPhotosAsync(Guid petId);
        Task<ServiceResponse> DeletePhotoAsync(Guid photoId);
        Task<bool> IsUserOwnerOfPetAsync(Guid userId, Guid petId);
    }

    public class PetService(AppDbContext context, IConfiguration configuration) : IPetService
    {
        private readonly AppDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        public async Task<bool> IsUserOwnerOfPetAsync(Guid userId, Guid petId)
        {
            // Consulta direta no banco para verificar o dono do pet
            return await _context.Pets
                .AnyAsync(p => p.PetId == petId && p.UserId == userId);
        }


        public async Task<ServiceResponse<Guid>> AddPetAsync(Guid userId, PetDto petDto)
        {
            string photoPath;

            // Verifica se uma foto foi enviada e salva no servidor
            if (petDto.PhotoFile != null && petDto.PhotoFile.Length > 0)
            {
                var storagePath = _configuration["PhotoStorage:Path"];
                if (string.IsNullOrWhiteSpace(storagePath))
                {
                    return new ServiceResponse<Guid> { Success = false, Message = "Diretório de armazenamento não configurado." };
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(petDto.PhotoFile.FileName)}";
                photoPath = Path.Combine(storagePath, fileName);

                try
                {
                    using var stream = new FileStream(photoPath, FileMode.Create);
                    await petDto.PhotoFile.CopyToAsync(stream);
                }
                catch (Exception ex)
                {
                    return new ServiceResponse<Guid> { Success = false, Message = $"Erro ao salvar a foto: {ex.Message}" };
                }
            }
            else
            {
                var storagePath = _configuration["PhotoStorage:Path"];
                if (string.IsNullOrWhiteSpace(storagePath))
                {
                    return new ServiceResponse<Guid> { Success = false, Message = "Diretório de armazenamento não configurado." };
                }

                var fileName = "profile.jpg";
                photoPath = Path.Combine(storagePath, fileName);

            }

            var pet = new Pet
            {
                UserId = userId,
                Name = petDto.Name,
                Species = petDto.Species,
                Gender = petDto.Gender,
                Breed = petDto.Breed,
                ColorPri = petDto.ColorPri,
                ColorSec = petDto.ColorSec,
                Size = petDto.Size,
                BirthDate = petDto.BirthDate,
                Photo = photoPath // Salva o caminho da foto no banco
            };

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            return new ServiceResponse<Guid>
            {
                Success = true,
                Data = pet.PetId,
                Message = "Pet adicionado com sucesso!"
            };
        }

        public async Task<ServiceResponse<IEnumerable<Pet>>> GetUserPetsAsync(Guid userId)
        {
            var pets = await _context.Pets
                .Where(p => p.UserId == userId)
                .Include(p => p.Photos) // Inclui as fotos relacionadas
                .ToListAsync();

            return new ServiceResponse<IEnumerable<Pet>>
            {
                Success = true,
                Data = pets
            };
        }

        public async Task<ServiceResponse<string>> UploadPhotoAsync(Guid petId, IFormFile photoFile)
        {
            // Verificar se o Pet existe
            var petExists = await _context.Pets.AnyAsync(p => p.PetId == petId);
            if (!petExists)
                return new ServiceResponse<string> { Success = false, Message = "Pet não encontrado." };

            // Caminho para salvar a foto
            var storagePath = _configuration["PhotoStorage:Path"];
            if (string.IsNullOrWhiteSpace(storagePath))
            {
                return new ServiceResponse<string> { Success = false, Message = "Diretório de armazenamento não configurado." };
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photoFile.FileName)}";
            var filePath = Path.Combine(storagePath, fileName);

            try
            {
                // Salvar o arquivo fisicamente no disco
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(stream);
                }

                // Criar a foto e salvar no banco
                var photo = new PetPhoto
                {
                    PetId = petId,  // Vincular ao Pet usando a chave estrangeira
                    FilePath = filePath
                };

                await _context.PetPhotos.AddAsync(photo); // Adiciona diretamente ao DbSet
                await _context.SaveChangesAsync();        // Persistir mudanças no banco

                return new ServiceResponse<string> { Success = true, Data = filePath };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string> { Success = false, Message = $"Erro ao salvar a foto: {ex.Message}" };
            }
        }


        public async Task<ServiceResponse<List<string>>> GetPhotosAsync(Guid petId)
        {
            var pet = await _context.Pets.Include(p => p.Photos).FirstOrDefaultAsync(p => p.PetId == petId);
            if (pet == null)
                return new ServiceResponse<List<string>> { Success = false, Message = "Pet não encontrado." };

            var photoPaths = pet.Photos.Select(p => p.FilePath).ToList();
            return new ServiceResponse<List<string>> { Success = true, Data = photoPaths };
        }

        public async Task<ServiceResponse> DeletePhotoAsync(Guid photoId)
        {
            var photo = await _context.PetPhotos.FirstOrDefaultAsync(p => p.PhotoId == photoId);
            if (photo == null)
                return new ServiceResponse { Success = false, Message = "Foto não encontrada." };

            // Remover o arquivo do disco local
            if (File.Exists(photo.FilePath))
            {
                File.Delete(photo.FilePath);
            }

            // Remover o registro do banco
            _context.PetPhotos.Remove(photo);
            await _context.SaveChangesAsync();

            return new ServiceResponse { Success = true };
        }
    }
}
