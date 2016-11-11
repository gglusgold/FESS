using Economia_Social_Y_Solidaria.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Economia_Social_Y_Solidaria.Controllers
{

    public class Changuito
    {
        public int idProducto { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public decimal precio = 0;

        public int comentarios = 0;
        public double rating = 0;

        public int categoria { get; set; }

        public int stock { get; set; }
    }

    public class ChanguitoCompleta
    {
        public int totalCompraTandaUsuario = 0;
        public string proxFecha { get; set; }
        public List<Locales> locales = new List<Locales>();
        public List<Changuito> changuito = new List<Changuito>();
        public List<Cat> categorias = new List<Cat>();
    }

    public class Cat
    {
        public int idCategoria;
        public string nombre;
    }

    public class ProductosComprados
    {
        public int idProducto;
        public int cantidad;
        public string nombre;
        public bool comentado;
        public decimal precioUnidad;
    }

    public class Comprados
    {
        public int idCompra;
        public int comuna;
        public int idLocal;
        public string local;
        public string barrio;
        public string fecha;
        public bool editar = false;
        public bool comentar = false;
        public List<ProductosComprados> productos;
        public string estado;
        public bool comentado { get; set; }
    }

    public class MisCompras
    {
        public List<Comprados> Compras = new List<Comprados>();
    }

    public class ComprasController : Controller
    {
        public ActionResult Carrito(int idCategoria = 1)
        {

            ChanguitoCompleta completo = new ChanguitoCompleta();
            crearChango(completo, idCategoria);

            DateTime ProximaEntrea = GetNextWeekday(DateTime.Now, DayOfWeek.Saturday);
            completo.proxFecha = ProximaEntrea.ToShortDateString();



            return View(completo);
        }

        public ActionResult cambioCategoria(int idCategoria)
        {
            ChanguitoCompleta completo = new ChanguitoCompleta();
            crearChango(completo, idCategoria);

            DateTime ProximaEntrea = GetNextWeekday(DateTime.Now, DayOfWeek.Saturday);
            completo.proxFecha = ProximaEntrea.ToShortDateString();

            return Json(new { lista = completo.changuito });
        }

        public ChanguitoCompleta crearChango(ChanguitoCompleta completo, int idCategoria = -1)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Tandas ultima = ctx.Tandas.ToList().LastOrDefault();
            if (ultima != null && ultima.fechaCerrado == null)
                completo.locales = ultima.Circuitos.Locales.Where(a => a.activo).ToList();

            completo.categorias = ctx.Categorias.Where(a => a.Productos.Any(b => b.activo)).Select(a => new Cat { idCategoria = a.idCategoria, nombre = a.nombre }).OrderBy(a => a.nombre).ToList();

            if (User.Identity.IsAuthenticated && ultima != null)
            {
                EstadosCompra EstadoEntregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 1);

                Vecinos actual = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);
                completo.totalCompraTandaUsuario = ctx.Compras.Where(a => a.vecinoId == actual.idVecino && a.tandaId == ultima.idTanda && a.estadoId == EstadoEntregado.idEstadoCompra).Count();
            }

            Categorias cat = ctx.Categorias.FirstOrDefault(a => a.nombre == "Bolsones");
            cat = ctx.Categorias.FirstOrDefault(a => a.idCategoria == idCategoria);
            if (!cat.Productos.Any(a => a.activo))
            {
                int cate = completo.categorias.ToArray()[0].idCategoria;
                cat = ctx.Categorias.FirstOrDefault(a => a.idCategoria == cate);
            }


            ViewBag.categoria = cat.idCategoria;

            completo.changuito = ctx.Productos.Where(a => a.categoriaId == cat.idCategoria).ToList().Where(a => a.activo).Select(a => new Changuito()
            {
                idProducto = a.idProducto,
                stock = a.stock,
                nombre = a.producto + " - " + a.presentacion + (a.marca != null ? "\n" + a.marca : ""),
                descripcion = a.descripcion == null ? "" : a.descripcion,//.Replace("\n", "<br/>"),
                precio = a.Precios.LastOrDefault().precio,
                comentarios = a.ComentariosProducto.Count,
                rating = a.ComentariosProducto.Count == 0 ? 0 : a.ComentariosProducto.Average(b => b.estrellas)
            }).ToList();

            return completo;
        }

        public ActionResult Detalle(int id)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Productos actual = ctx.Productos.FirstOrDefault(a => a.idProducto == id);

            return View(new Changuito()
            {
                idProducto = actual.idProducto,
                nombre = actual.producto,
                descripcion = actual.descripcion == null ? "" : actual.descripcion.Replace("\n", "<br/>"),
                precio = actual.Precios.LastOrDefault().precio,
                comentarios = actual.ComentariosProducto.Count,
                rating = actual.ComentariosProducto.Count == 0 ? 0 : actual.ComentariosProducto.Average(b => b.estrellas),
                categoria = actual.Categorias.idCategoria
            });
        }

        public ActionResult Historial()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);
            if (vecino == null)
                return RedirectToAction("Carrito", "Compras");

            EstadosCompra EstadoEntregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 1);
            EstadosCompra confirmado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 3);
            EstadosCompra comentado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 4);

            MisCompras compras = new MisCompras();
            compras.Compras = ctx.Compras.Where(a => a.vecinoId == vecino.idVecino).OrderByDescending(a => a.fecha).ToList().Select(a => new Comprados
            {
                idCompra = a.idCompra,
                estado = a.EstadosCompra.nombre,
                fecha = a.fecha.ToString("hh:mm dd/MM/yyyy"),
                idLocal = a.Locales.idLocal,
                local = a.Locales.direccion,
                barrio = a.Locales.barrio,
                editar = a.estadoId == EstadoEntregado.idEstadoCompra,
                comentar = a.estadoId == confirmado.idEstadoCompra,
                comentado = a.estadoId == comentado.idEstadoCompra,
                comuna = a.Locales.comuna,
                productos = a.CompraProducto.ToList().Select(b => new ProductosComprados
                {
                    idProducto = b.Productos.idProducto,
                    nombre = b.Productos.producto,
                    cantidad = b.cantidad,
                    comentado = a.Comentarios.Count == 1 ? a.Comentarios.FirstOrDefault().ComentariosProducto.FirstOrDefault(cp => cp.productoId == b.productoId).Productos != null : false,
                    precioUnidad = b.Productos.Precios.FirstOrDefault(precio => a.fecha > precio.fecha).precio
                }).ToList()
            }).ToList();

            return View(compras);
        }

        public ActionResult Entregar()
        {
            return View();
        }

        public ActionResult CancelarPedido(int idCompra)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);

            EstadosCompra EstadoEntregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 1);
            Compras cancelar = ctx.Compras.FirstOrDefault(a => a.idCompra == idCompra && vecino.idVecino == a.vecinoId);
            if (cancelar != null)
            {
                ctx.Compras.Remove(cancelar);
                ctx.SaveChanges();
            }

            MisCompras compras = new MisCompras();
            compras.Compras = ctx.Compras.Where(a => a.vecinoId == vecino.idVecino).ToList().Select(a => new Comprados
            {
                idCompra = a.idCompra,
                estado = a.EstadosCompra.nombre,
                fecha = a.fecha.ToString("hh:mm dd/MM/yyyy"),
                local = a.Locales.direccion,
                barrio = a.Locales.barrio,
                editar = a.estadoId == EstadoEntregado.idEstadoCompra,
                comuna = a.Locales.comuna,
                productos = a.CompraProducto.ToList().Select(b => new ProductosComprados
                {
                    idProducto = b.Productos.idProducto,
                    nombre = b.Productos.producto,
                    cantidad = b.cantidad,
                    precioUnidad = b.Productos.Precios.FirstOrDefault(precio => a.fecha > precio.fecha).precio
                }).ToList()
            }).ToList();

            return Json(compras);
        }

        public ActionResult ConfirmarPedido(int local, int[] idProducto, int[] cantidad, int? idCompra = null)
        {
            string error = null;

            TanoNEEntities ctx = new TanoNEEntities();

            Locales localCompro = ctx.Locales.FirstOrDefault(a => a.idLocal == local);
            if (localCompro == null)
                error = "No se indico en que local va a retirar lo pedido";

            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);
            if (vecino == null)
                error = "Hay que iniciar sesion para realizar un pedido";

            Tandas ultimaTanda = ctx.Tandas.ToList().LastOrDefault(a => a.fechaCerrado == null);
            if (ultimaTanda == null)
                error = "No hay circuitos abiertos en este momento";

            //Encargado
            EstadosCompra estado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 1);

            Compras compra = new Compras();
            if (idCompra.HasValue)
            {
                compra = ctx.Compras.FirstOrDefault(a => a.idCompra == idCompra.Value);
                compra.CompraProducto.ToList().ForEach(cs => ctx.CompraProducto.Remove(cs));
            }
            else
            {
                compra.fecha = DateTime.Now;
            }

            compra.Locales = localCompro;
            compra.Vecinos = vecino;
            compra.Tandas = ultimaTanda;
            compra.EstadosCompra = estado;

            if (!idCompra.HasValue)
                ctx.Compras.Add(compra);


            for (int x = 0; x < idProducto.Length; x++)
            {
                int prodActual = idProducto[x];
                int cantActual = cantidad[x];

                Productos prod = ctx.Productos.FirstOrDefault(a => a.idProducto == prodActual);
                if (prod.stock != -1)
                {
                    int stockrestante = prod.stock - cantActual;
                    if (stockrestante < 0)
                    {
                        error = string.Format("{0} del siguiente producto:<br/>{1} - {2} - {3}", prod.stock == 0 ? "No contamos con stock" : "Solo contamos con " + prod.stock + " articulos", prod.producto, prod.presentacion, prod.marca);
                        break;
                    }
                    else
                        prod.stock = stockrestante;
                }


                CompraProducto productos = new CompraProducto();
                productos.Productos = prod;
                productos.Compras = compra;
                productos.cantidad = cantidad[x];

                ctx.CompraProducto.Add(productos);
            }



            if (string.IsNullOrEmpty(error))
            {
                ctx.SaveChanges();
            }

            return Json(new { error = error }, JsonRequestBehavior.DenyGet);
        }

        public ActionResult Calificar(int idCompra, int[] idProducto, string[] tComentarios, int[] rating)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);

            EstadosCompra comentado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 4);

            Compras compra = ctx.Compras.FirstOrDefault(a => a.idCompra == idCompra);


            Comentarios comentario = new Comentarios();
            if (ctx.Comentarios.FirstOrDefault(a => a.vecinoId == vecino.idVecino && a.compraId == compra.idCompra) == null)
            {
                compra.EstadosCompra = comentado;

                comentario.Vecinos = vecino;
                comentario.fecha = DateTime.Now;
                comentario.Compras = compra;

                for (int x = 0; x < idProducto.Length; x++)
                {
                    int usar = idProducto[x];
                    Productos prod = ctx.Productos.FirstOrDefault(a => a.idProducto == usar);

                    ComentariosProducto cp = new ComentariosProducto();
                    cp.comentario = tComentarios[x];
                    cp.estrellas = rating[x];
                    cp.Productos = prod;
                    comentario.ComentariosProducto.Add(cp);
                }

                ctx.Comentarios.Add(comentario);
                ctx.SaveChanges();
                return Json(new { bien = true, idCompra = idCompra, comentario = comentado.nombre });
            }

            return Json(new { bien = false, mensaje = "No se puede comentar la misma compra 2 veces" });
        }

        public ActionResult ListaTanda()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);

            Tandas ultima = ctx.Tandas.ToList().LastOrDefault(a => a.fechaCerrado != null);
            var lista = ctx.Compras.Where(a => (a.tandaId == ultima.idTanda) && (vecino.localId == null ? vecino.comuna == a.Locales.comuna : vecino.localId == a.localId)).ToList().Select(a => new
            {
                idCompra = a.idCompra,
                nombre = a.Vecinos.nombres,
                productos = string.Join("<br/>", a.CompraProducto.ToList().Select(b => "<span class='idca' style='display:none'>[" + b.productoId + "|" + b.cantidad + "]</span>(" + b.cantidad + ") " + b.Productos.producto + " - " + b.Productos.marca  + " - " + b.Productos.presentacion)),
                precio = a.CompraProducto.ToList().Sum(b => b.cantidad * b.Productos.Precios.FirstOrDefault(precio => a.fecha > precio.fecha).precio),
                retiro = a.EstadosCompra.codigo
            });
            return Json(lista, JsonRequestBehavior.DenyGet);
        }

        public FileResult ExportarEncargado()
        {
            StringBuilder csv = new StringBuilder();
            string Columnas = string.Format("{0};{1};{2};{3};{4}", "N", "Nombre", "Productos", "Precio", "Local");
            csv.AppendLine(Columnas);

            decimal costoTotal = 0;
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);

            Tandas ultima = ctx.Tandas.ToList().LastOrDefault(a => a.fechaCerrado != null);
            var lista = ctx.Compras.Where(a => a.tandaId == ultima.idTanda && (vecino.localId == null ? vecino.comuna == a.Locales.comuna : vecino.localId == a.localId)).OrderBy(a => new { a.Locales.idLocal, a.Vecinos.nombres }).ToList().Select(a => new
            {
                idCompra = a.idCompra,
                nombre = a.Vecinos.nombres,
                productos = string.Join(" - ", a.CompraProducto.ToList().Select(b => "(" + b.cantidad + ") " + b.Productos.producto + " - " + b.Productos.marca + " - " + b.Productos.presentacion + " - " + b.Productos.Precios.FirstOrDefault(precio => a.fecha > precio.fecha).precio)),
                precio = a.CompraProducto.ToList().Sum(b => b.cantidad * b.Productos.Precios.FirstOrDefault(precio => a.fecha > precio.fecha).precio),
                retiro = a.EstadosCompra.codigo,
                local = a.Locales.direccion
            }).ToArray();

            for (int x = 0; x < lista.Count(); x++)
            {
                var compra = lista[x];
                string filas = string.Format("{0};{1};{2};${3};{4}", compra.idCompra, compra.nombre, compra.productos, compra.precio, compra.local);
                csv.AppendLine(filas);
            }

            using (MemoryStream memoryStream = new MemoryStream(Encoding.Default.GetBytes(csv.ToString())))
            {
                memoryStream.Position = 0;
                return File(memoryStream.ToArray() as byte[], "application/vnd.ms-excel", "Reporte");
            }


        }

        public ActionResult EnregarPedido(int idCompra, bool entregado, int[] ids = null, int[] cantidad = null)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);

            EstadosCompra nopaso = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 5);
            EstadosCompra entre = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 3);
            EstadosCompra confirmado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 2);
            Compras compra = ctx.Compras.FirstOrDefault(a => a.idCompra == idCompra && a.estadoId == confirmado.idEstadoCompra);

            if (ids == null)
                compra.EstadosCompra = entregado ? entre : nopaso;
            else
            {
                var productos = compra.CompraProducto;
                for ( int x = 0; x < ids.Count();  x++) {
                    var prod = productos.FirstOrDefault(a => a.productoId == ids[x]);
                    prod.cantidad = cantidad[x];

                    if (prod.cantidad == 0)
                        ctx.CompraProducto.Remove(prod);
                }
                compra.EstadosCompra = entregado ? entre : nopaso;
            }

            ctx.SaveChanges();

            return Json(new { error = false }, JsonRequestBehavior.DenyGet);
        }

        public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }
    }
}
