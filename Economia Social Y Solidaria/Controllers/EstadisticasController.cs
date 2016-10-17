using Economia_Social_Y_Solidaria.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Economia_Social_Y_Solidaria.Controllers
{
    public class Estadistica
    {
        public string Nombre;
        public string Gasto;
        public int Cuando;
    }


    public class EstadisticasController : Controller
    {

        public ActionResult Estadisticas()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            List<Estadistica> lista = new List<Estadistica>();

            lista = ctx.Compras.ToList().Select(com => new Estadistica
            {
                Nombre = com.Vecinos.nombres,
                Gasto = com.CompraProducto.Sum(cp => cp.Productos.Precios.FirstOrDefault(p => p.fecha < com.fecha).precio).ToString("0.##"),
                Cuando = com.fecha.Month
            }).ToList();

            return View(lista);
        }

    }
}
