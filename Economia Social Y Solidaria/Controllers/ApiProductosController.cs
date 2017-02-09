using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Economia_Social_Y_Solidaria.Models;

namespace Economia_Social_Y_Solidaria.Controllers
{
    public class ApiProductosController : ApiController
    {
        public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        // GET api/apiproductos
        [ActionName("Productos")]
        public IHttpActionResult Get(int idLocal)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            var categorias = ctx.ProductosLocales.Where(a => idLocal == -1 ? a.Locales.activo : a.localId == idLocal).ToList().Select(b => new
            {
                idProducto = b.Productos.idProducto,
                nombre = b.Productos.producto,
                marca = b.Productos.marca,
                presentacion = b.Productos.presentacion,
                precio = b.Productos.Precios.ToArray().Last().precio,
                stock = b.Productos.stock,
                idCategoria = b.Productos.categoriaId,
                categoria = b.Productos.Categorias.nombre
            });

            var groupedCustomerList = categorias
            .GroupBy(u => u.categoria)
            .Select(grp => new
            {
                nombre = grp.Key,
                productos = grp.ToList()
            })
            .ToList();

            //DateTime ProximaEntrea = GetNextWeekday(DateTime.Now, DayOfWeek.Saturday);
            //return Json(new { Proxima = ProximaEntrea.ToString("dd/MM/yyyy"), Lista = groupedCustomerList });
            return Json(groupedCustomerList);
        }


        [ActionName("Locales")]
        public IHttpActionResult Get()
        {
            TanoNEEntities ctx = new TanoNEEntities();

            var locales = ctx.ProductosLocales.Where(a => a.Locales.activo).ToList().Select(b => new
            {
                idLocal = b.Locales.idLocal,
                nombre = b.Locales.nombre,
                horario = b.Locales.horario,
                direccion = b.Locales.direccion,
                comuna = b.Locales.comuna,
                barrio = b.Locales.barrio,
            }).Distinct();

            DateTime ProximaEntrea = GetNextWeekday(DateTime.Now, DayOfWeek.Saturday);

            return Json(new { Proxima = ProximaEntrea.ToString("dd/MM/yyyy"), Lista = locales });

            //return Json(locales);
        }
    }
}
