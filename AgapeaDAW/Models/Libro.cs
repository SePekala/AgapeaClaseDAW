namespace AgapeaDAW.Models
{
    public class Libro
    {
        #region ...propiedades de clase libro.....
        //titulo, editorial, autores, Edicion, paginas, dimensiones, ididoma, isbn10, isbn13, resumen, precio
        public String IdCategoria { get; set; }
        public String ImagenLibro { get; set; }
        public string ImagenLibroBASE64 { get; set; }
        public String Titulo { get; set; }
        public String Editorial { get; set; }
        public String Autores { get; set; }
        public String Edicion { get; set; }
        public int NumeroPaginas { get; set; }
        public String Dimensiones { get; set; }
        public String Idioma { get; set; } = "Español";
        public String ISBN10 { get; set;}
        public String ISBN13 { get; set; }
        public String Resumen { get; set; }
        public Decimal Precio { get; set; }

        #endregion

        #region ....metodos clase libro....

        #endregion
    }
}
