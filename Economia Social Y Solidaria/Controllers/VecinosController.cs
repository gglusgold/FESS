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
                roles = a.RolesVecinos.Select(b => b.Roles.codigoRol),
                administrador = a.RolesVecinos.Any(b => b.Roles.codigoRol == 2),
                contador = a.RolesVecinos.Any(b => b.Roles.codigoRol == 3),
                encargado = a.RolesVecinos.Any(b => b.Roles.codigoRol == 4),
                noticias = a.RolesVecinos.Any(b => b.Roles.codigoRol == 5),
            });

            return Json(lista, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Editar(int idVecino, bool? administrador = null, bool? contador = null, bool? encargado = null, bool? noticias = null)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vec = ctx.Vecinos.FirstOrDefault(a => a.idVecino == idVecino);

            Roles admin = ctx.Roles.FirstOrDefault(a => a.codigoRol == 2);
            Roles cont = ctx.Roles.FirstOrDefault(a => a.codigoRol == 3);
            Roles enc = ctx.Roles.FirstOrDefault(a => a.codigoRol == 4);
            Roles noti = ctx.Roles.FirstOrDefault(a => a.codigoRol == 5);

            //var rolesVecino = ctx.RolesVecinos.Where(a => a.vecinoId == vec.idVecino).ToList<RolesVecinos>();
            //rolesVecino.ForEach(cs => ctx.RolesVecinos.Remove(cs));

            if (administrador.HasValue && administrador.Value)
                ctx.RolesVecinos.Add(new RolesVecinos { Roles = admin, vecinoId = vec.idVecino });
            else if (administrador.HasValue && !administrador.Value)
                ctx.RolesVecinos.Remove(vec.RolesVecinos.FirstOrDefault(a => a.rolId == admin.idRol));

            if (contador.HasValue && contador.Value)
                ctx.RolesVecinos.Add(new RolesVecinos { Roles = cont, vecinoId = vec.idVecino });
            else if (contador.HasValue && !contador.Value)
                ctx.RolesVecinos.Remove(vec.RolesVecinos.FirstOrDefault(a => a.rolId == cont.idRol));

            if (encargado.HasValue && encargado.Value)
                ctx.RolesVecinos.Add(new RolesVecinos { Roles = enc, vecinoId = vec.idVecino });
            else if (encargado.HasValue && !encargado.Value)
                ctx.RolesVecinos.Remove(vec.RolesVecinos.FirstOrDefault(a => a.rolId == enc.idRol));

            if (noticias.HasValue && noticias.Value)
                ctx.RolesVecinos.Add(new RolesVecinos { Roles = noti, vecinoId = vec.idVecino });
            else if (noticias.HasValue && !noticias.Value)
                ctx.RolesVecinos.Remove(vec.RolesVecinos.FirstOrDefault(a => a.rolId == noti.idRol));

            ctx.SaveChanges();

            return Json(new { error = false, admin = administrador, contador = contador, encargado = encargado, noticias = noticias }, JsonRequestBehavior.DenyGet);
        }

    }
}
