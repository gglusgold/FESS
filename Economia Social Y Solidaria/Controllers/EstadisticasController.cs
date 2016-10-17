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
        public int ComunaNumero;
        public string Comuna;
        public int Compras;

    }

    public class ProductosVecino
    {
        public string Nombre;
        public string Vecino;
        public int Cantidad;

    }


    public class EstadisticasController : Controller
    {

        public ActionResult Estadisticas()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            List<Estadistica> lista = new List<Estadistica>();

            EstadosCompra Entregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 3);
            lista = ctx.Compras.Where(a => a.EstadosCompra.codigo >= Entregado.codigo).GroupBy(a => a.Locales.comuna).Select(b => new Estadistica { ComunaNumero = b.Key, Comuna = "Comuna " + b.Key, Compras = b.Count() }).ToList();

            return View(lista);
        }

        public JsonResult ProductoPorComuna(int comuna)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            EstadosCompra Entregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 3);
            var listado = ctx.CompraProducto.Where(a => a.Compras.EstadosCompra.codigo >= Entregado.codigo && a.Compras.Locales.comuna == comuna)
                .GroupBy(a => a.Productos.nombre)
                .Select(b => new { 
                    Nombre = b.Key,
                    Cantidad = b.Sum(a => a.cantidad)
                });


            var quienes = ctx.CompraProducto.Where(a => a.Compras.EstadosCompra.codigo >= Entregado.codigo && a.Compras.Locales.comuna == comuna)
                .GroupBy(x => new { x.Productos.nombre, x.Compras.Vecinos.nombres })
                .Select(b => new
                {
                    Producto = b.Key.nombre,
                    Vecino = b.Key.nombres,
                    Cantidad = b.Sum(a => a.cantidad)
                });


            return Json(new { listado = listado, quienes = quienes });
        }

    }
}
