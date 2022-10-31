namespace AgapeaDAW.Models
{
    public class Pedido
    {
        #region ...propiedades de la clase Pedido....
        //Subtotal, gastos de envio, Total, lista de libros + cantidades, fecha pedido, IdPedido, IdDireccionEnvio, IdDireccionFact
        public String IdPedido { get; set; } = Guid.NewGuid().ToString();
        public Decimal SubTotalPedido { get; set; } = 0;
        public Decimal GastosEnvio { get; set; } = 2;
        public Decimal TotalPedido { get; set; } = 0;
        public String IdDireccionEnvio { get; set; } = "";
        public String IdDireccionFacturacion { get; set; } = "";

        //public Dictionary<Libro,int> ElementosPedido {get; set;} <----- funciona, problema es lento y dificil
        public DateTime FechaPedido { get; set; }
        public List<ItemPedido> ElementosPedido { get; set; }
        #endregion


        public Pedido()
        {
            this.ElementosPedido = new List<ItemPedido>();
        }

        #region ...metodos de la clase Pedido...

        #endregion
    }
}
