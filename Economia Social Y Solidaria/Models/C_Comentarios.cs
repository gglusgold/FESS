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
    
    public partial class C_Comentarios
    {
        public int idComentario { get; set; }
        public string comentario { get; set; }
        public int estrellas { get; set; }
        public System.DateTime fecha { get; set; }
        public int vecinoId { get; set; }
        public Nullable<int> productoId { get; set; }
        public Nullable<int> compraId { get; set; }
    
        public virtual Vecinos Vecinos { get; set; }
    }
}
