using System;
using System.Collections.Generic;

namespace ApiMiBiblioteca.Models;

public partial class Libro
{
    public string Isbn { get; set; } = null!;

    public string? Titulo { get; set; }

    public int? Paginas { get; set; }

    public string? FotoPortada { get; set; }

    public bool? Descatalogado { get; set; }

    public int? AutorId { get; set; }

    public int? EditorialId { get; set; }

    public decimal? Precio { get; set; }

    public virtual Autore? Autor { get; set; }

    public virtual Editoriale? Editorial { get; set; }
}
