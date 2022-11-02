using AgapeaDAW.Models;
using AgapeaDAW.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.IO;

namespace AgapeaDAW.Controllers
{
    public class ClienteController : Controller
    {
        //siempre q necesitemos solicitar la modulo de DI un servicio, se hace a traves del constructor y el objeto servicio q te devuelve
        //se encapsula en una prop. privada del controlador para q lo pueda manejar a nivel de clase

        #region ...propiedades clase ClienteController
        
        private IBDAccess _servicioBD;
        private IClienteEmail _servicioEmail;

        #endregion

        public ClienteController(IBDAccess servicioBDInyect, IClienteEmail servicioClienteEmail)
        {
            this._servicioBD = servicioBDInyect;
            this._servicioEmail = servicioClienteEmail;
        }





        #region ...metodos de clase ClienteController

        #region 1-metodos de accion del controlador(devuelven respuesta al navegador del cliente: vista, json, ....)


        #region------------------------------- metodos REGISTRO --------------------------------
        [HttpGet]
        public IActionResult Registro()
        {
            //asociamos la vista a un objeto de la clase Cliente q va a usarse como modelo -  Model Binding
            return View(new Cliente());
        }

        [HttpPost]
        public IActionResult Registro(Cliente nuevocliente,
                                     [FromForm] Boolean condUso,
                                     [FromForm] String repEmail, 
                                     [FromForm] String repPassword) {
            //una vez reciba los datos del formulario de registro, hay q validarlos...si estan ok
            //la clase Controller tiene una propiedad q es .ModelState <=== estado validacion del modelo
            //cuando un metodo de un controlador pregunta por el valor de esta prop. se produce la validacion de las props. del modelo
            //este ModelState es un dicionario clave-valor, donde la clave es la prop. a validar y el valor es el resultado de la validacion
            //para esa propiedad

            if (repEmail != nuevocliente.Credenciales.Email)
            {
                ModelState.AddModelError("Credenciales.Email", "*Los Emails no coinciden...");
            }

            if (repPassword != nuevocliente.Credenciales.Password) 
            {
                ModelState.AddModelError("Credenciales.Password", "*Las Contraseñas no coinciden...");
            }


            if (ModelState.IsValid) {
                // 1º - insertar datos en la BD en tabla clientes
                if (this._servicioBD.RegistrarCliente(nuevocliente))
                {
                    // 2º - mandar email a la cuanta del usuario para q active su cuenta
                    String __bodyEmail = $"<h3><strong>Se ha registrado correctamente en Agapea.com</strong></h3><br>Pulsa <a href='https://localhost:44359/Cliente/ActivarCuenta/{nuevocliente.Credenciales.IdCuenta}'>AQUI</a> para activar tu cuenta.";
                    this._servicioEmail.EnviarEmail(nuevocliente.Credenciales.Email, "Activa tu cuenta en Agapea.com", __bodyEmail,"");
                    // 3º - devolver vista de de comprobacion de correo o si hay errores devolver los errores...
                    return View("RegistroOK");

                }
                else
                {
                    ModelState.AddModelError("", "Error interno en el servidor, vuelva a intentarlo mas tarde..");
                    return View(nuevocliente); //mandar mensaje personalizado de "FALLO INTERNO DEL SERVIDOR, INTENTELO MAS TARDE"

                }


            }
            else //estado de validacion del modelo invalido....alguna propiedad esta MAL...
            {
                return View(nuevocliente);
            }

        }


        [HttpGet]
        public IActionResult RegistroOK() {
            return View();
        }

        [HttpGet]
        public IActionResult ActivarCuenta(String id)
        {
            //en parametro "id" va el tercer segmento de la routa /Cliente/ActivarCuenta/id-cuenta-cliente
            //usando el servico de acceso a datos, 1º comprobar q existe una cuenta con ese id, si existe actualizar
            //tabla Cuetnas y poner columna CuentaActivada a true, y si todo ok...redirigir al LOGIN
            if (this._servicioBD.ActivarCuentaCliente(id))
            {
                return RedirectToAction("Login");

            } else
            {
                //tengo q mandar el email de nuevo, necesito recuperar de la bd los datos de la cuenta
                Cuenta __cuentaCliente = this._servicioBD.RecuperarCuenta(id);
                if (__cuentaCliente != null) {
                    String __bodyEmail = $"<h3><strong>Se te ha enviado un NUEVO CORREO DE ACTIVACION de tu cuenta en Agapea.com</strong></h3><br>Pulsa <a href='https://localhost:44359/Cliente/ActivarCuenta/{__cuentaCliente.IdCuenta}'>AQUI</a> para activar tu cuenta.";
                    this._servicioEmail.EnviarEmail(__cuentaCliente.Email, "Activa tu cuenta en Agapea.com", __bodyEmail, "");

                    return RedirectToAction("Login");

                } else
                {
                    //no existe la cuenta con ese IdCuenta...q se registre
                    return RedirectToAction("Registro");
                }
            }
        }

        #endregion

        #region ----------------------------- metodos LOGIN ------------------------------------

