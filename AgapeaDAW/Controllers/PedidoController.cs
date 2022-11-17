using AgapeaDAW.Models;
using AgapeaDAW.Models.Interfaces;
using IronPdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayPal.Api;
using Stripe;
using System.Data.SqlClient;
using System.Drawing;
using System.Text.Json;

namespace AgapeaDAW.Controllers
{
    public class PedidoController : Controller
    {
        #region ...propiedades de clase pedidocontroller...
        private IBDAccess _servicioBD;
        private IConfiguration _accesoappsettings;
        private IClienteEmail _clienteEmail;

        #endregion

        #region ...metodos de clase pedidocontroller...

        public PedidoController(IBDAccess servicioBDInyect,IConfiguration accesoappsettings, IClienteEmail clienteEmail)
        {
            this._servicioBD = servicioBDInyect;
            this._accesoappsettings = accesoappsettings;
            this._clienteEmail = clienteEmail;
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
        public IActionResult FinalizarPedido(Cliente formcliente,
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

            Cliente _cliente = JsonSerializer.Deserialize<Cliente>(HttpContext.Session.GetString("datoscliente"));

            // los datos de contacto para entregar el pedido: nombre,apellido, email, telefono y otrosdatos si son diferentes de los del cliente ... yo no hago nada con ellos,
            // se pueden almacenar en una tabla de la bd de PersonasContacto pasando el id de cliente de la variable de session

            // si datosfactura != null, es q quiere factura y puede valer "empresa" o "particular", esto influye en variables nombreEmpresa q si es un particular
            // es el nombre y apellido del particular y en cifEmpresa q si es un particular es un NIF

            // en pagoradios puede ir "pagopaypal" <--- usar api de paypal para el pago, "pagocard" <--- pago con tarjeta usando stripe, instalar NuGet: Stripe.net
            if (pagoradios == "pagocard")
            {
                /*
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
                #endregion*/

            }
            else
            {
                #region ...pago con PayPal...
                //el pago por paypal usa oAuth, protocolo de autentificacion externo
                /*
                         aplicacion asp.net core
                         pedidocontroller -------redirecciona (con el cargo ya predefinido)--------> PayPal login
                                          <---------------------------------------------------------
                                                            token-usuario, si login ok
                            token-usuario ------------------------cargo con id-pago----------------> cliente selecciona pago
                         cancel-url,accept-url                                                       ACEPTAR O bien puede CANCELAR
                                                                                                       ||                     ||-->paypal redirige a cancel-url
                                                                                              paypal redirige a accept-url
                 */
                //1º paso obtener AccesToken de paypal una vez q el usuario se autentifique correctamente <-- objeto clase OAuthTokenCredential
                //usando ese token, nos creamos un contexto de ejecucion de peticiones a la api paypal <-- objeto clase APIContext
                OAuthTokenCredential _accesToken = new OAuthTokenCredential(
                        this._accesoappsettings.GetSection("Paypal:ClienteId").Value,
                        this._accesoappsettings.GetSection("Paypal:SecretKey").Value,
                        new Dictionary<string, string>
                        {
                            { "mode", this._accesoappsettings.GetSection("Paypal:mode").Value},
                            { "business", this._accesoappsettings.GetSection("Paypal:BussinessSandboxAccount").Value }
                        }
                    );

                APIContext _apicontext = new APIContext(_accesToken.GetAccessToken());

                //2º paso pasamos a paypal el pedido con las url de aceptacion y posible cancelacion del mismo
                //los elementos del pedido se meten en un objeto clase ItemList de paypal. Tiene una propiedad, "items" q es la lista de objetos
                //Item de paypal para cada elemento del pedido
                //estos elementos del pedido, se meten en un objeto de tipo Payment

                ItemList _itemsPayPal = new ItemList { items = new List<Item>() };

                _cliente.PedidoActual
                        .ElementosPedido
                        .Select((ItemPedido item) => new Item { 
                                name=item.LibroItem.Titulo,
                                price=item.LibroItem.Precio.ToString().Replace(",","."),
                                currency="EUR",
                                quantity=item.CantidadItem.ToString(),
                                sku=item.LibroItem.ISBN13.ToString(),
                            }
                        )
                        .ToList<Item>()
                        .ForEach((Item itempaypal) => _itemsPayPal.items.Add(itempaypal));

                String _redirectURL = $"https://localhost:7179/Pedido/PayPalCallback/{_cliente.PedidoActual.IdPedido}";

                Payment _cargo = new Payment
                {
                    intent = "sale",
                    payer = new Payer { payment_method = "paypal" },
                    redirect_urls = new RedirectUrls { cancel_url = _redirectURL + "&Cancel=true", return_url = _redirectURL },
                    transactions = new List<Transaction>
                    {
                        new Transaction
                        {
                            description=$"Pedido de Agapea.com con id: {_cliente.PedidoActual.IdPedido} en fecha: {_cliente.PedidoActual.FechaPedido}",
                            invoice_number=_cliente.PedidoActual.IdPedido,
                            item_list=_itemsPayPal,
                            amount= new Amount//OJO! SEPARADOR DECIMAL EL ".", Y NO MAS DE 2 DECIMALES, total=subtotal+shipping
                                            {
                                                currency="EUR",
                                                details= new Details
                                                {
                                                    tax="0",
                                                    shipping=_cliente.PedidoActual.GastosEnvio.ToString().Replace(",","."),
                                                    subtotal=_cliente.PedidoActual.SubTotalPedido.ToString().Replace(",",".")
                                                },
                                                total=_cliente.PedidoActual.TotalPedido.ToString().Replace(",",".")
                                            }
                        }
                    }
                }.Create(_apicontext);

                //3º paso meto en variable de sesion el APIContent pq lo voy a necesitar en la url de vuelta de paypal para saber como
                //ha ido el pago y obtener detalles si quiero... tambien meto en variable de session el ID DEL CARGO DE PAYPAL
                //redirecciono al cliente a paypal para q pague, se hace usando detro de la variable Payment de paypal, hay una propiedad
                //que es links, q son enlaces a la api de paypal (es como un diccionario), hay un link "approval_url" q es el q tengo q usar
                //para redireccionar

                HttpContext.Session.SetString("cargoPayPal", _cargo.id);
                HttpContext.Session.SetString("apicontextpaypal", JsonSerializer.Serialize<APIContext>(_apicontext));

                String _urlPaypal = _cargo.links
                                        .Where((Links linkpaypal) => linkpaypal.rel.ToLower() == "approval_url")
                                        .Select((Links linkpaypal) => linkpaypal.href)
                                        .Single<String>();

                return Redirect(_urlPaypal);

                #endregion

            }


            // si el cargo del importe del pedido se ha pasado ok...generar factura en pdf con paquete Nuget: IronPdf, enviar correo al cliente adjuntando la factura 
            // almacenar pedido en la BD  en tabla Pedidos, actualizar variable de sesion cliente y redirigir a Panel --> MisPedidos

            return View();
        }


        [HttpGet]
        public IActionResult PayPalCallback([FromQuery] String PayerId,
                                             [FromQuery] String guid,
                                             [FromQuery] String Cancel = "false")
        {
            //recuperamos el apicontext de la variable de sesion
            APIContext _apiContext = JsonSerializer.Deserialize<APIContext>(HttpContext.Session.GetString("apicontextpaypal"));

            //si parametro Cancel="true" ha cancelado la compra el muy cabron...
            if (Convert.ToBoolean(Cancel))
            {
                HttpContext.Session.SetString("errores", "Ha habido un error en el pago con PayPal, puedes finalizar la compra de tu pedido mas tarde o usando una tarjeta de credito valida");
                return RedirectToAction("MostrarPedido");
            }
            //tengo que hacer efectivo el cargo aceptado por el cliente...usando objeto Payment
            String _cargoId = HttpContext.Session.GetString("cargoPayPal");
            Payment _pagoPedido = new Payment { id = _cargoId }.Execute(_apiContext, new PaymentExecution { payer_id = PayerId });

            switch (_pagoPedido.state)
            {
                case "approved":
                    //generar pdf de factura y mandar por email
                    //guardar direccion y datos de envio cuando ya hemos comprobado el pago
                    this.GenerarFacturaPDF(guid);

                    return RedirectToAction("FinalizarPedidoOK", new { id = guid });

                    break;

                case "failed":
                    HttpContext.Session.SetString("errores", "Ha habido un error en el pago con PayPal, puedes finalizar la compra de tu pedido mas tarde o usando una tarjeta de credito valida");
                    return RedirectToAction("MostrarPedido");
                        
                    break;

            }
            return View();
        }

        [HttpGet]
        public IActionResult FinalizarPedidoOK(String id)
        {
            //en id va el id del pedido finalizado...
            Cliente _cliente = JsonSerializer.Deserialize<Cliente>(HttpContext.Session.GetString("datoscliente"));
            Pedido _pedidoactual = _cliente.PedidoActual;

            //destruyo ya el pedidoactual y lo dejo vacio para q el cliente pueda seguir comprando con otro pedido...
            _cliente.MisPedidos.Add(_cliente.PedidoActual);
            _cliente.PedidoActual = new Pedido();

            //actualizo variable de sesion
            HttpContext.Session.SetString("datoscliente",JsonSerializer.Serialize<Cliente>(_cliente));

            return View(_pedidoactual);
        }
        #endregion

        #region ...metodos privados de la clase(no originan vistas)...
        private void GenerarFacturaPDF(String idpedido)
        {
            try
            {
                Cliente _cliente = JsonSerializer.Deserialize<Cliente>(HttpContext.Session.GetString("datoscliente"));

                String _items = "";
                _cliente.PedidoActual.ElementosPedido.ForEach(
                    (ItemPedido item) =>
                    {
                        decimal _subtotal = item.LibroItem.Precio * item.CantidadItem;

                        _items += "<tr>";
                        _items += $"<td>{item.LibroItem.Titulo}</td>";
                        _items += $"<td>{item.LibroItem.Precio.ToString()}</td>";
                        _items += $"<td>{item.CantidadItem}</td>";
                        _items += $"<td>{_subtotal.ToString()}</td>";
                        _items += "</tr>";
                    }    
                );

                String _factura = $@"
                    <div>
                        <h3><strong>RESUMEN DEL PEDIDO CON ID: {idpedido}</strong></h3> con fecha {_cliente.PedidoActual.FechaPedido.ToString()}
                    </div>
                    <hr/>
                    <div>
                        <p> A continuacion le mostramos un resumen detallado de su pedido en AGAPEA.COM</p>
                        <table>
                            <tr>                            
                                <td>Titulo</td>
                                <td>Precio</td>
                                <td>Cantidad</td>
                                <td>Subtotal</td>
                            </tr>
                            {_items}
                        </table>  
                    </div>
                    </hr> 

                    <div>
                        <p><strong>Subtotal del pedido: {_cliente.PedidoActual.SubTotalPedido} €</strong></p>
                        <p>Gastos de envio: {_cliente.PedidoActual.GastosEnvio} €</p>
                        <p><h3><strong>TOTAL del pedido: {_cliente.PedidoActual.TotalPedido} €</strong></h3></p>
                    </div>

                ";
                ChromePdfRenderer _renderIRONPDF = new IronPdf.ChromePdfRenderer();
                PdfDocument _facturaPDF = _renderIRONPDF.RenderHtmlAsPdf(_factura);

                _facturaPDF.SaveAs($"~/factura__{_cliente.IdCliente}__{idpedido}.pdf");
                this._clienteEmail.EnviarEmail(
                                               _cliente.Credenciales.Email, 
                                               "Pedido realizado correctamente en Agapea.com",
                                               _factura, 
                                               $"~/InfraEstructura/facturasPDF/factura__{_cliente.IdCliente}__{idpedido}.pdf"
                                               );
            }
            catch (Exception)
            {

                throw;
            }
            
        }


        #endregion

    }
}
