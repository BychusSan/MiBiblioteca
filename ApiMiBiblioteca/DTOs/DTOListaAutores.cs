namespace ApiMiBiblioteca.DTOs
{
    

    public class DTOListaAutores
    {
        public int IdAutor { get; set; }
        public string? Nombre { get; set; }
        public int TotalLibros { get; set; }
        public decimal? PromedioPrecio { get; set; }
        public List<DTOLibrosAutor>? Libro { get; set; }
    }

    public class DTOLibrosAutor
    {
        public string? ISBN { get; set; }
        public string? Titulo { get; set; }
        public decimal? Precio { get; set; }
    }
}