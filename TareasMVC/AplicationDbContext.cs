using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Entidades;

namespace TareasMVC
{
    public class AplicationDbContext: IdentityDbContext
    {
        public AplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)  //ApiFluente
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Tarea>()
            //    .Property(t => t.Titulo)
            //    .IsRequired()
            //    .HasMaxLength(250);
        }

        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<Paso> Pasos { get; set; }
        public DbSet<ArchivoAdjunto> ArchivosAdjuntos { get; set; }
    }
}
