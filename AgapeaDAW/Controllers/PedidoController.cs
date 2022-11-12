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
                _cliente.PedidoActual.CalcularTotalPedido();

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
                Cliente _cliente = JsonSerializer.Deserialize<Cliente>(HttpContext.Session.GetString("datoscliente"));

                int _posicionLibro = _cliente.PedidoActual.ElementosPedido.FindIndex((ItemPedido elem)=> elem.LibroItem.ISBN13 == id);

                if (_posicionLibro == -1)
                {
                    throw new Exception("No existe libro con ese isbn en el carrito");
                }
                else
                {
                    _cliente.PedidoActual.ElementosPedido[_posicionLibro].CantidadItem++;
                }

                HttpContext.Session.SetString("datoscliente", JsonSerializer.Serialize<Cliente>(_cliente));

                return RedirectToAction("MostrarPedido");
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]

        public IActionResult RestarCantidadLibro(String id)
        {
            try
            {
                Cliente _cliente = JsonSerializer.Deserialize<Cliente>(HttpContext.Session.GetString("datoscliente"));

                int _posicionLibro = _cliente.PedidoActual.ElementosPedido.FindIndex((ItemPedido elem) => elem.LibroItem.ISBN13 == id);

                if (_posicionLibro == -1)
                {
                    throw new Exception("No existe libro con ese isbn en el carrito");
                }
                else
                {
                    if(_cliente.PedidoActual.ElementosPedido[_posicionLibro].CantidadItem > 1)
                    {
                        _cliente.PedidoActual.ElementosPedido[_posicionLibro].CantidadItem -= 1;
                    }
                }

                HttpContext.Session.SetString("datoscliente",JsonSerializer.Serialize<Cliente>(_cliente));

                return RedirectToAction("MostrarPedido");
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]

        public IActionResult EliminarPedidoLibro(String id)
        {
            try
            {
                Cliente _cliente = JsonSerializer.Deserialize<Cliente>(HttpContext.Session.GetString("datoscliente"));

                int _posicionEliminar = _cliente.PedidoActual.ElementosPedido.FindIndex((ItemPedido elem) => elem.LibroItem.ISBN13 == id);
                _cliente.PedidoActual.ElementosPedido.Remove(_cliente.PedidoActual.ElementosPedido[_posicionEliminar]);
                //_cliente.PedidoActual.ElementosPedido.RemoveAt(_posicionEliminar);

                HttpContext.Session.SetString("datoscliente",JsonSerializer.Serialize<Cliente>(_cliente));

                return RedirectToAction("MostrarPedido");
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        [HttpPost]
        public IActionResult FinalizarPedido(Cliente datosCliente,
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
            if (pagoradios == "pagocard")
            {
                // 1º paso crear un objeto stripe de tipo Customer
                StripeConfiguration.ApiKey = "sk_test_51M2dKCKBGYdXSACA4vwzykeAzQCxoqU69ADoRDdaeBaB4bOQUQNZG0rhrxg53zuQtRbV9eVvG45CwGi4NW6rxMsf00ViDKTGBs";

                var nuevoCustomer = new CustomerCreateOptions
                {
                    Description = "Primer cliente de prueba",
                    Email = email,
                    Name = nombre,
                    Address =
                    {
                        Country= pais,
                        State= provincia,
                        City= municipio,
                        PostalCode= cp,
                        Line1=calle
                    },
                    Phone = telefono

                };
                var serviceCustomer = new CustomerService();
                serviceCustomer.Create(nuevoCustomer);

                // 2º paso, crearse un TokenCard para asociarlo a la tarjeta de credito q va a usar el cliente (objeto Customer de stripe) para el pago <---datos de la tarjeta: 
                // numero, fecha exp, cvv, nombre del propietario...
                // con ese TokenCard generar un objeto stripe de tipo Card <--- necesitas el Id del TokenCard y el Id del objeto Customer
                // para hacer pruebas usar numero de tarjeta: 4242 4242 4242 4242, fecha de exp posterior al año actual, y como cvv tres digitos cualesquiera

                var tokenCard = new TokenCreateOptions
                {
                    Card = new TokenCardOptions
                    {
                        Number = numerocard,
                        ExpMonth = mescard,
                        ExpYear = aniocard,
                        Cvc = cvv,
                    }
                };
                var serviceToken = new TokenService();
                serviceToken.Create(tokenCard);

                //3º crearse un objeto de tipo Charge (cargo) <-- le tienes q pasar entre otras cosas la cantidad a cobrar, tipo de divisa, etc

                var nuevoCargo = new ChargeCreateOptions
                {
                    Amount = 2000,
                    Currency = "eur",
                    Source = "tok_mastercard",
                    Description = "Pago de prueba",
                };
                var serviceCharge = new ChargeService();
                serviceCharge.Create(nuevoCargo);
            }


            #endregion

            // si el cargo del importe del pedido se ha pasado ok...generar factura en pdf con paquete Nuget: IronPdf, enviar correo al cliente adjuntando la factura 
            // almacenar pedido en la BD  en tabla Pedidos, actualizar variable de sesion cliente y redirigir a Panel --> MisPedidos

            return RedirectToAction("RecuperaLibros", "Tienda");
        }

        #endregion

    }
}
