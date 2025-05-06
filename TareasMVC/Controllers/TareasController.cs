using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Entidades;
using TareasMVC.Models;
using TareasMVC.Servicios;

namespace TareasMVC.Controllers
{
    [Route("api/tareas")]
    public class TareasController: ControllerBase
    {
        private readonly AplicationDbContext context;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IMapper mapper;

        public TareasController(AplicationDbContext context, IServicioUsuarios servicioUsuarios, IMapper mapper)
        {
            this.context = context;
            this.servicioUsuarios = servicioUsuarios;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<TareaDTO>>> Get()
        {
            return BadRequest("No puedes hacer esto");
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tareas = await context.Tareas.Where(t=>t.UsuarioCreacionId == usuarioId).OrderBy(x=>x.Orden).ProjectTo<TareaDTO>(mapper.ConfigurationProvider).ToListAsync();
            return tareas;
        }

        [HttpPost]
        public async Task<ActionResult<Tarea>> Post([FromBody] string titulo)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var existenTareas = await context.Tareas.AnyAsync(x => x.UsuarioCreacionId == usuarioId);

            var ordenMayor = 0;
            if(existenTareas)
            {
                ordenMayor = await context.Tareas.Where(x => x.UsuarioCreacionId == usuarioId).Select(x=> x.Orden).MaxAsync();
            }

            var tarea = new Tarea
            {
                Titulo = titulo,
                Orden = ordenMayor + 1,
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacionId = usuarioId
            };

            context.Add(tarea);
            await context.SaveChangesAsync();

            return tarea;
        }

    }
}
