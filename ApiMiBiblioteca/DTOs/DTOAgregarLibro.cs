using ApiMiBiblioteca.Validators;

namespace ApiMiBiblioteca.DTOs
{
    public class DTOAgregarLibro
    {
        public string Isbn { get; set; }
        public string Titulo { get; set; }

        [PaginaValidacion]
        public int Paginas { get; set; }
        [PesoArchivoValidacion(PesoMaximoEnMegaBytes: 4)]
        [TipoArchivoValidacion(grupoTipoArchivo: GrupoTipoArchivo.Imagen)]
        public IFormFile FotoPortada { get; set; }
        public int AutorId { get; set; }
        public int EditorialId { get; set; }
        // [PrecioValidacion]
        public decimal Precio { get; set; }
    }
}
