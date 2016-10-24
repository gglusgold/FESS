using Economia_Social_Y_Solidaria.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

        public JsonResult Lista(/*string order, int limit, int offset*/)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var lista = ctx.Productos/*.OrderBy(a => a.idProducto).Skip(offset).Take(limit)*/.ToList().Select(a => new
            {
                idProducto = a.idProducto,
                nombre = a.producto,
                marca = a.marca,
                descripcion = a.descripcion != null ? a.descripcion.Replace("\n", "<br/>") : "",
                cantidad = a.presentacion,
                idCategoria = a.categoriaId,
                categoria = a.Categorias.nombre,
                precio = a.Precios.LastOrDefault().precio,
                costo = a.Costos.LastOrDefault().costo,
                borrar = a.activo
            });

            //return Json(new { total = lista.Count(), rows = lista }, JsonRequestBehavior.DenyGet);
            return Json(lista, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Crear(string nombre, string marca, string descripcion, HttpPostedFileBase imagen_img, string cantidad, int idCategoria, decimal precio, decimal costo)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Productos item = ctx.Productos.FirstOrDefault(a => a.producto == nombre && a.marca == marca && a.presentacion == cantidad);
            if (item == null)
            {
                Categorias categoria = ctx.Categorias.FirstOrDefault(a => a.idCategoria == idCategoria);
                item = new Productos();
                item.producto = nombre;
                item.marca = marca;
                item.descripcion = descripcion;
                item.presentacion = cantidad;
                item.Categorias = categoria;
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

        public JsonResult Editar(int idProducto, string nombre, string marca, string descripcion, HttpPostedFileBase imagen_img, string cantidad, int idCategoria, decimal precio, decimal costo)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Productos item = ctx.Productos.FirstOrDefault(a => a.idProducto == idProducto);
            if (item != null)
            {
                if (ctx.Productos.FirstOrDefault(a => a.producto == nombre && a.marca == marca && a.presentacion == cantidad && a.idProducto != idProducto) != null)
                {
                    return Json(new { error = true, mensaje = "Ya existe otro producto de esas caracteristicas" }, JsonRequestBehavior.DenyGet);
                }

                Categorias categoria = ctx.Categorias.FirstOrDefault(a => a.idCategoria == idCategoria);
                item.marca = marca;
                item.descripcion = descripcion;
                item.presentacion = cantidad;
                item.Categorias = categoria;

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

        public JsonResult Borrar(int idProducto, bool activar)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Productos item = ctx.Productos.FirstOrDefault(a => a.idProducto == idProducto);
            if (item != null)
            {
                item.activo = activar;
                ctx.SaveChanges();

                string savedFileName = rutaImagen + "Producto-" + item.idProducto + ".jpg";
                //if (System.IO.File.Exists(savedFileName))
                //{
                //    System.IO.File.Delete(savedFileName);
                //}
            }
            else
            {
                return Json(new { error = true, mensaje = "No existe el producto" }, JsonRequestBehavior.DenyGet);
            }

            return Json(activar, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Muchos(HttpPostedFileBase archivo)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            string[] result = new StreamReader(archivo.InputStream).ReadToEnd().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string linea in result)
            {
                string[] usar = linea.Replace("\r", "").Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                string nombre = usar[0];
                string marca = usar[1];
                string presentacion = usar[2];
                int cate = int.Parse(usar[3]);
                decimal precio = decimal.Parse(usar[4], new NumberFormatInfo() { NumberDecimalSeparator = "," });
                decimal costo = decimal.Parse(usar[5], new NumberFormatInfo() { NumberDecimalSeparator = "," });

                Productos item = ctx.Productos.FirstOrDefault(a => a.producto == nombre && a.marca == marca && a.presentacion == presentacion);
                if (item == null)
                {
                    Categorias categoria = ctx.Categorias.FirstOrDefault(a => a.idCategoria == cate);
                    item = new Productos();
                    item.producto = nombre;
                    item.marca = marca;
                    item.presentacion = presentacion;
                    item.Categorias = categoria;
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
                }

            }

            ctx.SaveChanges();

            return Json(new { error = false, mensaje = "Producto grabado satisfactoriamente" }, JsonRequestBehavior.DenyGet);
        }

    }

}
