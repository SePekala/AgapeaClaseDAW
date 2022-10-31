namespace AgapeaDAW.Models.Interfaces
{
    public interface IBDAccess
    {
        /*
         interface q define props y metodos q van a tener los servicios de acceso a datos(BD) en la aplicacion y q se
        van a inyectar como "servicio" por el modulo de inyeccion de dependencias(DI)
         */
        #region ...propieades del interface de acceso a datos....
        public String CadenaConexion { get; set; }

        #endregion


        #region ...metodos del interface de acceso a datos....

        #region 1-metodos ClienteController
        public Boolean RegistrarCliente(Cliente newcliente);
        public Cliente ComprobarCredenciales(String email, String password);
        public Boolean ActivarCuentaCliente(String idCuenta);
        public Cuenta RecuperarCuenta(String idCuenta);
        public Boolean OperarDireccion(Direccion direc, String operacion, String idcliente);
        public Boolean AlmacenaFichImagen(String idCuenta, String idCliente, String nombreFichero, String base64Img);
        #endregion


        #region 2-metodos TiendaController
        public List<Libro> RecuperaLibros(String idcategoria);
        public List<Categoria> RecuperaCategorias(String idCategoria);
        #endregion

        #endregion



    }
}
