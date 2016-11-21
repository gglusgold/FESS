using Economia_Social_Y_Solidaria.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Economia_Social_Y_Solidaria.Controllers
{

    public class ConfCircuitos
    {
        public int idTanda;
        public int idCircuito;
        public int codigo;
        public string circuito;
        public string fechaAbierto;
        public string leyenda;
        public bool abrir = true;
        public string error;
    }


    public class TandasController : Controller
    {


        public ActionResult Tandas()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            List<Tandas> total = ctx.Tandas.ToList();


            ConfCircuitos conf = new ConfCircuitos();
            if (total.Count == 0)
            {
                conf.leyenda = "No existen tandas creadas";
                conf.abrir = true;
            }
            else
            {
                Tandas ultima = total.LastOrDefault(a => a.fechaCerrado == null);
                if (ultima == null)
                {
                    ultima = total.LastOrDefault();
                    conf.abrir = true;
                    conf.codigo = ultima.Circuitos.codigo;
                    conf.leyenda = "No hay tanda abierta, último abierto: " + ultima.Circuitos.nombre;
                }
                else
                {
                    conf.leyenda = "Circuito Abierto el " + ultima.fechaAbierto.ToShortDateString() + " por : " + ultima.Vecinos.nombres;
                    conf.idCircuito = ultima.Circuitos.idCircuito;
                    conf.abrir = false;
                }

                conf.idTanda = ultima.idTanda;
                conf.circuito = ultima.Circuitos.nombre;
                conf.fechaAbierto = ultima.fechaAbierto.ToShortDateString();

            }

            return View(conf);
        }

        public ActionResult Modificar(int codigo, bool abrir, int idTanda = -1)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            ConfCircuitos conf = new ConfCircuitos();

            Circuitos circuito = ctx.Circuitos.FirstOrDefault(a => a.codigo == codigo);
            Vecinos responsable = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);
            RolesVecinos rol = responsable.RolesVecinos.FirstOrDefault(a => a.Roles.codigoRol == 2);
            if (rol == null)
            {
                conf.error = "Su ususario no tiene permisos";
            }
            else
            {
                if (abrir)
                {
                    Tandas ultima = ctx.Tandas.ToList().LastOrDefault();
                    if (ultima != null && ultima.fechaCerrado == null)
                    {
                        conf.error = "No puede haber 2 tandas abiertas";
                    }
                    else
                    {
                        Tandas tanda = new Tandas();
                        tanda.fechaAbierto = DateTime.Now;
                        tanda.Circuitos = circuito;
                        tanda.Vecinos = responsable;
                        conf.abrir = false;
                        conf.circuito = tanda.Circuitos.nombre;
                        conf.codigo = tanda.Circuitos.codigo;
                        conf.idTanda = tanda.idTanda;
                        conf.idCircuito = tanda.Circuitos.idCircuito;
                        conf.leyenda = "Circuito Abierto: ";

                        ctx.Tandas.Add(tanda);
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    Tandas ultima = ctx.Tandas.ToList().LastOrDefault();
                    if (ultima != null && ultima.fechaCerrado != null)
                    {
                        conf.error = "Ya se encuentra cerrado";
                    }
                    else
                    {
                        if (idTanda == -1)
                            conf.error = "No se encontró la tanda";
                        else
                        {
                            Tandas tanda = ctx.Tandas.FirstOrDefault(a => a.idTanda == idTanda);
                            tanda.fechaCerrado = DateTime.Now;
                            tanda.Vecinos = responsable;

                            conf.leyenda = "No hay tanda abierta, último abierto:";
                            conf.abrir = true;
                            conf.idCircuito = ultima.Circuitos.idCircuito;
                            conf.codigo = ultima.Circuitos.codigo;

                            //CERRAR TODOS LOS PEDIDOS DE ESTA TANDA

                            EstadosCompra confirmado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 2);
                            foreach (var compraEnTanda in tanda.Compras)
                            {
                                compraEnTanda.EstadosCompra = confirmado;
                            }
                            ctx.SaveChanges();

                            MandarMailConfirmandoCompra(tanda);

                            ctx.SaveChanges();
                        }
                    }

                }
            }



            return RedirectToAction("Tandas", "Tandas");
        }

        public JsonResult Lista()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var lista = ctx.Tandas.OrderByDescending(a => a.idTanda).ToList().Select(a => new
            {
                idTanda = a.idTanda,
                fechaAbierto = a.fechaAbierto.ToShortDateString(),
                fechaCerrado = a.fechaCerrado.HasValue ? a.fechaCerrado.Value.ToShortDateString() : "-",
                circuito = a.Circuitos.nombre,
                responsable = a.Vecinos.nombres,
            });
            return Json(lista, JsonRequestBehavior.DenyGet);
        }

        private void MandarMailConfirmandoCompra(Tandas tandaActual)
        {
            DateTime ProximaEntrea = GetNextWeekday(DateTime.Now, DayOfWeek.Saturday);


            TanoNEEntities ctx = new TanoNEEntities();
            EstadosCompra confirmado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 2);
            var vecinosQueCompraron = tandaActual.Compras.Where(a => a.EstadosCompra.idEstadoCompra == confirmado.idEstadoCompra).GroupBy(a => a.vecinoId).Select(a => a.FirstOrDefault(b => b.vecinoId == a.Key));
            foreach (var actualTanda in vecinosQueCompraron)
            {
                string fecha = ProximaEntrea.ToShortDateString() + " - " + actualTanda.Locales.horario;
                string nombre = actualTanda.Vecinos.nombres;
                string correo = actualTanda.Vecinos.correo;

                using (MailMessage mail = new MailMessage())
                {

                    mail.From = new MailAddress("economiasocial@encuentrocapital.com.ar", "Economía Social y Solidaria");
                    mail.To.Add(correo);
                    //mail.To.Add("julianlionti@hotmail.com");
                    mail.Subject = "Economia Social y Solidaria -- Nuevo Encuentro";
                    mail.Body = "<p>Se han confirmado las siguientes compras</p>";
                    mail.BodyEncoding = System.Text.Encoding.UTF8;

                    List<Compras> totalCompras = ctx.Compras.Where(a => a.tandaId == actualTanda.tandaId && a.vecinoId == actualTanda.Vecinos.idVecino && a.EstadosCompra.idEstadoCompra == confirmado.idEstadoCompra).ToList();
                    foreach (var compras in totalCompras)
                    {
                        mail.Body += "<p>-------------------</p>";
                        mail.Body += "<p>Compra N° " + (totalCompras.IndexOf(compras) + 1) + "</p>";
                        mail.Body += "<p><b>Lo tenés que pasar a retirar el dia " + fecha + " Por nuestrno local en " + actualTanda.Locales.direccion + "</b></p>";

                        foreach (CompraProducto prod in compras.CompraProducto)
                        {
                            mail.Body += "<p>" + prod.Productos.producto + " - Cantidad: " + prod.cantidad + "</p>";
                        }
                        mail.Body += "<p>-------------------</p>";
                        mail.Body += "<br/><br/>";
                    }

                    mail.Body += "<p>Muchas gracias! Te esperamos</p>";
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("economiasocial@encuentrocapital.com.ar", "Frent3355");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
            }
        }

        public ActionResult VerCompras(bool admin = false)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);
            ViewBag.Admin = vecino.RolesVecinos.Any(a => a.Roles.codigoRol == 2) && admin;
            return View();
        }

        public JsonResult ListaCompras(bool ad)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);
            ad = vecino.RolesVecinos.Any(a => a.Roles.codigoRol == 2) && ad;

            Tandas ultimaAbierta = ctx.Tandas.FirstOrDefault(a => a.fechaCerrado == null);
            if (ultimaAbierta == null)
                ultimaAbierta = ctx.Tandas.ToList().Last();

            var lista = ctx.Compras.Where(a => (a.tandaId == ultimaAbierta.idTanda) && (ad ? a.localId > 0 : (vecino.localId == null ? vecino.comuna == a.Locales.comuna : vecino.localId == a.localId))).ToList().Select(a => new
            {
                idCompra = a.idCompra,
                vecine = a.Vecinos.nombres,
                productos = string.Join("<br/>", a.CompraProducto.Select(ab => string.Format("{0}: {1} {2} - {3}", ab.cantidad, ab.Productos.producto, ab.Productos.marca, ab.Productos.presentacion))),
                local = a.Locales.direccion,
                fecha = a.fecha.ToShortDateString()
            });

            return Json(new { Result = "OK", Records = lista }, JsonRequestBehavior.DenyGet);
        }

        public ActionResult CrearExcel(int idTanda, bool porLocal = false)
        {

            string handle = Guid.NewGuid().ToString();

            StringBuilder csv = new StringBuilder();
            string Columnas = null;
            if (porLocal)
                Columnas = ""; // string.Format("{0};{1};{2};{3}", "N", "Local", "Productos");
            else
                Columnas = string.Format("{0};{1};{2};{3}", "N", "Producto", "Presentacion", "Cantidad", "Costo(Aprox)");
            csv.AppendLine(Columnas);


            TanoNEEntities ctx = new TanoNEEntities();
            Tandas actual = ctx.Tandas.FirstOrDefault(a => a.idTanda == idTanda);

            if (porLocal)
            {
                decimal costoTotal = 0;
                var locales = ctx.Compras.Where(a => a.tandaId == idTanda).Select(a => a.Locales).Distinct().ToList();
                foreach  ( var local in locales ) {
                    csv.AppendLine(local.direccion);
                    var listado = ctx.CompraProducto.Where(a => a.Compras.localId == local.idLocal && a.Compras.tandaId == idTanda).GroupBy(a => a.productoId).Select(a => new { idProducto = a.Key, Cantidad = a.Sum(b => b.cantidad) }).ToArray();
                    for (int x = 0; x < listado.Count(); x++)
                    {
                        var compra = listado[x];
                        Productos prod = ctx.Productos.FirstOrDefault(a => a.idProducto == compra.idProducto);
                        decimal costo = prod.Costos.FirstOrDefault(a => a.fecha <= actual.fechaAbierto).costo * compra.Cantidad;
                        costoTotal += costo;
                        string filas = string.Format(";{0};{1};${2}", compra.Cantidad, string.Format("{0} - {1} - {2}", prod.producto, prod.marca, prod.presentacion), costo.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                        csv.AppendLine(filas);
                    }
                }
                string cierre = string.Format("Total;;;${0}", costoTotal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                csv.AppendLine(cierre);
            }
            else
            {
                var listado = ctx.CompraProducto.Where(a => a.Compras.tandaId == idTanda).GroupBy(a => a.productoId).Select(a => new { idProducto = a.Key, Cantidad = a.Sum(b => b.cantidad) }).ToArray();
                decimal costoTotal = 0;
                for (int x = 0; x < listado.Count(); x++)
                {
                    var compra = listado[x];
                    Productos prod = ctx.Productos.FirstOrDefault(a => a.idProducto == compra.idProducto);
                    decimal costo = prod.Costos.FirstOrDefault(a => a.fecha <= actual.fechaAbierto).costo * compra.Cantidad;
                    costoTotal += costo;
                    string filas = string.Format("{0};{1};{2};{3};${4}", x + 1, prod.producto, prod.presentacion, compra.Cantidad, costo.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                    csv.AppendLine(filas);
                }

                string cierre = string.Format("{0};{1};{2};{3};${4}", "", "", "", "", costoTotal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                csv.AppendLine(cierre);
            }



            using (MemoryStream memoryStream = new MemoryStream(Encoding.Default.GetBytes(csv.ToString())))
            {
                memoryStream.Position = 0;
                TempData[handle] = memoryStream.ToArray();
            }

            return Json(new { FileGuid = handle, FileName = "Reporte.csv" });

        }

        public ActionResult CrearExcelFinanzas(int idTanda)
        {

            string handle = Guid.NewGuid().ToString();

            StringBuilder csv = new StringBuilder();
            string Columnas = string.Format(";{0};{1};{2};{3};{4}", "Cantidad", "Producto", "Costo", "Precio", "Diferencia");
            csv.AppendLine(Columnas);


            TanoNEEntities ctx = new TanoNEEntities();
            Tandas actual = ctx.Tandas.FirstOrDefault(a => a.idTanda == idTanda);

            
            var locales = ctx.Compras.Where(a => a.tandaId == idTanda).Select(a => a.Locales).Distinct().ToList();
            foreach (var local in locales)
            {
                csv.AppendLine(local.direccion);
                decimal costoLocal = 0;
                decimal precioLocal = 0;
                var listado = ctx.CompraProducto.Where(a => a.Compras.localId == local.idLocal && a.Compras.tandaId == idTanda).GroupBy(a => a.productoId).Select(a => new { idProducto = a.Key, Cantidad = a.Sum(b => b.cantidad) }).ToArray();
                for (int x = 0; x < listado.Count(); x++)
                {
                    var compra = listado[x];
                    Productos prod = ctx.Productos.FirstOrDefault(a => a.idProducto == compra.idProducto);
                    decimal costo = prod.Costos.FirstOrDefault(a => a.fecha <= actual.fechaAbierto).costo * compra.Cantidad;
                    decimal precio = prod.Precios.FirstOrDefault(a => a.fecha <= actual.fechaAbierto).precio * compra.Cantidad;
                    costoLocal += costo;
                    precioLocal += precio;
                    string filas = string.Format(";{0};{1};${2};${3};${4}", compra.Cantidad, string.Format("{0} - {1} - {2}", prod.producto, prod.marca, prod.presentacion), costo.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), precio.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), (precio - costo).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                    csv.AppendLine(filas);
                }
                csv.AppendLine(string.Format(";;;${0};${1};${2}", costoLocal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), precioLocal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), (precioLocal - costoLocal).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)));
            }
            //string cierre = string.Format("Total;;;${0}", costoTotal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
            //csv.AppendLine(cierre);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.Default.GetBytes(csv.ToString())))
            {
                memoryStream.Position = 0;
                TempData[handle] = memoryStream.ToArray();
            }

            return Json(new { FileGuid = handle, FileName = "Reporte.csv" });

        }


        [HttpGet]
        public virtual ActionResult Descargar(string fileGuid, string fileName)
        {
            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];
                return File(data, "application/vnd.ms-excel", fileName);
            }
            else
            {
                // Problem - Log the error, generate a blank file,
                //           redirect to another controller action - whatever fits with your application
                return new EmptyResult();
            }
        }



        public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

    }
}
