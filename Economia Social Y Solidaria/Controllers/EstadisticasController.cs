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
        public string Comuna;
        public int Compras;
        //public int Cuando;
    }


    public class EstadisticasController : Controller
    {

        public ActionResult Estadisticas()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            List<Estadistica> lista = new List<Estadistica>();


            EstadosCompra Entregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo ==3);

            lista = ctx.Compras.GroupBy(a => a.Locales.comuna).Select(b => new Estadistica { Comuna = "Comuna " + b.Key, Compras = b.Count() }).ToList();

            //var ls = ctx.Compras.Where(a => a.EstadosCompra.codigo > Entregado.codigo).GroupBy(a => a.Locales.comuna).Select(a =>
            //{

            //});
            /*lista = ctx.Compras.Where(a => a.EstadosCompra.codigo > Entregado.codigo).Select(com => new Estadistica
            {
                Comuna = "Comuna " + com.Locales.comuna,
                Compras = com.
            }).ToList();*/

            return View(lista);
        }

    }
}
