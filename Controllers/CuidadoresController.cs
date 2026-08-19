using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioMascotas.Models;
using System.Globalization;
using System.Text.RegularExpressions;   
using System.Linq;

namespace RefugioMascotas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuidadoresController : ControllerBase
{
    private readonly RefugioDbContext _db;

    public CuidadoresController(RefugioDbContext db) => _db = db;



     private static string NormalizarTexto(string texto)
{
    if (string.IsNullOrWhiteSpace(texto))
        return string.Empty;

    // Quita espacios al inicio/final y colapsa espacios repetidos
    texto = Regex.Replace(texto.Trim(), @"\s+", " ");

    // Convierte a nombre propio
    texto = texto.ToLower();

    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(texto);
}

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cuidadores = await _db.Cuidadores
            .OrderBy(c => c.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToListAsync();

        return Ok(cuidadores);
    }

    // TODO (Ticket 1): GetById(int id) -> 400 si id <= 0, 404 si no existe
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    if (id <= 0)
        return BadRequest("El id debe ser mayor que cero.");

    var cuidador = await _db.Cuidadores.FindAsync(id);

    if (cuidador == null)
        return NotFound("El cuidador no existe.");

    return Ok(cuidador);
}

        // TODO (Ticket 2): normalizar texto (espacios, capitalización) y validar
        // formato de Nombre y que Turno sea exactamente "Mañana", "Tarde" o "Noche"

    [HttpPost]
    public async Task<IActionResult> Create(Cuidador cuidador)
{
    // 1. Normalización
    cuidador.Nombre = NormalizarTexto(cuidador.Nombre);
    cuidador.Turno = NormalizarTexto(cuidador.Turno);

        if (string.IsNullOrWhiteSpace(cuidador.Nombre))
            return BadRequest("El nombre del cuidador es obligatorio.");

        if (cuidador.Nombre.Length < 2 || cuidador.Nombre.Length > 100)
            return BadRequest("El nombre del cuidador debe tener entre 2 y 100 caracteres.");

        if (string.IsNullOrWhiteSpace(cuidador.Turno))
            return BadRequest("El turno es obligatorio.");

    var turnosPermitidos = new[] { "Mañana", "Tarde", "Noche" };

    if (!turnosPermitidos.Contains(cuidador.Turno))
        return BadRequest(
            "El turno debe ser exactamente uno de estos valores: Mañana, Tarde o Noche."
        );

    _db.Cuidadores.Add(cuidador);
    await _db.SaveChangesAsync();

    return CreatedAtAction(
        nameof(GetById),
        new { id = cuidador.Id },
        cuidador
    );
}

        // TODO (Ticket 3): validar duplicado (Nombre + Turno) -> 409 Conflict

        if (string.IsNullOrWhiteSpace(cuidador.Nombre))
            return BadRequest("El nombre del cuidador es obligatorio.");

        _db.Cuidadores.Add(cuidador);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = cuidador.Id }, cuidador);
    }

    // TODO (Ticket 4): Update(int id, Cuidador cuidadorActualizado)

    // TODO (Ticket 5): Delete(int id) -> 409 si el cuidador tiene mascotas asignadas

    // TODO (Ticket 6): GetMascotasPorCuidador(int id)
    // Ruta esperada: GET api/cuidadores/{id}/mascotas -> 404 si el cuidador no existe
}
