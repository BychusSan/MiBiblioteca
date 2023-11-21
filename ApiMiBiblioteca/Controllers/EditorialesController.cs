using ApiMiBiblioteca.DTOs;
using ApiMiBiblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace ApiMiBiblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class EditorialesController : ControllerBase
    {

        private readonly MiBibliotecaContext _context;
        private readonly ILogger<AutoresController> _logger;


        public EditorialesController(MiBibliotecaContext context, ILogger<AutoresController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region GET

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Editoriale>>> GetEditoriales()
        {
            _logger.LogInformation("OBTENIENDO EDITORIALES"); // Ejecutar y ver el resultado en consola o en ventana salida
            var editoriales = await _context.Editoriales.ToListAsync();
            if (editoriales == null)
            {
                return NotFound();
            }

            return Ok(editoriales);
        }




        [HttpGet("{id:int}")]
        public async Task<ActionResult<Editoriale>> GetEditorialesId2([FromRoute]int id)
        {
          
            var editoriales = await _context.Editoriales.FindAsync(id);

            if (editoriales == null)
            {
                return NotFound();
            }

            return Ok(editoriales);
        }

        [HttpGet("ObtenerEditorialesConLibros")]
        public async Task<ActionResult<IEnumerable<Editoriale>>> GetEditorialesConLibros()
        {
            var editorialesConLibros = await _context.Editoriales
                .Include(e => e.Libros).ToListAsync();
            if (editorialesConLibros == null)
            {
                return NotFound();
            }

            return Ok(editorialesConLibros);
        }


        #endregion

        #region POST

        //[HttpPost]
        //public async Task<ActionResult<Editoriales>> AgregarEditorial(Editoriales editorial)
        //{
        //    var newEditorial = new Editoriales()
        //    {
        //        Nombre = editorial.Nombre
        //    };
        //    await _context.AddAsync(newEditorial);
        //    await _context.SaveChangesAsync();

        //    return Created("Editorial", new {familia = newEditorial});
        //}


        [HttpPost]
        public async Task<ActionResult<Editoriale>> AgregarEditorial(DTOEditorial dtoeditorial)
        {
            var newEditorial = new Editoriale()
            {
                Nombre = dtoeditorial.Nombre
            };

            await _context.AddAsync(newEditorial);
            await _context.SaveChangesAsync();

            return Created("Editorial", new { editorialId = newEditorial.IdEditorial });
        }



        #endregion

        #region PUT
        [HttpPut("{id:int}")]
        public async Task<ActionResult> PutEditorial([FromRoute] int id, [FromBody] DTOEditorial dtoEditorial)
        {
        

            var editorialUpdate = await _context.Editoriales.AsTracking().FirstOrDefaultAsync(x => x.IdEditorial == id);

            if (editorialUpdate == null)
            {
                return NotFound();
            }

            editorialUpdate.Nombre = dtoEditorial.Nombre;
            _context.Update(editorialUpdate);

         
                await _context.SaveChangesAsync();
                return NoContent();
         
        }

        #endregion


        #region DELETE

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteEditorial(int id)
        {

            var hayLibros = await _context.Libros.AnyAsync(x => x.EditorialId == id);

            if (hayLibros)
            {
                return NotFound("ERROR: hay libros que coinciden con la");
            }

            var editorial = await _context.Editoriales.FirstOrDefaultAsync(x => x.IdEditorial == id);

            if (editorial is null)
            {
                return NotFound("La editorial no existe");
            }

            _context.Remove(editorial);
            await _context.SaveChangesAsync();
            return Ok();
        }

        #endregion

        #region SQL

        [HttpPut("modificareditorial")]
        public async Task<ActionResult> ModificarEditorial([FromQuery] int id, [FromQuery] string nuevoNombre)
        {
            var editorial = await _context.Editoriales.FirstOrDefaultAsync(e => e.IdEditorial == id);

            if (editorial == null)
            {
                return NotFound("La editorial no fue encontrada.");
            }

            var sql = $"UPDATE Editoriales SET Nombre = '{nuevoNombre}' WHERE IdEditorial = {id}";
            await _context.Database.ExecuteSqlRawAsync(sql);

            return NoContent();
        }


        #endregion

    }
}