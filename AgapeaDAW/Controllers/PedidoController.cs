using AgapeaDAW.Models;
using AgapeaDAW.Models.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Data.SqlClient;
using System.Text.Json;

namespace AgapeaDAW.Controllers
{
    public class PedidoController : Controller
    {
        #region ...propiedades de clase pedidocontroller...
        private IBDAccess _servicioBD;


        #endregion

        #region ...metodos de clase pedidocontroller...

        public PedidoController(IBDAccess servicioBDInyect)
        {
            this._servicioBD = servicioBDInyect; 
        }

        [HttpGet]
        public IActionResult AddLibroPedido(String id)
        {
            try
            {
                //comprobar si el libro existe o no dentro del pedidoactual de la variable de sesion cliente
                //si existe incrementamos cantidad, sino añadimos como elemento nuevo
                Cliente _cliente = JsonSerializer.Deserialize<Cliente>(HttpContext.Session.GetString("datoscliente"));

                int _posLibro = _cliente.PedidoActual.ElementosPedido.FindIndex((ItemPedido item)=>item.LibroItem.ISBN13 == id);

                if (_posLibro == -1)
                {
                    Libro _libro = this._servicioBD.RecuperaLibroISBN(id);
                    if(_libro != null)
                    {
                        _cliente.PedidoActual.ElementosPedido.Add(new ItemPedido { LibroItem = _libro , CantidadItem = 1});
                    }
                    else
                    {
                        throw new Exception("libro con isbn no existe...");
                    }
                }
                else
                {
                    _cliente.PedidoActual.ElementosPedido[_posLibro].CantidadItem += 1;
                }

                //actualizamos la variable de sesion

                HttpContext.Session.SetString("datoscliente", JsonSerializer.Serialize<Cliente>(_cliente));

                //redirigimos a MostrarPedido

                return RedirectToAction("MostrarPedido");
            }
            catch (Exception)
            {
                return Redirect("https://localhost:7179/Cliente/Login");
            }
        }

        [HttpGet]

