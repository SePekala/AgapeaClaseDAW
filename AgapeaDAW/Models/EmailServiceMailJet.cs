using AgapeaDAW.Models.Interfaces;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;

namespace AgapeaDAW.Models
{
    public class EmailServiceMailJet : IClienteEmail
    {
        public string UserId { get; set; } = "d9661f4dbfce2ac2ced9aec8e8a5dfcd";
        public string Key { get; set; } = "";

        public Boolean EnviarEmail(string emailCliente, string subject, string cuerpoMensaje, string? ficheroAdjunto)
        {
            MailjetClient __mailjetClient = new MailjetClient(this.UserId, this.Key);
            
            MailjetRequest __request=new MailjetRequest()
                                        .Property(Send.FromEmail, "pmr.aiki@gmail.com")
                                        .Property(Send.FromName, "adminAgapea")
                                        .Property(Send.Subject, subject)
                                        .Property(Send.TextPart, "")
                                        .Property(Send.HtmlPart, cuerpoMensaje)
                                        .Property(Send.Recipients, new JArray {
                                            new JObject {
                                             {"Email", emailCliente}
                                             }
                                            });
            
           MailjetResponse __respuestaEnvioEmail=__mailjetClient.PostAsync(__request).Result;
            return __respuestaEnvioEmail.IsSuccessStatusCode ;

        }
    }
}
