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
        // GET api/apiproductos
        public IEnumerable<ApiProductos> Get()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var listado = ctx.Productos.Where(a => a.activo).Select(a => new ApiProductos
            {
                idProducto = a.idProducto,
                nombre = a.producto,
                presentacion = a.presentacion,
                marca = a.marca,
                categoriaId = a.categoriaId,
                categorias = a.Categorias.nombre
            });
            return listado;
        }

        // GET api/apiproductos/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/apiproductos
        public void Post([FromBody]string value)
        {
        }

        // PUT api/apiproductos/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/apiproductos/5
        public void Delete(int id)
        {
        }

        public class ApiProductos
        {
            internal int? categoriaId;
            internal string categorias;

            public int idProducto { get; set; }
            public string marca { get; set; }
            public string nombre { get; set; }
            public string presentacion { get; set; }
        }
    }
}
