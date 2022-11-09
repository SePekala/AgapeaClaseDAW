using AgapeaDAW.Models;
using AgapeaDAW.Models.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

                throw;
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
            return RedirectToAction("MostrarPedido");
        }

        [HttpGet]

        public IActionResult RestarCantidadLibro(String id)
        {
            return RedirectToAction("MostrarPedido");
        }

        [HttpGet]

        public IActionResult EliminarPedidoLibro(String id)
        {
            return RedirectToAction("MostrarPedido");
        }

        #endregion

    }
}
