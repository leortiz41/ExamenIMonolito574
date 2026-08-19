using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioMascotas.Models;
using System.Globalization;
using System.Text.RegularExpressions;


namespace RefugioMascotas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MascotasController : ControllerBase
{
    private readonly RefugioDbContext _db;

    public MascotasController(RefugioDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var mascotas = await _db.Mascotas
            .Include(m => m.Cuidador)
            .OrderBy(m => m.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToListAsync();

        return Ok(mascotas);
    }

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

    // TODO (Ticket 1): GetById(int id) -> 400 si id <= 0, 404 si no existe

    [HttpPost]
    public async Task<IActionResult> Create(Mascota mascota)
    {
        if (string.IsNullOrWhiteSpace(mascota.Nombre))
            return BadRequest("El nombre de la mascota es obligatorio.");


        if (string.IsNullOrWhiteSpace(mascota.Especie))
            return BadRequest("La especie es obligatoria.");

        // Un voluntario reportó que esta validación de edad se comporta raro (Ticket 0)
        if (mascota.Edad < 0 || mascota.Edad > 30)
            return BadRequest("La edad debe estar entre 0 y 30 años.");

        var cuidadorExiste = await _db.Cuidadores.AnyAsync(c => c.Id == mascota.CuidadorId);
        if (!cuidadorExiste)
            return BadRequest("El cuidador especificado no existe.");

        _db.Mascotas.Add(mascota);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = mascota.Id }, mascota);
    }

[HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {

    if (id <= 0)

        return BadRequest("El id debe ser mayor que cero.");

    var mascota = await _db.Mascotas.FindAsync(id);

    if (mascota == null)

        return NotFound("La mascota no existe.");

    return Ok(mascota);

    }

    // TODO (Ticket 4): Update(int id, Mascota mascotaActualizada)
[HttpPost]
public async Task<IActionResult> Create(Cuidador cuidador)
{
    // 1. Normalización
    cuidador.Nombre = NormalizarTexto(cuidador.Nombre);
    cuidador.Turno = NormalizarTexto(cuidador.Turno);

    // 2. Validación de nombre
    if (string.IsNullOrWhiteSpace(cuidador.Nombre))
        return BadRequest("El nombre del cuidador es obligatorio.");

    if (cuidador.Nombre.Length < 2 || cuidador.Nombre.Length > 100)
        return BadRequest("El nombre del cuidador debe tener entre 2 y 100 caracteres.");

    // 3. Validación del turno
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
    

    // TODO (Ticket 5): Delete(int id) -> 409 si EnTratamiento es true
}
