using System.ComponentModel.DataAnnotations; //<------- espacio de nombres donde estan definidas las clases Validator para validar props. del modelo

namespace AgapeaDAW.Models
{
    public class Cliente
    {
        #region ....propiedades de la clase cliente....
        /*
             propiedades Nombre, Apellidos, Telefono, Cuenta, NIF
            Lista-Direcciones, Lista-Pedidos , lista-opiniones, listas-listadedeseos
        */
        public String IdCliente { get; set; } = Guid.NewGuid().ToString();
        
        //inicializamos estas propiedades para q a la hora del registro y meter los datos en la bd no salte excepcion en el insert por valores NULL
        [Required(ErrorMessage ="*Nombre obligatortio")]
        [MaxLength(50,ErrorMessage ="No se admiten mas de 50 caracteres")]
        public String Nombre { get; set; } = "";



        [Required(ErrorMessage = "*Apellidos obligatortios")]
        [MaxLength(50, ErrorMessage = "No se admiten mas de 300 caracteres")]
        public String Apellidos { get; set; } = "";




        [Required(ErrorMessage ="*Telefono de contacto obligatorio")]
        //[RegularExpression(@"^\d{3}(\s\d{2}){3}$",ErrorMessage ="*Formato invalido de tlfno, es asi: 666 11 22 33")]
        [Phone(ErrorMessage ="*Formato invalido de telefono")]
        public String Telefono { get; set; } = "";



        public DateTime FechaNacimiento { get; set; } = DateTime.Now;
        public String Genero { get; set; } = "";
        public String Descripcion { get; set; } = "";

        public String NIF { get; set; } = "";
        public Cuenta Credenciales { get; set; }
        public List<Direccion> MisDirecciones { get; set; }

        //public Dictionary<String,Direccion> MisDirecciones { get; set; }
        public List<Pedido> MisPedidos { get; set; }
        public Pedido PedidoActual { get; set; } //<---- objeto pedido con el q el cliente va a trabajar nada mas iniciar sesion
        #endregion

        public Cliente()
        {
            this.Credenciales = new Cuenta();
            this.MisDirecciones = new List<Direccion>();
            this.MisPedidos = new List<Pedido>();
            this.PedidoActual = new Pedido();
        }


        #region ....metodos de la clase cliente.....

        #endregion
    }
}
