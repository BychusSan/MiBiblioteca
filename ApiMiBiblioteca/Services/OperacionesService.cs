using ApiMiBiblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiMiBiblioteca.Services
{
    public class OperacionesService
    {
        private readonly MiBibliotecaContext _context;
        private readonly IHttpContextAccessor _accessor;

        public OperacionesService(MiBibliotecaContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }

        public async Task AddOperacion(string operacion, string controller)
        {
            Operacione nuevaOperacion = new Operacione()
            {
                FechaAccion = DateTime.Now,
                Operacion = operacion,
                Controller = controller,
                Ip = _accessor.HttpContext.Connection.RemoteIpAddress.ToString()
            };

            await _context.Operaciones.AddAsync(nuevaOperacion);
            await _context.SaveChangesAsync();

            Task.FromResult(0);
        }


        public async Task<bool> TiempoDeEspera()

        {
            var ipPrueba = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            var ultimasOperaciones = await _context.Operaciones
            .Where(x => x.Ip == ipPrueba)
            .OrderByDescending(o => o.FechaAccion)
            .Take(2)
            .ToListAsync();

            var ultimaOperacion = ultimasOperaciones[0];

            var prueba =  (DateTime.Now - ultimaOperacion.FechaAccion).TotalSeconds >= 30;
            return prueba;

        }
        
    }
}
