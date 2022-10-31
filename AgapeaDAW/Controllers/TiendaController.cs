using AgapeaDAW.Models;
using AgapeaDAW.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgapeaDAW.Controllers
{
    public class TiendaController : Controller
    {
        #region ...propiedades de clase controlador TiendaController....
        private IBDAccess _servicioBD;
        #endregion


        public TiendaController(IBDAccess servicioDBInyectado)
        {
            this._servicioBD = servicioDBInyectado;
        }



        #region ...metodos de clase controlador TiendaController....

        #region 1-metodos de accion

        [HttpGet]
        public IActionResult RecuperaLibros(String id)
        {
            //metodos para recuperar libros de una determinada categoria...pasada como 3º segmento en la url
            //si el segmento esta vacio, se recuperaria las OFERTAS o NOVEDADES o...
            if (String.IsNullOrEmpty(id)) id = "2-10"; //<---por defecto yo cargo los de informatica

            List<Libro> listaLibros = this._servicioBD.RecuperaLibros(id);
            return View(listaLibros);

        }


        [HttpGet]
        public IActionResult MostrarDetallesLibro([FromQuery] String isbn13, [FromQuery] String titulo)
        {
            //recuperar de la bd el objeto Libro y pasarselo a la vista...
            return View();
        }
        #endregion

        #region 2-otros metodos
        #endregion

        #endregion

    }
}
