
//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------


namespace Economia_Social_Y_Solidaria.Models
{

using System;
    using System.Collections.Generic;
    
public partial class Costos
{

    public Costos()
    {

        this.CompraProducto = new HashSet<CompraProducto>();

    }


    public int idCosto { get; set; }

    public decimal costo { get; set; }

    public System.DateTime fecha { get; set; }

    public Nullable<int> productoId { get; set; }



    public virtual ICollection<CompraProducto> CompraProducto { get; set; }

    public virtual Productos Productos { get; set; }

}

}
