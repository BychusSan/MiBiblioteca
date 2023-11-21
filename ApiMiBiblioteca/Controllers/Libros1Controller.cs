using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiMiBiblioteca.Models;
using ApiMiBiblioteca.DTOs;
using ApiMiBiblioteca.Services;

namespace ApiMiBiblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Libros1Controller : ControllerBase
    {
        private readonly MiBibliotecaContext _context;
        private readonly IGestorArchivos _gestorArchivosLocal;
        private readonly OperacionesService _operacionesService;




        public Libros1Controller(MiBibliotecaContext context, IGestorArchivos gestorArchivosLocal, OperacionesService operacionesService)
        {
            _context = context;
            _gestorArchivosLocal = gestorArchivosLocal;
            _operacionesService = operacionesService;

        }

        #region POST LIBRO CON IMAGENES

        // Agregar libro cob IMAGEN (Transient)
        [HttpPost]
        public async Task<ActionResult> PostLibros([FromForm] DTOAgregarLibro libro)
        {
            // ACTIVIDAD 4 PARTE 2 ( PROBAR EL METODO DEL SERVICE )
            var probarMetodo = await _operacionesService.TiempoDeEspera();
            if (!probarMetodo)
            {
                return BadRequest("Espera 30 segundos.");
            }

            Libro newLibro = new Libro
            {
                Isbn = libro.Isbn,
                Titulo = libro.Titulo,
                Paginas = libro.Paginas,
                FotoPortada = "",
                Precio = libro.Precio,
                AutorId = libro.AutorId,
                EditorialId = libro.EditorialId,
                Descatalogado = false,

            };
            if (libro.FotoPortada != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Extraemos la imagen de la petición
                    await libro.FotoPortada.CopyToAsync(memoryStream);
                    // La convertimos a un array de bytes que es lo que necesita el método de guardar
                    var contenido = memoryStream.ToArray();
                    // La extensión la necesitamos para guardar el archivo
                    var extension = Path.GetExtension(libro.FotoPortada.FileName);
                    // Recibimos el nombre del archivo
                    // El servicio Transient GestorArchivosLocal instancia el servicio y cuando se deja de usar se destruye
                    // muere aquí
                    newLibro.FotoPortada = await _gestorArchivosLocal.GuardarArchivo(contenido, extension, "imagenes",
                        libro.FotoPortada.ContentType);
                }
            }

            await _context.AddAsync(newLibro);
            await _context.SaveChangesAsync();
            await _operacionesService.AddOperacion("Post", "Libros1");

            // el servicio scoped (el _context) muere aquí, con el return
            return Ok(newLibro);
        }
        #endregion

        #region PUT lIBROS CON IMAGENES

        [HttpPut]
        public async Task<ActionResult<Libro>> PutLibro([FromForm] DTOAgregarLibro libro)
        {
            var libroExiste = await _context.Libros.AsTracking().FirstOrDefaultAsync(x => x.Isbn == libro.Isbn);

            if (libroExiste == null)
            {
                return NotFound("El ISBN no coincide.");
            }

            var autorExiste = await _context.Autores.FindAsync(libro.AutorId);
            if (autorExiste == null)
            {
                return BadRequest("El autor no existe.");
            }

            var editorialExiste = await _context.Libros.AnyAsync(x => x.EditorialId == libro.EditorialId);
            if (!editorialExiste)
            {
                return BadRequest("La editorial no existe.");
            }


            libroExiste.Isbn = libro.Isbn;
            libroExiste.Titulo = libro.Titulo;
            libroExiste.Paginas = libro.Paginas;
            libroExiste.AutorId = libro.AutorId;
            libroExiste.EditorialId = libro.EditorialId;
            libroExiste.Precio = libro.Precio;
            libroExiste.FotoPortada = "";

            if (libro.FotoPortada != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await libro.FotoPortada.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(libro.FotoPortada.FileName);
                    var nuevaRutaImagen = await _gestorArchivosLocal.GuardarArchivo(contenido, extension, "imagenes", libro.FotoPortada.ContentType);

                    // elimina la imagen anterior
                    await _gestorArchivosLocal.BorrarArchivo(libroExiste.FotoPortada, "imagenes");

                    // añade una nueva ruta para la imagen
                    libroExiste.FotoPortada = nuevaRutaImagen;
                }
            }

            _context.Update(libroExiste);
            await _context.SaveChangesAsync();
            await _operacionesService.AddOperacion("Put", "Libros1");

            //return (libroExiste);
            return NoContent();
        }

        #endregion

        #region PUT lIBROS CON IMAGENES

        [HttpPut("/SOLOIMAGENES/")]
        public async Task<ActionResult<Libro>> PutLibro2([FromForm] DTOAñadirImagen libro)
        {
            // ACTIVIDAD 4 PARTE 2 ( PROBAR EL METODO DEL SERVICE )
            var probarMetodo = await _operacionesService.TiempoDeEspera();
            if (!probarMetodo)
            {
                return BadRequest("Espera 8 segundos.");
            }


            var libroExiste = await _context.Libros.AsTracking().FirstOrDefaultAsync(x => x.Isbn == libro.Isbn);

            if (libroExiste == null)
            {
                return NotFound("El ISBN no coincide.");
            }

            libroExiste.Isbn = libro.Isbn; 
            libroExiste.FotoPortada = "";

            if (libro.FotoPortada != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await libro.FotoPortada.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(libro.FotoPortada.FileName);
                    var nuevaRutaImagen = await _gestorArchivosLocal.GuardarArchivo(contenido, extension, "imagenes", libro.FotoPortada.ContentType);

                    // elimina la imagen anterior
                    await _gestorArchivosLocal.BorrarArchivo(libroExiste.FotoPortada, "imagenes");

                    // añade una nueva ruta para la imagen
                    libroExiste.FotoPortada = nuevaRutaImagen;
                }
            }

            _context.Update(libroExiste);
            await _context.SaveChangesAsync();
            await _operacionesService.AddOperacion("Put", "Libros1");

 
            //return (libroExiste);
            return NoContent();
        }

        #endregion

        #region DELETE CON IMAGENES

        [HttpDelete("{ISBN}")]
        public async Task<ActionResult> BorrarLibro(string ISBN)
        {
            var libro = await _context.Libros.FirstOrDefaultAsync(x => x.Isbn == ISBN);

            if (libro == null)
            {
                return NotFound("El libro no existe");
            }

             await _gestorArchivosLocal.BorrarArchivo(libro.FotoPortada, "imagenes");
            
            _context.Remove(libro);
            await _context.SaveChangesAsync();
            await _operacionesService.AddOperacion("Delete", "Libros1");

            return Ok();
        }


        #endregion


    }

}

