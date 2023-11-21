using ApiMiBiblioteca.DTOs;
using ApiMiBiblioteca.Models;
using ApiMiBiblioteca.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Microsoft.EntityFrameworkCore;
using System.Drawing;


namespace ApiMiBiblioteca.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LibrosController : ControllerBase
    {
        //private readonly MiBibliotecaContext _context;
        //private readonly GestorArchivosLocal _gestorArchivosLocal;
        //private readonly OperacionesService _operacionesService;



        //public LibrosController(MiBibliotecaContext context,GestorArchivosLocal gestorArchivosLocal, OperacionesService operacionesService)
        //{
        //    _context = context;
        //    _gestorArchivosLocal = gestorArchivosLocal;
        //    _operacionesService = operacionesService;
        //}
        // Inyectamos el servicio de gestión de archivos, el context y el servicio de registro de operaciones
        private readonly MiBibliotecaContext _context;
        private readonly IGestorArchivos _gestorArchivosLocal;
        private readonly OperacionesService _operacionesService;

        public LibrosController(MiBibliotecaContext context, IGestorArchivos gestorArchivosLocal, OperacionesService operacionesService)
        {
            _context = context;
            _gestorArchivosLocal = gestorArchivosLocal;
            _operacionesService = operacionesService;
        }

        #region GET

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Libro>>> GetLibros()
        {
            // ACTIVIDAD 4 PARTE 2 ( PROBAR EL METODO DEL SERVICE )
            var probarMetodo = await _operacionesService.TiempoDeEspera();
            if (!probarMetodo)
            {
                return BadRequest("Espera 5 segundos.");

            }

            var libros = await _context.Libros.ToListAsync();
            if (libros == null)
            {
                return NotFound();

            }
            await _operacionesService.AddOperacion("Get", "Libros");

            return Ok(libros);
        }

        //[Route("api/[controller]")]
        [HttpGet("/API/[controller]")]
        public async Task<ActionResult<IEnumerable<Libro>>> GetLibros2()
        {
            var libros = await _context.Libros.ToListAsync();
            if (libros == null)
            {
                return NotFound();
            }

            return Ok(libros);
        }


        [HttpGet("{ISBN}")]
        public async Task<ActionResult<Libro>> GetLibros(string ISBN)
        {
            if (_context.Libros == null)
            {
                return NotFound();
            }
            var libros = await _context.Libros.FindAsync(ISBN);

            if (libros == null)
            {
                //return NotFound();
                throw new Exception("Se ha introduicido un ISBN incorrecto");

            }

            return libros;
        }

        [HttpGet("titulo/contiene/{text}")]
        public async Task<ActionResult<IEnumerable<Libro>>> GetLibrosText(string text)
        {
            if (_context.Libros == null)
            {
                return NotFound();
            }
            var libros = _context.Libros.Where(a=>a.Titulo.Contains(text));

            if (libros == null)
            {
                return NotFound();
            }

            return await libros.ToListAsync();
        }
        ///api/libros/ordenadosportitulo/dirección: 
        //Si dirección es true, devolverá los libros ordenados por título de forma ascendente
        //Si dirección es false, devolverá los libros ordenados por título de forma descendente.
        [HttpGet("ordenadosportitulo/{direccion}")]
        public async Task<ActionResult<IEnumerable<Libro>>> GetLibrosOrdenadosPorTitulo(bool DireccionOrdenar)
        {
            var librosQuery = _context.Libros.AsQueryable();

            if (DireccionOrdenar)
            {
                librosQuery = librosQuery.OrderBy(l => l.Titulo);
            }
            else
            {
                librosQuery = librosQuery.OrderByDescending(l => l.Titulo);
            }

            var libros = await librosQuery.ToListAsync();

            if (libros == null)
            {
                return NotFound();
            }

            return Ok(libros);
        }


        [HttpGet("parametrocontienequerystring")]
        public async Task<ActionResult<IEnumerable<Libro>>> GetLibrosEntrePreciosQueryString([FromQuery] decimal PrecioMinimo, [FromQuery] decimal PrecioMaximo )
        {
            var libritos = await _context.Libros.Where(x => x.Precio > PrecioMinimo && x.Precio < PrecioMaximo).ToListAsync();
            return Ok(libritos);
        }

        [HttpGet("desdehasta/{desde}/{hasta}")]
        public async Task<ActionResult<IEnumerable<Libro>>> GetLibrosDesdeHasta(int IdEditorialDesde, int IdEditorialHasta)
        {
            var libros = await _context.Libros
                .Where(l => l.EditorialId >= IdEditorialDesde && l.EditorialId <= IdEditorialHasta).ToListAsync();

            if (libros == null)
            {
                return NotFound();
            }

            return Ok(libros);
        }


        [HttpGet("VentaLibros")]
        public async Task<ActionResult<IEnumerable<DTOVentaLibro>>> GetVentaLibros()
        {
            var libritos = await _context.Libros.ToListAsync();

            var librosParaVenta = libritos.Select(libro => new DTOVentaLibro
            {
                TituloLibro = libro.Titulo,
                PrecioLibro = libro.Precio.GetValueOrDefault(0),
            });

            return Ok(librosParaVenta);
        }

        [HttpGet("librosagrupadospordescatalogados")]

        public async Task<ActionResult<IEnumerable<Libro>>> GetCatalogados()
        {
            var prueba = _context.Libros.GroupBy(d => d.Descatalogado)
                .Select(x => new
                {
                    //Descatalogado = (bool)x.Key ? "SI" : "No",
                    Descatalogado = x.Key,
                    Cantidad = x.Count()
                }).ToList();

            return Ok(prueba);
        }


        #endregion


        #region POST

        [HttpPost]
        public async Task<ActionResult<Libro>> PostLibro(DTOLibrosPost dtolibrospost)
        {

            var autorExiste = await _context.Autores.FindAsync(dtolibrospost.AutorId);
            if (autorExiste == null)
            {
                return BadRequest("El autor no existe.");
            }

            var editorialExiste = await _context.Libros.AnyAsync(x => x.EditorialId == dtolibrospost.EditorialId );
            if (!editorialExiste)
            {
                return BadRequest("La editorial no existe.");
            }

            //var editorialExiste2 = await _context.Libros.AnyAsync(x => x.EditorialId == dtolibrospost.EditorialId);
            //if (editorialExiste2 != null) //Alternativa a la formula de arriba, aunque esta es menos correcta
            //{
            //    return BadRequest("La editorial no existe.");
            //}

            var nuevoLibro = new Libro()
            {
                Isbn = dtolibrospost.Isbn,
                Titulo = dtolibrospost.Titulo,
                Paginas = dtolibrospost.Paginas,
                FotoPortada = dtolibrospost.FotoPortada,
                Descatalogado = dtolibrospost.Descatalogado,
                AutorId = dtolibrospost.AutorId,
                EditorialId = dtolibrospost.EditorialId,
                Precio = dtolibrospost.Precio
            };

            await _context.AddAsync(nuevoLibro);
            await _context.SaveChangesAsync();

            return Created("Libro", new { isbn = nuevoLibro.Isbn });
        }

        [HttpPost("varios")]
        public async Task<ActionResult> PostLibros([FromBody] DTOLibrosPost[] libros)
        {
            //Método 1.Por cada DTOFamilia creamos un objeto Familia y lo agregamos.Al final, hacemos el SaveChanges
            //foreach (var f in libros)
            //{
            //    var nuevaFamilia = new Libros
            //    {
            //        Isbn = f.Isbn
            //    };

            //    await _context.AddAsync(nuevaFamilia);
            //}

            //await _context.SaveChangesAsync();

            //Método 2.Consturimos una lista de objetos Familia.Por cada DTOFamilia agregamos ese objeto a la lista.Al final, agregamos la lista
            // entera con AddRangeAsync y al final hacemos el SaveChanges

            //    //List<Libros> variasFamilias = new();

            var variasFamilias = new List<Libro>();
            foreach (var f in libros)
            {
                variasFamilias.Add(new Libro
                {
                    Isbn = f.Isbn,
                    Titulo = f.Titulo,
                    Paginas = f.Paginas,
                    FotoPortada = f.FotoPortada,
                    Descatalogado = f.Descatalogado,
                    AutorId = f.AutorId,
                    EditorialId = f.EditorialId,
                    Precio = f.Precio

                });
            }
            await _context.AddRangeAsync(variasFamilias);
            await _context.SaveChangesAsync();

            return Ok();
        }


        #endregion


        #region PUT


        [HttpPut]
        public async Task<ActionResult<Libro>> PutLibro(DTOLibrosPost dtolibrospost2)
        {

            var libroExiste = await _context.Libros.AsTracking().FirstOrDefaultAsync(x => x.Isbn == dtolibrospost2.Isbn);

            if (libroExiste == null)
            {
                return NotFound("El ISBN no coincide.");
            }


            var autorExiste = await _context.Autores.FindAsync(dtolibrospost2.AutorId);
            if (autorExiste == null)
            {
                return BadRequest("El autor no existe.");
            }

            var editorialExiste = await _context.Libros.AnyAsync(x => x.EditorialId == dtolibrospost2.EditorialId);
            if (!editorialExiste)
            {
                return BadRequest("La editorial no existe.");
            }


            libroExiste.Isbn = dtolibrospost2.Isbn;
            libroExiste.Titulo = dtolibrospost2.Titulo;
            libroExiste.Paginas = dtolibrospost2.Paginas;
            libroExiste.FotoPortada = dtolibrospost2.FotoPortada;
            libroExiste.Descatalogado = dtolibrospost2.Descatalogado;
            libroExiste.AutorId = dtolibrospost2.AutorId;
            libroExiste.EditorialId = dtolibrospost2.EditorialId;
            libroExiste.Precio = dtolibrospost2.Precio;

            _context.Update(libroExiste);

            await _context.SaveChangesAsync();
            return NoContent();
        }



        #endregion


        #region DELETE

        [HttpDelete("{ISBN}")]
        public async Task<ActionResult> BorrarLibro(string ISBN)
        {

            var libro = await _context.Libros.FirstOrDefaultAsync(x => x.Isbn == ISBN);

            if (libro is null)
            {
                return NotFound("El libro no existe");
            }

            _context.Remove(libro);
            await _context.SaveChangesAsync();
            return Ok();
        }

        #endregion


        #region SQL

        [HttpGet("filtrarlibros")]
        public async Task<ActionResult<IEnumerable<Libro>>> FiltrarLibros([FromQuery] decimal? precio, [FromQuery] bool? descatalogado)
        {
            var librosQuery = _context.Libros.AsQueryable();

            if (precio.HasValue)
            {
                librosQuery = librosQuery.Where(l => l.Precio.HasValue && l.Precio.Value > precio.Value);
            }

            if (descatalogado.HasValue)
            {
                librosQuery = librosQuery.Where(l => l.Descatalogado == descatalogado.Value);
            }

            var libros = await librosQuery.ToListAsync();

            return Ok(libros);
        }


        #endregion

    }
}
    
