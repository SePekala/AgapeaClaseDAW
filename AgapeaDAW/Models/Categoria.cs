namespace AgapeaDAW.Models
{
    public class Categoria
    {
        #region ...propiedades de la clase categoria...
        
        public String IdCategoria { get; set; } //<--- cat.raiz => subcategoria => subcategoria => ...
        public String NombreCategoria { get; set; } 

        #endregion

    }
}
