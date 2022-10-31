using System.Text.Json;

namespace AgapeaDAW.Models
{
    public class APIRestMessage
    {
        //clase q mapea respuesta de cualquier peticion a servicio REST
        #region ...props de clase apirestmessage....

        public String Update_date { get; set; } = "";
        public int Size { get; set; }
        public List<JsonElement> Data { get; set; } = new List<JsonElement>();
        public String Warning { get; set; } = "";


        #endregion
    }
}
