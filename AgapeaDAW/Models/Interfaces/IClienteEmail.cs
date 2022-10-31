namespace AgapeaDAW.Models.Interfaces
{
    public interface IClienteEmail
    {
        public String UserId { get; set; }
        public String Key { get; set; }

        public Boolean EnviarEmail(String emailCliente, String subject, String cuerpoMensaje, String? ficheroAdjunto);
    }
}
