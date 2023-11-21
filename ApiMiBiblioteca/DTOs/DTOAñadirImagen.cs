using ApiMiBiblioteca.Validators;

namespace ApiMiBiblioteca.DTOs
{
    public class DTOAñadirImagen
    {

        public string Isbn { get; set; }
   
        [PesoArchivoValidacion(PesoMaximoEnMegaBytes: 4)]
        [TipoArchivoValidacion(grupoTipoArchivo: GrupoTipoArchivo.Imagen)]
        public IFormFile FotoPortada { get; set; }

    }
}
