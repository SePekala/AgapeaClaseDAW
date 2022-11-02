using AgapeaDAW.Models.Interfaces;

using System.Net;
using System.Net.Mail;

namespace AgapeaDAW.Models
{
    public class EmailServerMailJetSINAPI : IClienteEmail
    {
        /*
          usando  las  clases del espacio de nombres System.Net,  nos vamos a conectar al servidor SMTP de MailJet: SmtpClient
         y luego vamos a construir un mensaje de correo con clase MailMessage y  despues lo mandamos al server de MailJet usando SmtpClient
         */
        public string UserId { get; set; } = "d20d2ba3b0aa190bca751ba10b90d2d0";
        public string Key { get; set; } = "1bbba28aaed493b8851d0858cae75a04";

        public bool EnviarEmail(string emailCliente, string subject, string cuerpoMensaje, string? ficheroAdjunto)
        {
            try
            {
                SmtpClient _clienteSMTP = new SmtpClient("in-v3.mailjet.com");
                _clienteSMTP.Credentials = new NetworkCredential(this.UserId, this.Key);

                MailMessage __mensajeAEnviar = new MailMessage("pmr.aiki@gmail.com", emailCliente);
                __mensajeAEnviar.Subject = subject;
                __mensajeAEnviar.IsBodyHtml = true;
                __mensajeAEnviar.Body = cuerpoMensaje;

                _clienteSMTP.Send(__mensajeAEnviar);
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }

        }
    }
}