        [HttpGet]
        public IActionResult Login()
        {
            return View(new Cuenta());
        }

        [HttpPost]
        public IActionResult Login(Cuenta cuenta)
        {
            /*
             OJO!!! no puedo preguntar por el estado de validacion de TODO el objeto CUENTA mapeado contra la vista, pq la prop. LOGIN
            como no esta asociada a ningun input del formulario nunca se va a validar bien...  ModelState.IsValid=FALSE siempre
            ¿q hago? pregunto por el estado de validacion de las propiedades q me interesen, email y password
             */
            
            //OJO V2.O!!!! HAY Q COMPROBAR Q LA CUENTA ESTA ACTIVADA!!!!!! SINO VOLVER A MANDAR EMAIL DE ACTIVACION....

            if (
                ModelState.GetValidationState("Email")==Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid &&
                ModelState.GetValidationState("Password") == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid
                )
            {
                //invocar al servicio de acceso a datos para comprobar credenciales
                Cliente _datosClienteBD=this._servicioBD.ComprobarCredenciales(cuenta.Email, cuenta.Password);
                if (_datosClienteBD==null)
                {
                    ModelState.AddModelError("", "*Email o Password incorrectas, intentelo de nuevo");
                    return View(cuenta);
                } else
                {
                    //si ok, almacenar el objeto Cliente en estado de sesion
                    //redirigimos al panel del cliente
                    HttpContext.Session.SetString("datoscliente", JsonSerializer.Serialize<Cliente>(_datosClienteBD));
                    return RedirectToAction("InicioPanel");

                }


            } else
            {
                return View(cuenta);
            }
        }



        //[HttpGet]
        //public IActionResult Login()
        //{
        //    return View(); //<---------- no mapeo la vista contra ningun modelo (del fichero Login.cshtml, quito @model Cuenta
        //                   // quito ademas el asp-validation-summary, los span con los asp-validation-for, y los atributos asp-for de los input

        //}

        //[HttpPost]
        //public IActionResult Login([FromBody] String email, [FromBody] String password) //<--- el parametro "email" debe coincidir con el nombre del atributo "name" del input type="email", para password igual
        //{
        //    //valido aqui con objetos RegExp si el argumento email es correcto o no y para el argumento password igual
        //}
        #endregion

        #region ---------------------- metodos PANEL USUARIO --------------------------

        [HttpGet]
        public async Task<IActionResult> InicioPanel()
        {
            //tengo q pasar a la vista los datos del cliente recuperados del estado de sesion...OJO!!! HAY Q COMPROBAR SIEMPRE Q NO SEAN NULL por si acaso
            //se han eliminado los datos del estado de sesion para ese cliente (cookie expirada), si pasara esto, se le envia al Login

            try
            {
                Cliente __clienteEstadoSession = JsonSerializer.Deserialize<Cliente>(HttpContext.Session.GetString("datoscliente"));
                
                ViewData["_meses"] = new Dictionary<String, String>()
                                {
                                    { "1", "Enero"},
                                    { "2", "Febrero"},
                                    { "3", "Marzo"},
                                    { "4", "Abril"},
                                    { "5", "Mayo"},
                                    { "6", "Junio"},
                                    { "7", "Julio"},
                                    { "8", "Agosto"},
                                    { "9", "Septiembre"},
                                    { "10", "Octubre"},
                                    { "11", "Noviembre"},
                                    { "12", "Diciembre"}
                                };
                //tengo q cargar las provincias para pasarselas a la vista...invoco servicio rest externo https://apiv1.geoapi.es/provincias?type=JSON&key=&sandbox=1
                //usando objeto clase HttpClient

                HttpClient _clienteREST = new HttpClient();
                APIRestMessage _resREST = await _clienteREST.GetFromJsonAsync<APIRestMessage>("https://apiv1.geoapi.es/provincias?type=JSON&key=&sandbox=1");

                //tengo q desserializar el array de JsonElement a objetos de tipo provincia...se podria hacer en el modelo APIRestMessage un metodo donde
                //recibes el tipo de objeto  q hay dentro de ese .data y los deserializas 

                List<Provincia> _provincias = new List<Provincia>();
                foreach (JsonElement item in _resREST.Data)
                {
                    _provincias.Add(JsonSerializer.Deserialize<Provincia>(item));
                }

                ViewData["provincias"] = _provincias;

                return View(__clienteEstadoSession);

            }
            catch (ArgumentNullException ex)
            {
                //la variable de sesion cliente ha desaparecido del estado de sesion...redirigimos al LOGIN
                return RedirectToAction("Login");
            }


        }

