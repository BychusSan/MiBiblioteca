using ApiMiBiblioteca.DTOs;
using ApiMiBiblioteca.Filters;
using ApiMiBiblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace ApiMiBiblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    //[TypeFilter(typeof(FiltroDeExcepcion))]  ////activa la exception solo en este controller
    public class AutoresController : ControllerBase
    {
        private readonly MiBibliotecaContext _context;
        private readonly ILogger<AutoresController> _logger;


        public AutoresController(MiBibliotecaContext context, ILogger<AutoresController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region GET

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Autore>>> GetAutores()
        {
            var autores = await _context.Autores.ToListAsync();
            if (autores == null)
            {
                return NotFound();
            }
            _logger.LogInformation("Obteniendo familias"); // Ejecutar y ver el resultado en consola o en ventana salida

            return Ok(autores);
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<Autore>> GetAutoresId([FromRoute] int id)
        {
            if (_context.Autores == null)
            {
                return NotFound();
            }
            var autores = await _context.Autores.FindAsync(id);

            if (autores == null)
            {
                return NotFound();
            }

            return Ok(autores);
        }


        [HttpGet("AutoresInfo")]
        public async Task<ActionResult<IEnumerable<Autore>>> ObtenerListaAutores()
        {

            var autoresDTO = await _context.Autores.Select(a => new DTOListaAutores
            {
                IdAutor = a.IdAutor,
                Nombre = a.Nombre,
                TotalLibros = a.Libros.Count(),
                PromedioPrecio = a.Libros.Average(l => l.Precio),
                Libro = a.Libros.Select(l => new DTOLibrosAutor
                {
                    ISBN = l.Isbn,
                    Titulo = l.Titulo,
                    Precio = l.Precio,
                }).ToList(),
            }).ToListAsync();

            return Ok(autoresDTO);
        }


        [HttpGet("librosautores/{AutoresInfo2}")]
        public async Task<ActionResult<IEnumerable<Autore>>> ObtenerListaAutores2(int AutoresInfo2)
        {

            var autoresDTO = await _context.Autores.Where(a => a.IdAutor == AutoresInfo2).Select(a => new DTOListaAutores
            {
                IdAutor = a.IdAutor,
                Nombre = a.Nombre,
                TotalLibros = a.Libros.Count(),
                PromedioPrecio = a.Libros.Average(l => l.Precio),
                Libro = a.Libros.Select(l => new DTOLibrosAutor
                {
                    ISBN = l.Isbn,
                    Titulo = l.Titulo,
                    Precio = l.Precio,
                }).ToList(),
            }).ToListAsync();

            return Ok(autoresDTO);
        }


        #endregion

        #region POST

        [HttpPost]
        public async Task<ActionResult<Autore>> PostAutor(DTOEditorial autores)
        {
            var newAutor = new Autore()
            {
                Nombre = autores.Nombre
            };

            await _context.AddAsync(newAutor);
            await _context.SaveChangesAsync();

            return Created("Autor", new { AutorId = newAutor.IdAutor });
        }

        #endregion


        #region PUT
        [HttpPut("{id:int}")]
        public async Task<ActionResult> PutAutores([FromRoute] int id, [FromBody] DTOAutor dtoAutor)
        {

            var autorUpdate = await _context.Autores.AsTracking().FirstOrDefaultAsync(x => x.IdAutor == id);

            if (autorUpdate == null)
            {
                return NotFound();
            }

            autorUpdate.Nombre = dtoAutor.Nombre;
            _context.Update(autorUpdate);


            await _context.SaveChangesAsync();
            return NoContent();

        }



        #endregion


        #region DELETE

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteAutor(int id)
        {

            var hayAutor = await _context.Libros.AnyAsync(x => x.AutorId == id);

            if (hayAutor)
            {
                return NotFound("ERROR: hay libros que coinciden con el autor");
            }

            var autor = await _context.Autores.FirstOrDefaultAsync(x => x.IdAutor == id);

            if (autor is null)
            {
                return NotFound("El autor no existe");
            }

            _context.Remove(autor);
            await _context.SaveChangesAsync();
            return Ok();
        }

        #endregion

        #region SQL

        [HttpDelete("eliminarautor")]
        public async Task<ActionResult> EliminarAutor([FromQuery] int id)
        {
            // Verificar si hay libros asociados a este autor
            var tieneLibros = await _context.Libros.AnyAsync(l => l.AutorId == id);

            if (tieneLibros)
            {
                return BadRequest("No se puede eliminar el autor porque tiene libros asociados.");
            }

            // Si no hay libros asociados, se puede eliminar el autor
            var sql = $"DELETE FROM Autores WHERE IdAutor = {id}";
            await _context.Database.ExecuteSqlRawAsync(sql);

            return NoContent();
        }


        #endregion

    }
}
