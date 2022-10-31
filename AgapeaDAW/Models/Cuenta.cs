using System.ComponentModel.DataAnnotations;

namespace AgapeaDAW.Models
{
    public class Cuenta
    {

        #region ...propiedes de la clase cuenta....
        // login, password, email, imagenAvatar, activada
        public String IdCuenta { get; set; } = Guid.NewGuid().ToString();
        
        
        [Required(ErrorMessage ="*El login del usuario es obligatorio")]
        public String Login { get; set; } = "";


        [Required(ErrorMessage ="*Contraseña obligatoria")]
        [MinLength(4,ErrorMessage ="*La contraseña debe tener al menos 4 caracteres")]
        [MaxLength(50,ErrorMessage ="*La contraseña no debe exceder de 50 caracteres")]
        public String Password { get; set; } = "";



        [Required(ErrorMessage ="*Email obligatorio")]
        [EmailAddress(ErrorMessage ="*Formato de Email invalido")]
        public String Email { get; set; } = "";



        public String ImagenCuenta { get; set; } = ""; //<---nombre del fichero imagen almacenado fisicamente en el servidor, en directorio wwwroot/images/upload_images/...
        public String ImagenCuentaBASE64 { get; set; } = ""; //<----contenido del fichero imagen en base64 almacenado en la BD

        public Boolean CuentaActiva { get; set; } = false;

        #endregion

        #region ...metodos de la clase cuenta...

        #endregion
    }
}
