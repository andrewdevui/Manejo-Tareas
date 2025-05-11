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
            //return BadRequest("No puedes hacer esto");
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tareas = await context.Tareas.Where(t=>t.UsuarioCreacionId == usuarioId).OrderBy(x=>x.Orden).ProjectTo<TareaDTO>(mapper.ConfigurationProvider).ToListAsync();
            return tareas;
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Tarea>> Get(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tarea = await context.Tareas.Include(t=>t.Pasos).FirstOrDefaultAsync(t=> t.Id == id && t.UsuarioCreacionId == usuarioId);
            if (tarea == null)
            {
                return NotFound();
            }
            return tarea;
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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarTarea(int id, [FromBody] TareaEditarDTO tareaEditarDTO)
        {
            var usuarioid = servicioUsuarios.ObtenerUsuarioId();
            var tarea = await context.Tareas.FirstOrDefaultAsync(t=>t.Id == id && t.UsuarioCreacionId == usuarioid);

            if(tarea is null)
            {
                return NotFound();
            }

            tarea.Titulo = tareaEditarDTO.Titulo;
            tarea.Descripcion = tareaEditarDTO.Descripcion;
            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuarioid = servicioUsuarios.ObtenerUsuarioId();

            var tarea = await context.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioCreacionId == usuarioid);

            if(tarea is null)
            {
                return NotFound();
            }

            context.Remove(tarea);

            await context.SaveChangesAsync();

            return Ok();

        }

        [HttpPost("ordenar")]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tareas = await context.Tareas.Where(x => x.UsuarioCreacionId == usuarioId).ToListAsync();

            var tareasId = tareas.Select(t => t.Id);

            var idsTareasNoPertenecenAlUsuario = ids.Except(tareasId).ToList();

            if(idsTareasNoPertenecenAlUsuario.Any())
            {
                return Forbid();
            }

            var tareasDiccionario = tareas.ToDictionary(x => x.Id);

            for(int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var tarea = tareasDiccionario[id];
                tarea.Orden = i;
            }

            await context.SaveChangesAsync();
            return Ok();

        }


    }
}
