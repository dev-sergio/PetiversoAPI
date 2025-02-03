using Microsoft.AspNetCore.Mvc;
using PetiversoAPI.DTOs;
using PetiversoAPI.Services;
using System.Security.Claims;

namespace PetiversoAPI.Controllers
{
    [Route("api/pets")]
    [ApiController]
    public class PetController(IPetService petService) : ControllerBase
    {
        private readonly IPetService _petService = petService;

        [HttpPost]
        public async Task<IActionResult> AddPet([FromForm] PetDto petDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !Guid.TryParse(userId, out var parsedUserId))
                return Unauthorized(new { success = false, message = "Usuário não autenticado." });

            var result = await _petService.AddPetAsync(parsedUserId, petDto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !Guid.TryParse(userId, out var parsedUserId))
                return Unauthorized(new { success = false, message = "Usuário não autenticado." });

            var result = await _petService.GetUserPetsAsync(parsedUserId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePet(Guid id, [FromForm] PetDto petDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !Guid.TryParse(userId, out var parsedUserId))
                return Unauthorized(new { success = false, message = "Usuário não autenticado." });

            var result = await _petService.UpdatePetAsync(id, parsedUserId, petDto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }



        [HttpPost("{petId}/photos")]
        public async Task<IActionResult> UploadPhoto(Guid petId, IFormFile photoFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !Guid.TryParse(userId, out Guid parsedUserId))
                return Unauthorized(new { success = false, message = "Usuário não autenticado." });

            if (photoFile == null || photoFile.Length == 0)
                return BadRequest(new { success = false, message = "Foto inválida." });

            // Verificar se o pet pertence ao usuário autenticado
            var isOwner = await _petService.IsUserOwnerOfPetAsync(parsedUserId, petId);
            if (!isOwner)
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Usuário não autorizado para adicionar fotos a este pet." });

            var result = await _petService.UploadPhotoAsync(petId, photoFile);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, data = result.Data, message = "Foto adicionada com sucesso!" });
        }

        [HttpGet("{petId}/photos")]
        public async Task<IActionResult> GetPhotos(Guid petId)
        {
            var result = await _petService.GetPhotosAsync(petId);
            if (!result.Success)
                return NotFound(new { success = false, message = result.Message });

            return Ok(new { success = true, data = result.Data });
        }

        [HttpDelete("photos/{photoId}")]
        public async Task<IActionResult> DeletePhoto(Guid photoId)
        {
            var result = await _petService.DeletePhotoAsync(photoId);
            if (!result.Success)
                return NotFound(new { success = false, message = result.Message });

            return Ok(new { success = true, message = "Foto deletada com sucesso!" });
        }

    }

}
