using System;
using System.Collections.Generic;

namespace ApiMiBiblioteca.Models;

public partial class Editoriale
{
    public int IdEditorial { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<Libro> Libros { get; set; } = new List<Libro>();
}