        public async Task<IActionResult> MostrarPedido()
        {
            try
            {
                //pasar la variable sesion cliente a la vista y en el viewdata la lista de provincias para pintarlas en el form.de alta direccion nueva de envio
                Cliente _cliente = JsonSerializer.Deserialize<Cliente>(HttpContext.Session.GetString("datoscliente"));

                //tengo que cargar las provincias para pasarseslas a la vista... invoco servicio rest externo 
                //https://apiv1.geoapi.es/provincias?type=JSON&key=&sandbox=1
                //usando objeto clase HttpClient
                HttpClient _clienteREST = new HttpClient();
                APIRestMessage _resREST = await _clienteREST.GetFromJsonAsync<APIRestMessage>("https://apiv1.geoapi.es/provincias?type=JSON&key=&sandbox=1");
                //tengo que desserializar el array de JsonElement a objetos de tipo de provincia... se podria hacer en el modelo APIRestMessage
                //un metodo donde recibes el tipo de objeto q hay dentro de ese .data y los deserializas
                List<Provincia> _provincias = new List<Provincia>();
                foreach (JsonElement item in _resREST.Data)
                {
                    _provincias.Add(JsonSerializer.Deserialize<Provincia>(item));
                }
                ViewData["provincias"] = _provincias;

                return View(_cliente);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]

        public IActionResult SumarCantidadLibro(String id)
        {
            try
            {
                Cliente? _cliente = JsonSerializer.Deserialize<Cliente>(HttpContext.Session.GetString("datoscliente") ?? "");
                //OJO!! SIEMPRE COMPROBAR Q EL ID VA UN ISBN13 DE UN LIBRO Q EXISTE EN EL PEDIDO, EN TEORIA ES ASI, PERO NO TIENE POR QUE
                _cliente.PedidoActual.ElementosPedido.Find((ItemPedido item) => item.LibroItem.ISBN13 == id).CantidadItem++;

                HttpContext.Session.SetString("datoscliente", JsonSerializer.Serialize<Cliente>(_cliente));

                return RedirectToAction("MostrarPedido");
            }
            catch (Exception ex)
            {
                if (ex.Source == "System.Text.Json") return RedirectToAction("Login", "Cliente");//ha caducado la variable de sesion
                HttpContext.Session.SetString("errores", ex.Message);//<-- nunca se muestran en las vistas los mensajes de excepcion, mostrar mejor "Ha habido un fallo interno en el servidor"
                return RedirectToAction("MostrarPedido");
                throw;
            }
        }

        [HttpGet]

        public IActionResult RestarCantidadLibro(String id)
        {
            try
            {
                Cliente? _cliente = JsonSerializer.Deserialize<Cliente>(HttpContext.Session.GetString("datoscliente") ?? "");
                //OJO!! SIEMPRE COMPROBAR Q EL ID VA UN ISBN13 DE UN LIBRO Q EXISTE EN EL PEDIDO, EN TEORIA ES ASI, PERO NO TIENE POR QUE

                int _posicionLibro = _cliente.PedidoActual.ElementosPedido.FindIndex((ItemPedido elem) => elem.LibroItem.ISBN13 == id);

                if (_posicionLibro == -1) throw new Exception("Libro con ISBN13 inexistente, no puedo quitar cantidad...");
                if (_cliente.PedidoActual.ElementosPedido[_posicionLibro].CantidadItem == 1)
                {
                    return RedirectToAction("MostrarPedido");
                }
                else
                {
                    _cliente.PedidoActual.ElementosPedido[_posicionLibro].CantidadItem -= 1;
                }

                HttpContext.Session.SetString("datoscliente",JsonSerializer.Serialize<Cliente>(_cliente));

                return RedirectToAction("MostrarPedido");
            }
            catch (Exception ex)
            {
                if (ex.Source == "System.Text.Json") return RedirectToAction("Login", "Cliente");//ha caducado la variable de sesion
                HttpContext.Session.SetString("errores", ex.Message);//<-- nunca se muestran en las vistas los mensajes de excepcion, mostrar mejor "Ha habido un fallo interno en el servidor"
                return RedirectToAction("MostrarPedido");
                throw;
            }
        }

        [HttpGet]

        public IActionResult EliminarPedidoLibro(String id)
        {
            try
            {
                Cliente? _cliente = JsonSerializer.Deserialize<Cliente>(HttpContext.Session.GetString("datoscliente") ?? "");
                //OJO!! SIEMPRE COMPROBAR Q EL ID VA UN ISBN13 DE UN LIBRO Q EXISTE EN EL PEDIDO, EN TEORIA ES ASI, PERO NO TIENE POR QUE

                int _posicionEliminar = _cliente.PedidoActual.ElementosPedido.FindIndex((ItemPedido elem) => elem.LibroItem.ISBN13 == id);
                _cliente.PedidoActual.ElementosPedido.Remove(_cliente.PedidoActual.ElementosPedido[_posicionEliminar]);
                //_cliente.PedidoActual.ElementosPedido.RemoveAt(_posicionEliminar);

                HttpContext.Session.SetString("datoscliente",JsonSerializer.Serialize<Cliente>(_cliente));

                return RedirectToAction("MostrarPedido");
            }
            catch (Exception ex)
            {
                if (ex.Source == "System.Text.Json") return RedirectToAction("Login", "Cliente");//ha caducado la variable de sesion
                HttpContext.Session.SetString("errores", ex.Message);//<-- nunca se muestran en las vistas los mensajes de excepcion, mostrar mejor "Ha habido un fallo interno en el servidor"
                return RedirectToAction("MostrarPedido");
                throw;
            }

        }

        [HttpPost]
        public IActionResult FinalizarPedido(Cliente _cliente,
                                             [FromForm] String direccionradios,
                                             [FromForm] String calle,
                                             [FromForm] String cp,
                                             [FromForm] String pais,
                                             [FromForm] String provincia,
                                             [FromForm] String municipio,
                                             [FromForm] String nombre,
                                             [FromForm] String apellidos,
                                             [FromForm] String email,
                                             [FromForm] String telefono,
                                             [FromForm] String otrosdatos,
                                             [FromForm] String datosfactura,
                                             [FromForm] String nombreEmpresa,
                                             [FromForm] String cifEmpresa,
                                             [FromForm] String pagoradios,
                                             [FromForm] String numerocard,
                                             [FromForm] String mescard,
                                             [FromForm] String aniocard,
                                             [FromForm] String cvv,
                                             [FromForm] String nombrebancocard)
        {
            // si la variable direccionradios==otradireccion tengo q crear una nueva direccion en el
            // cliente con los datos de calle,cp,pais,provincia,municipio meterla en la bd tabla direcciones y en variable sesion
            // modificar direccion de envio del pedido actual a esta nueva direccion



            // los datos de contacto para entregar el pedido: nombre,apellido, email, telefono y otrosdatos si son diferentes de los del cliente ... yo no hago nada con ellos,
            // se pueden almacenar en una tabla de la bd de PersonasContacto pasando el id de cliente de la variable de session

            // si datosfactura != null, es q quiere factura y puede valer "empresa" o "particular", esto influye en variables nombreEmpresa q si es un particular
            // es el nombre y apellido del particular y en cifEmpresa q si es un particular es un NIF

            // en pagoradios puede ir "pagopaypal" <--- usar api de paypal para el pago, "pagocard" <--- pago con tarjeta usando stripe, instalar NuGet: Stripe.net

            #region ...pago con stripe...

            StripeConfiguration.ApiKey = "sk_test_51M2dKCKBGYdXSACA4vwzykeAzQCxoqU69ADoRDdaeBaB4bOQUQNZG0rhrxg53zuQtRbV9eVvG45CwGi4NW6rxMsf00ViDKTGBs";
            //1º paso ---nos creamos un objeto Customer
            CustomerCreateOptions _optionsCustomer = new CustomerCreateOptions
            {
                Email = _cliente.Credenciales.Email,
                Name = _cliente.Nombre,
                Phone = _cliente.Telefono,
                Address = new AddressOptions
                {
                    //City = _direccion.LocalidadDirecc.DMUN50,
                    //Country = _direccion.Pais,
                    //PostalCode = _direccion.CP.ToString(),
                    //State = _direccion.ProvinciaDirecc.PRO,
                    //Line1 = _direccion.Calle
                },
                Metadata = new Dictionary<string, string> { { "id", _cliente.IdCliente }, { "fechaNacimiento", _cliente.FechaNacimiento.ToString() } }
            };
            Customer _customer = new CustomerService().Create(_optionsCustomer);
            //2º paso añadirmos tajeta de credito al cliente...primero necesitamos un token para la tarjeta y despues se la asociamos al cliente
            //2.1 -TOKEN card
            TokenCreateOptions _optionsCardToken = new TokenCreateOptions
            {
                Card = new TokenCardOptions
                {
                    Cvc = cvv,
                    Name = _cliente.Nombre + _cliente.Apellidos,
                    ExpMonth = mescard,
                    ExpYear = aniocard,
                    Number = numerocard
                }
            };
            Token _tokencard = new TokenService().Create(_optionsCardToken);

            //2.2 -CARD asociada al customer
            CardCreateOptions _cardOptions = new CardCreateOptions
            {
                Source = _tokencard.Id
            };
            Card _card = new CardService().Create(_customer.Id, _cardOptions);
            //3º paso, añadimos cargo a esa tarjeta del cliente con el importe del pedido OJO!! q se paga en centimos!!!
            ChargeCreateOptions _chargeoptions = new ChargeCreateOptions
            {
                Amount = System.Convert.ToInt64(_cliente.PedidoActual.TotalPedido) * 100,
                Currency = "eur",
                Customer = _customer.Id,
                Source = _card.Id,
                Description = _cliente.PedidoActual.IdPedido
            };
            Charge _cargopedido = new ChargeService().Create(_chargeoptions);
            if (_cargopedido.Status.ToLower() == "succeeded")
            {
                //meter en variable de sesion el _custormer.Id y _card.Id de stripe...para no tener q dar de alta continuamente al cliente y a la tarjeta
                //meter en la bd, tabla de pedidos el pedido actual y en direccion la nueva direccion por si la quiere usar mas adelante
                //actualizar variable de sesion cliente añadiendo el pedido al historico de pedidos
                return RedirectToRoute("panelclientes", new { controller = "Cliente", action = "MisPedidos", id = _cliente.PedidoActual.IdPedido });
            }
            else
            {
                throw new Exception("pago rechazado por la pasarela de pago, revisa los datos de tu tarjeta he intentalo de nuevo");
            }
            #endregion

            // si el cargo del importe del pedido se ha pasado ok...generar factura en pdf con paquete Nuget: IronPdf, enviar correo al cliente adjuntando la factura 
            // almacenar pedido en la BD  en tabla Pedidos, actualizar variable de sesion cliente y redirigir a Panel --> MisPedidos

            return RedirectToAction("RecuperaLibros", "Tienda");
        }

        #endregion

    }
}
