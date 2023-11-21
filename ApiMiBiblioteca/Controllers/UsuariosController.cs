using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiMiBiblioteca.DTOs;
using ApiMiBiblioteca.Models;
using ApiMiBiblioteca.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAlmacen.DTOs;

namespace WebApiAlmacen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly MiBibliotecaContext _context;
        // Para acceder a la clave de encriptación, que está registrada en el appsettings.Development.json
        // necesitamos una dependencia más que llama IConfiguration. Esa configuración en un servicio
        // que tenemos que inyectar en el constructor
        private readonly IConfiguration _configuration;
        // Para encriptar, debemos incorporar otra dependencia más. Se llama IDataProtector. De nuevo, en un servicio
        // que tenemos que inyectar en el constructor
        private readonly IDataProtector _dataProtector;
        // El IDataProtector, para que funcione, lo debemos registrar en el program
        // Mirar en el program la línea: builder.Services.AddDataProtection();
        private readonly HashService _hashService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsuariosController(MiBibliotecaContext context, IConfiguration configuration, IDataProtectionProvider dataProtectionProvider, HashService hashService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            // Con el dataProtector podemos configurar un gestor de encriptación con esta línea
            // dataProtectionProvider.CreateProtector crea el gestor de encriptación y se apoya en la clave
            // de encriptación que tenemos en el appsettings.Development y que hemos llamado ClaveEncriptacion
            _dataProtector = dataProtectionProvider.CreateProtector(configuration["ClaveEncriptacion"]);
            _hashService = hashService;
            _httpContextAccessor = httpContextAccessor;
        }
        [HttpPost("encriptar/nuevousuario")]
        public async Task<ActionResult> PostNuevoUsuario([FromBody] DTOUsuario usuario)
        {
            // Encriptamos el password
            var passEncriptado = _dataProtector.Protect(usuario.Password);
            var newUsuario = new Usuario
            {
           
                Email = usuario.Email,
                Password = passEncriptado
            };
            await _context.Usuarios.AddAsync(newUsuario);
            await _context.SaveChangesAsync();

            return Ok(newUsuario);
        }

        [HttpPost("encriptar/checkusuario")]
        public async Task<ActionResult> PostCheckUserPassEncriptado([FromBody] DTOUsuario usuario)
        {
            // Esto haría un login con nuestro sistema de encriptación
            // Buscamos si existe el usuario
            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == usuario.Email);
            if (usuarioDB == null)
            {
                return Unauthorized(); // Si el usuario no existe, devolvemos un 401
            }
            // Descencriptamos el password
            var passDesencriptado = _dataProtector.Unprotect(usuarioDB.Password);
            // Y ahora miramos aver si el password de la base de datos que ya hemos encriptado cuando hemos creado el usuario
            // coincide con el que viene en la petición
            if (usuario.Password == passDesencriptado)
            {
                return Ok(); // Devolvemos un Ok si coinciden
            }
            else
            {
                return Unauthorized(); // Devolvemos un 401 si no coinciden
            }
        }

        //[HttpDelete("{ID}")]
        //public async Task<ActionResult> BorrarLibro([FromBody] DTOUsuario usuario)
        //{
        //    var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == usuario.Id);

        //    if (usuarioDB == null)
        //    {
        //        return Unauthorized("El usuario no existe");
        //    }
        //    var passDesencriptado = _dataProtector.Unprotect(usuarioDB.Password);

        //    var deleteUsuario = new Usuario
        //    {
        //        Id = usuario.Id,
        //        Email = usuario.Email,
        //        Password = passDesencriptado
        //    };
        //    _context.Usuarios.Remove(deleteUsuario);
        //    _context.SaveChangesAsync();

        //    //_context.Remove(usuario);
        //    //await _context.SaveChangesAsync();
        //    //await _operacionesService.AddOperacion("Delete", "Libros1");

        //    return Ok();
        //}

                [HttpDelete("{email}")]
        public async Task<ActionResult> BorrarLibro(string email)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == email);

            if (usuario == null)
            {
                return NotFound("El usuario no existe");
            }

          
            
            _context.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("hash/nuevousuario")]
        public async Task<ActionResult> PostNuevoUsuarioHash([FromBody] DTOUsuario usuario)
        {
            var resultadoHash = _hashService.Hash(usuario.Password);
            var newUsuario = new Usuario
            {
                Email = usuario.Email,
                Password = resultadoHash.Hash,
                Salt = resultadoHash.Salt
            };

            await _context.Usuarios.AddAsync(newUsuario);
            await _context.SaveChangesAsync();

            return Ok(newUsuario);
        }

        [HttpPost("hash/checkusuario")]
        public async Task<ActionResult> CheckUsuarioHash([FromBody] DTOUsuario usuario)
        {
            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == usuario.Email);
            if (usuarioDB == null)
            {
                return Unauthorized("usuario no existe");
            }

            var resultadoHash = _hashService.Hash(usuario.Password, usuarioDB.Salt);
            if (usuarioDB.Password == resultadoHash.Hash)
            {
                return Ok();
            }
            else
            {
                return Unauthorized();
            }

        }

        // ACTIVIDAD 6 PARTE 2 AMPLIADA

        [HttpPost("CambiarPassword")]
        public async Task<ActionResult> CambiarPassword([FromBody] DTOCambiarPassword cambiarPassword)
        {
            var usuarioDB = await _context.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.Email == cambiarPassword.Email);
            if (usuarioDB == null)
            {
                return Unauthorized("Usuario no existe"); 
            }

            var resultadoHash = _hashService.Hash(cambiarPassword.Password, usuarioDB.Salt);
            if (usuarioDB.Password != resultadoHash.Hash)
            {
                return Unauthorized("Contraseña incorrecta"); 
            }

            // nuevo hash 
            var nuevoHash = _hashService.Hash(cambiarPassword.NuevoPassword);

            // se actualizan contraseña y el salt 
            usuarioDB.Password = nuevoHash.Hash;
            usuarioDB.Salt = nuevoHash.Salt;

            await _context.SaveChangesAsync();

            return Ok();
        }

        #region Enlace 1 minuto

        
        [HttpGet("/changepassword/{textoEnlace}")]
        public async Task<ActionResult> LinkChangePasswordHash(string textoEnlace)
        {
            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.EnlaceCambioPass == textoEnlace);
            if (usuarioDB == null)
            {
                return Unauthorized("Operación no autorizada");
            }

            var fechaCaducidad = usuarioDB.FechaEnvioEnlace.Value.AddMinutes(1);

            if (fechaCaducidad < DateTime.Now)
            {
                return Unauthorized("Operación no autorizada");
            }

            return Ok("Enlace correcto");
        }


        [HttpPost("hash/LinkDe1minuto")]
        public async Task<ActionResult> LinkChangePasswordHash2([FromBody] DTOUsuarioLinkChangePassword usuario)
        {
            var usuarioDB = await _context.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.Email == usuario.Email);
            if (usuarioDB == null)
            {
                return Unauthorized("Usuario no registrado");
            }

            if (usuarioDB.FechaEnvioEnlace.HasValue && usuarioDB.FechaEnvioEnlace.Value.AddMinutes(1) > DateTime.Now)
            {
                return BadRequest("Ya se ha enviado un enlace en el último minuto");
            }


            // Creamos un string aleatorio 
            Guid miGuid = Guid.NewGuid();
            string textoEnlace = Convert.ToBase64String(miGuid.ToByteArray());
            // Eliminar caracteres que pueden causar problemas
            textoEnlace = textoEnlace.Replace("=", "").Replace("+", "").Replace("/", "").Replace("?", "").Replace("&", "").Replace("!", "").Replace("¡", "");


            // guardar el enlace
            usuarioDB.FechaEnvioEnlace = DateTime.Now;


            usuarioDB.EnlaceCambioPass = textoEnlace;
            await _context.SaveChangesAsync();
            var ruta = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/changepassword/{textoEnlace}";
            return Ok(ruta);
        }



        [HttpPost("usuarios/changepassword")]
        public async Task<ActionResult> LinkChangePasswordHash([FromBody] DTOUsuarioChangePassword infoUsuario)
        {
            var usuarioDB = await _context.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.Email == infoUsuario.Email && x.EnlaceCambioPass == infoUsuario.Enlace);
            if (usuarioDB == null)
            {
                return Unauthorized("Operación no autorizada");
            }

            if (usuarioDB.FechaEnvioEnlace.Value.AddMinutes(1) < DateTime.Now)
            {
                return Unauthorized("Operación no autorizada");
            }

            var resultadoHash = _hashService.Hash(infoUsuario.Password);
            usuarioDB.Password = resultadoHash.Hash;
            usuarioDB.Salt = resultadoHash.Salt;
            usuarioDB.EnlaceCambioPass = null;
            usuarioDB.FechaEnvioEnlace = null;

            await _context.SaveChangesAsync();

            return Ok("Password cambiado con exito");
        }
        #endregion

    }
}

