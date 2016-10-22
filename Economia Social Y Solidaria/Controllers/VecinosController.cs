using Economia_Social_Y_Solidaria.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Economia_Social_Y_Solidaria.Controllers
{
    public class VecinosController : Controller
    {
        //
        // GET: /Vecinos/

        public ActionResult Vecino()
        {
            return View();
        }

        public JsonResult Lista()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var lista = ctx.Vecinos.ToList().Select(a => new
            {
                idVecino = a.idVecino,
                nombres = a.nombres,
                correo = a.correo,
                telefono = a.telefono,
                comuna = a.comuna,
                administrador = a.RolesVecinos.Any(b => b.Roles.codigoRol == 3),
                contador = a.RolesVecinos.Any(b => b.Roles.codigoRol == 4),
                encargado = a.RolesVecinos.Any(b => b.Roles.codigoRol == 5),
            });

            return Json(lista, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Editar(int idVecino, bool administrador = false, bool contador = false, bool encargado = false)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vec = ctx.Vecinos.FirstOrDefault(a => a.idVecino == idVecino);

            Roles admin = ctx.Roles.FirstOrDefault(a => a.codigoRol == 3);
            Roles cont = ctx.Roles.FirstOrDefault(a => a.codigoRol == 4);
            Roles enc = ctx.Roles.FirstOrDefault(a => a.codigoRol == 5);

            if (administrador)
                ctx.RolesVecinos.Add(new RolesVecinos { Roles = admin, vecinoId = vec.idVecino });

            if (contador)
                ctx.RolesVecinos.Add(new RolesVecinos { Roles = cont, vecinoId = vec.idVecino });

            if (encargado)
                ctx.RolesVecinos.Add(new RolesVecinos { Roles = enc, vecinoId = vec.idVecino });

            ctx.SaveChanges();

            return Json(new { error = false, mensaje = "Vecine editado satisfactoriamente" }, JsonRequestBehavior.DenyGet);
        }

    }
}
