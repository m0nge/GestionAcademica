using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionAcademica.Models;

namespace GestionAcademica.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValidationController : ControllerBase
    {
        private readonly GestionAcademicaContext _context;

        public ValidationController(GestionAcademicaContext context)
        {
            _context = context;
        }

        [HttpGet("carnet-exists")]
        public async Task<IActionResult> CarnetExists(string carnet)
        {
            if (string.IsNullOrEmpty(carnet))
            {
                return BadRequest("Carnet no puede estar vacío");
            }

            try
            {
                var exists = await _context.Estudiantes
                    .AnyAsync(e => e.Carnet == carnet);

                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al verificar carnet");
            }
        }

        [HttpGet("email-exists")]
        public async Task<IActionResult> EmailExists(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email no puede estar vacío");
            }

            try
            {
                var exists = await _context.Estudiantes
                    .AnyAsync(e => e.Email == email);

                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al verificar email");
            }
        }

        [HttpGet("telefono-exists")]
        public async Task<IActionResult> TelefonoExists(string telefono)
        {
            if (string.IsNullOrEmpty(telefono))
            {
                return BadRequest("Teléfono no puede estar vacío");
            }

            try
            {
                var exists = await _context.Estudiantes
                    .AnyAsync(e => e.Telefono == telefono);

                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al verificar teléfono");
            }
        }
    }
}