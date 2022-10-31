namespace AgapeaDAW.Models
{
    public class Direccion
    {
        #region ....propiedades clase direccion....
        /*
             calle, numero, edificio, piso, letra, cp, localidad, provincia, pais, IdDireccion
         */
        public String IdDireccion { get; set; }=Guid.NewGuid().ToString();
        public String Calle { get; set; }
        public int CP { get; set; }
        public Muncipio LocalidadDirecc { get; set; }
        public Provincia ProvinciaDirecc { get; set; }
        public String Pais { get; set; } = "España";
        public Boolean EsPrincipal { get; set; } = false;
        public Boolean EsFacturacion { get; set; } = false;

        #endregion

        #region ...metodos clase direccion....

        #endregion
    }
}
