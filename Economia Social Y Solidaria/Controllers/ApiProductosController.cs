using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Economia_Social_Y_Solidaria.Models;
using System.Web;

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

        [ActionName("Usuario")]
        public IHttpActionResult Usuario() // string correo, string pass)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            var form = HttpContext.Current.Request.Form;

            string correo = form["correo"];
            string pass = form["pass"];

            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == correo);
            if (vecino != null)
            {

                if (!vecino.verificado)
                {
                    return Json(new { Error = "Hay que verificar la cuenta. Revisá tu mail" });
                }

                pass = InicioController.GetCrypt(pass);
                if (vecino.contrasena == pass)
                {
                    EstadosCompra EstadoEntregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 1);
                    EstadosCompra confirmado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 3);
                    EstadosCompra comentado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 4);

                    //Precios ultimop = prod.Precios.Count > 1 ? prod.Precios.LastOrDefault(a => a.fecha.Date <= actual.fechaAbierto.Date) : prod.Precios.FirstOrDefault();


                    var json = ctx.Compras.Where(a => a.vecinoId == vecino.idVecino).OrderByDescending(a => a.fecha).ToList().Select(a => new
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
                        productos = a.CompraProducto.ToList().Select(b => new
                        {
                            idProducto = b.Productos.idProducto,
                            nombre = b.Productos.producto,
                            marca = b.Productos.marca,
                            presentacion = b.Productos.presentacion,
                            cantidad = b.cantidad,
                            comentado = a.ComentariosProducto.FirstOrDefault(c => c.productoId == b.productoId) != null,  // a.Comentarios.Count == 1 ? a.Comentarios.FirstOrDefault().ComentariosProducto.FirstOrDefault(cp => cp.productoId == b.productoId).Productos != null : false,
                            precioUnidad = b.Productos.Precios.Count > 1 ? b.Productos.Precios.LastOrDefault(precio => a.fecha.Date >= precio.fecha.Date).precio : b.Productos.Precios.FirstOrDefault().precio,

                        })
                    }).ToList();

                    var jsonvecino = new
                    {
                        idVecino = vecino.idVecino,
                        nombre = vecino.nombres,
                        telefono = vecino.telefono,
                        comuna = vecino.comuna
                    };

                    return Json(new { Historico = json, Vecino = jsonvecino });
                }
                else
                    return Json(new { Error = "Contraseña incorrecta" });
            }
            else
                return Json(new { Error = "No existe el usuario, para usar la plataforma es necesario registrarse " });

        }

        [ActionName("Pedir")]
        public IHttpActionResult Pedir([FromBody] int local, [FromBody] int[] idProducto, [FromBody] int[] cantidad)
        {
            string error = null;

            

            TanoNEEntities ctx = new TanoNEEntities();

            Locales localCompro = ctx.Locales.FirstOrDefault(a => a.idLocal == local);
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);
            if (vecino == null)
                error = "Hay que iniciar sesion para realizar un pedido";

            Tandas ultimaTanda = ctx.Tandas.ToList().LastOrDefault(a => a.fechaCerrado == null);
            if (ultimaTanda == null)
                error = "No hay circuitos abiertos en este momento";

            //Encargado
            EstadosCompra estado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 1);

            Compras compra = new Compras();
            //if (idCompra.HasValue)
            //{
            //    compra = ctx.Compras.FirstOrDefault(a => a.idCompra == idCompra.Value);
            //    compra.CompraProducto.ToList().ForEach(cs => ctx.CompraProducto.Remove(cs));
            //}
            //else
            //{
                compra.fecha = DateTime.Now;
            //}

            compra.Locales = localCompro;
            compra.Vecinos = vecino;
            compra.Tandas = ultimaTanda;
            compra.EstadosCompra = estado;

            //if (!idCompra.HasValue)
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

            return Json(new { error = error });
        }

    }
}
