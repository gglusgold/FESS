using Economia_Social_Y_Solidaria.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Economia_Social_Y_Solidaria.Controllers
{
    public class CategoriasController : Controller
    {
        //
        // GET: /Categorias/

        public JsonResult Lista()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var lista = ctx.Categorias.Select(a => new
            {
                DisplayText = a.nombre,
                Value = a.idCategoria
            });

            return Json(new { Options = lista });
        }

    }
}
