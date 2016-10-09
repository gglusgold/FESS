using Economia_Social_Y_Solidaria.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }

    public class ChanguitoCompleta
    {
        public int totalCompraTandaUsuario = 0;
        public List<Locales> locales = new List<Locales>();
        public List<Changuito> changuito = new List<Changuito>();
    }


    public class ProductosComprados
    {
        public int idProducto;
        public int cantidad;
        public string nombre;
        public decimal precioUnidad;
    }

    public class Comprados
    {
        public int comuna;
        public string local;
        public string barrio;
        public string fecha;
        public bool editar = false;
        public List<ProductosComprados> productos;
        public string estado;
    }

    public class MisCompras
    {
        public List<Comprados> Compras = new List<Comprados>();
    }

    public class ComprasController : Controller
    {
        public ActionResult Carrito()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            ChanguitoCompleta completo = new ChanguitoCompleta();

            Tandas ultima = ctx.Tandas.ToList().LastOrDefault();
            if (ultima != null && ultima.fechaCerrado == null)
                completo.locales = ultima.Circuitos.Locales.ToList();

            if (User.Identity.IsAuthenticated && ultima != null)
            {
                Vecinos actual = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);
                completo.totalCompraTandaUsuario = ctx.Compras.Where(a => a.vecinoId == actual.idVecino && a.tandaId == ultima.idTanda).Count();
            }

            completo.changuito = ctx.Productos.ToList().Where(a => a.activo && !a.esAlmacen).Select(a => new Changuito()
            {
                idProducto = a.idProducto,
                nombre = a.nombre,
                descripcion = a.descripcion.Replace("\n", "<br/>"),
                precio = a.Precios.LastOrDefault().precio
            }).ToList();

            return View(completo);
        }

        public ActionResult Detalle(int id)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Productos actual = ctx.Productos.FirstOrDefault(a => a.idProducto == id);

            return View(new Changuito()
            {
                idProducto = actual.idProducto,
                nombre = actual.nombre,
                descripcion = actual.descripcion.Replace("\n", "<br/>"),
                precio = actual.Precios.LastOrDefault().precio
            });
        }

        public ActionResult Historial()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);

            EstadosCompra EstadoEntregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 1);

            MisCompras compras = new MisCompras();
            compras.Compras = ctx.Compras.Where(a => a.vecinoId == vecino.idVecino).ToList().Select(a => new Comprados
                {
                    estado = a.EstadosCompra.nombre,
                    fecha = a.fecha.ToString("hh:mm dd/MM/yyyy"),
                    local = a.Locales.direccion,
                    barrio = a.Locales.barrio,
                    editar = a.estadoId == EstadoEntregado.idEstadoCompra,
                    comuna = a.Locales.comuna,
                    productos = a.CompraProducto.ToList().Select(b => new ProductosComprados
                    {
                        idProducto = b.Productos.idProducto,
                        nombre = b.Productos.nombre,
                        cantidad = b.cantidad,
                        precioUnidad = b.Productos.Precios.FirstOrDefault(precio => a.fecha > precio.fecha).precio
                    }).ToList()
                }).ToList();

            return View(compras);
        }

        public ActionResult ConfirmarPedido(int local, int[] idProducto, int[] cantidad)
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
            compra.fecha = DateTime.Now;
            compra.Locales = localCompro;
            compra.Vecinos = vecino;
            compra.Tandas = ultimaTanda;
            compra.EstadosCompra = estado;
            ctx.Compras.Add(compra);


            for (int x = 0; x < idProducto.Length; x++)
            {
                int prodActual = idProducto[x];
                int cantActual = cantidad[x];

                Productos prod = ctx.Productos.FirstOrDefault(a => a.idProducto == prodActual);

                CompraProducto productos = new CompraProducto();
                productos.Productos = prod;
                productos.Compras = compra;
                productos.cantidad = cantidad[x];

                ctx.CompraProducto.Add(productos);
            }



            if (string.IsNullOrEmpty(error))
                ctx.SaveChanges();

            return RedirectToAction("Historial", "Compras");
        }

    }
}
