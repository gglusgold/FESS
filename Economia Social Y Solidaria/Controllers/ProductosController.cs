using Economia_Social_Y_Solidaria.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Economia_Social_Y_Solidaria.Controllers
{
    public class ProductosController : Controller
    {
        private string rutaImagen = AppDomain.CurrentDomain.BaseDirectory + "Imagenes\\";
        //
        // GET: /Productos/

        public ActionResult Productos()
        {
            return View();
        }

        public JsonResult Lista()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var lista = ctx.Productos.Where(a => a.activo).ToList().Select(a => new
            {
                idProducto = a.idProducto,
                nombre = a.nombre,
                descripcion = a.descripcion.Replace("\n", "<br/>"),
                cantidad = a.cantidad,
                esAlmacen = a.esAlmacen ? 2 : 1,
                precio = a.Precios.LastOrDefault().precio,
                costo = a.Costos.LastOrDefault().costo,
            });
            return Json(lista, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Crear(string nombre, string descripcion, HttpPostedFileBase imagen_img, string cantidad, int esAlmacen, decimal precio, decimal costo)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Productos item = ctx.Productos.FirstOrDefault(a => a.nombre == nombre && a.activo);
            if (item == null)
            {
                item = new Productos();
                item.nombre = nombre;
                item.descripcion = descripcion;
                item.cantidad = cantidad;
                item.esAlmacen = esAlmacen == 2;
                item.activo = true;

                Precios pre = new Precios();
                pre.precio = precio;
                pre.fecha = DateTime.Now;
                item.Precios.Add(pre);

                Costos cos = new Costos();
                cos.costo = costo;
                cos.fecha = DateTime.Now;
                item.Costos.Add(cos);

                ctx.Productos.Add(item);
                ctx.SaveChanges();

                if (imagen_img != null)
                {
                    string savedFileName = rutaImagen + "Producto-" + item.idProducto + imagen_img.FileName.Substring(imagen_img.FileName.LastIndexOf("."));
                    imagen_img.SaveAs(savedFileName);
                }
            }
            else
            {
                return Json(new { error = true, mensaje = "Ya existe el producto" }, JsonRequestBehavior.DenyGet);
            }

            return Json(new { error = false, mensaje = "Producto grabado satisfactoriamente" }, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Editar(int idProducto, string nombre, string descripcion, HttpPostedFileBase imagen_img, string cantidad, int esAlmacen, decimal precio, decimal costo)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Productos item = ctx.Productos.FirstOrDefault(a => a.idProducto == idProducto);
            if (item != null)
            {
                item.descripcion = descripcion;
                item.cantidad = cantidad;
                item.esAlmacen = esAlmacen == 2;

                Precios pre = new Precios();
                pre.precio = precio;
                pre.fecha = DateTime.Now;
                item.Precios.Add(pre);

                Costos cos = new Costos();
                cos.costo = costo;
                cos.fecha = DateTime.Now;
                item.Costos.Add(cos);

                ctx.SaveChanges();

                if (imagen_img != null)
                {
                    string savedFileName = rutaImagen + "Producto-" + item.idProducto + imagen_img.FileName.Substring(imagen_img.FileName.LastIndexOf("."));
                    imagen_img.SaveAs(savedFileName);
                }
            }
            else
            {
                return Json(new { error = true, mensaje = "No existe el producto" }, JsonRequestBehavior.DenyGet);
            }

            return Json(new { error = false, mensaje = "Producto editado satisfactoriamente" }, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Borrar(int idProducto)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Productos item = ctx.Productos.FirstOrDefault(a => a.idProducto == idProducto);
            if (item != null)
            {
                item.activo = false;
                ctx.SaveChanges();

                string savedFileName = rutaImagen + "Producto-" + item.idProducto + ".jpg";
                if (System.IO.File.Exists(savedFileName))
                {
                    System.IO.File.Delete(savedFileName);
                }
            }
            else
            {
                return Json(new { error = true, mensaje = "No existe el producto" }, JsonRequestBehavior.DenyGet);
            }

            return Json(new { error = false, mensaje = "Producto editado satisfactoriamente" }, JsonRequestBehavior.DenyGet);
        }

    }
}