        [HttpPost]
        public IActionResult OperaDireccion([FromForm] String calle,
                                            [FromForm] String cp,
                                            [FromForm] String pais,
                                            [FromForm] String provincia,
                                            [FromForm] String municipio,
                                            [FromForm] String operacion
                                            )
        {
            //invocar metodo OperarDireccion del servicio de acceso a datos
            // si estoy borrando provincia="0", municipio="0" y el resto de datos null menos operacion...
            //para q no salte excepcion en el split de provincia y municipio:
            if(new Regex("^borrar_").IsMatch(operacion)) { provincia = "0-lalala"; municipio = "0-lolooo"; calle = ""; cp = "0"; pais = "lululu"; }

            Direccion _direc = new Direccion
            {
                Calle = calle,
                CP = System.Convert.ToInt32(cp),
                Pais = pais,
                ProvinciaDirecc = new Provincia { CCOM = "", CPRO = provincia.Split('-')[0], PRO = provincia.Split('-')[1] },
                LocalidadDirecc = new Muncipio { CUN = "", CPRO = provincia.Split('-')[0], CMUM = municipio.Split('-')[0], DMUN50 = municipio.Split('-')[1] }
            };
            if (new Regex("^(modificar_|borrar_)").IsMatch(operacion)) _direc.IdDireccion = operacion.Split('_')[1];

            Cliente _cliente = JsonSerializer.Deserialize<Cliente>(HttpContext.Session.GetString("datoscliente"));


            if(this._servicioBD.OperarDireccion(_direc, operacion.Split('_')[0], _cliente.IdCliente))
            {
                //si estoy actualizando borro direccion con ese id y  añado el nuevo objeto creado, es un poco ummmm...pq cuando sale la lista de direcciones en el panel
                //la direccion "modificada" saldria al final, no en la posicion en la q estaba
                if (new Regex("^(modificar_|borrar_)").IsMatch(operacion)) _cliente.MisDirecciones.RemoveAt(_cliente.MisDirecciones.FindIndex( (Direccion dir)=> dir.IdDireccion==_direc.IdDireccion));
                
                if(! new Regex("^borrar_").IsMatch(operacion)) _cliente.MisDirecciones.Add(_direc); //si no es borrar direccion añadimos direccion nueva o direccion modificada a lista de direcciones del cliente


                //actualizar el estado de sesion para q cargue las direcciones actualizadas (la q se crea nueva o la q se modifica)
                HttpContext.Session.SetString("datoscliente", JsonSerializer.Serialize<Cliente>(_cliente));
            }
            else
            {
                //meter en tempdata o bien en estado de sesion una prop. "mensajesErrorServer" con el contenido "ha habido un fallo interno a la hora de guardar los datos de la direccion"
            }


            return RedirectToAction("InicioPanel");
        }

        [HttpPost]
        public IActionResult UploadImagenFichero(IFormFile fichimagen)
        {
            String _nombreFichero = fichimagen.FileName;
            Cliente _cliente = JsonSerializer.Deserialize<Cliente>(HttpContext.Session.GetString("datoscliente"));
            //tengo q almacenar en el directorio /images/uploads_images/  la imagen q ha mandado el cliente con el nombre:  __nombre_fichero__idCliente.extension
            //para ello necesito un FileStream donde volcar el contenido de esa imagen
            String _nombreFichCliente = _nombreFichero.Split('.')[0] + "_" + _cliente.IdCliente + "." + _nombreFichero.Split('.')[1];

            FileStream _stream = new FileStream(
                                                Path.Combine(@"wwwroot\images\uploads_images",_nombreFichCliente),
                                                FileMode.Create
                                                );
            fichimagen.CopyTo(_stream);
            _stream.Close();

            //almacenamos tb el contenido del fich.imagen en base64 por si se borra el fichero fisico del server de  forma accidental
            byte[] _contenidoImagen = System.IO.File.ReadAllBytes(Path.Combine(@"wwwroot\images\uploads_images", _nombreFichCliente));
            String _base64Imagen = Convert.ToBase64String(_contenidoImagen);

            //tengo q concatenar al contenido en base64 del fichero el literal "data: tipo_imagen; base64, ..."
            //para q pueda mostrarse en el atributo src de un elemento <img ...>
            _base64Imagen = "data:" + fichimagen.ContentType + ";base64, " + _base64Imagen;

            //modifico tabla Cuentas en la BD para almacenar el nombre del fichero imagen en columna ImagenCuenta
            if (this._servicioBD.AlmacenaFichImagen(_cliente.Credenciales.IdCuenta, _cliente.IdCliente, _nombreFichCliente, _base64Imagen))
            {
                //actualizamos variable de sesion del cliente....
                _cliente.Credenciales.ImagenCuenta = _nombreFichero;
                _cliente.Credenciales.ImagenCuentaBASE64 = _base64Imagen; 

                HttpContext.Session.SetString("datoscliente", JsonSerializer.Serialize<Cliente>(_cliente));
                return Ok(
                            new
                            {
                                codigo = 0,
                                mensaje = "Imagen subida con exito",
                                otrosdatos = _nombreFichCliente
                            }
                         );

            }
            else
            {
                return Ok(
                            new
                            {
                                codigo = 1,
                                mensaje = "Fallo interno del servidor al intentar guardar la imagen, intentalo mas tarde",
                                otrosdatos = ""
                            }
                         );

            }

        }
        #endregion



        #endregion

        #region 2-metodos funcionales del controlador

        #endregion




        #endregion
    }
}
