﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class TanoNEEntities : DbContext
    {
        public TanoNEEntities()
            : base("name=TanoNEEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<C_Comentarios> C_Comentarios { get; set; }
        public virtual DbSet<Categorias> Categorias { get; set; }
        public virtual DbSet<Circuitos> Circuitos { get; set; }
        public virtual DbSet<Comentarios> Comentarios { get; set; }
        public virtual DbSet<ComentariosProducto> ComentariosProducto { get; set; }
        public virtual DbSet<CompraProducto> CompraProducto { get; set; }
        public virtual DbSet<Compras> Compras { get; set; }
        public virtual DbSet<Costos> Costos { get; set; }
        public virtual DbSet<EstadosCompra> EstadosCompra { get; set; }
        public virtual DbSet<Locales> Locales { get; set; }
        public virtual DbSet<Noticias> Noticias { get; set; }
        public virtual DbSet<Precios> Precios { get; set; }
        public virtual DbSet<Productos> Productos { get; set; }
        public virtual DbSet<ProductosLocales> ProductosLocales { get; set; }
        public virtual DbSet<Relevamientos> Relevamientos { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<RolesVecinos> RolesVecinos { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<Tandas> Tandas { get; set; }
        public virtual DbSet<Vecinos> Vecinos { get; set; }
    }
}
