using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Entidades;
using TareasMVC.Models;
using TareasMVC.Servicios;

namespace TareasMVC.Controllers
{
    [Route("api/pasos")]
    public class PasosController: ControllerBase
    {
        private readonly AplicationDbContext context;
        private readonly IServicioUsuarios servicioUsuarios;

        public PasosController(AplicationDbContext context, IServicioUsuarios servicioUsuarios)
        {
            this.context = context;
            this.servicioUsuarios = servicioUsuarios;
        }


        [HttpPost("{tareaId:int}")]
        public async Task<ActionResult<Paso>> Post(int tareaId, [FromBody] PasoCrearDTO pasoCrearDTO)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tarea = await context.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId);

            if(tarea == null)
            {
                return NotFound();
            }

            if(tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            var existenPasos = await context.Pasos.AnyAsync(t => t.TareaId == tareaId);

            var ordenMayor = 0;
            if(existenPasos)
            {
                ordenMayor = await context.Pasos.Where(t => t.TareaId == tareaId).Select(p=> p.Orden).MaxAsync();
            }

            var paso = new Paso();
            paso.TareaId = tareaId;
            paso.Descripcion = pasoCrearDTO.Descripcion;
            paso.Realizado = pasoCrearDTO.Realizado;
            paso.Orden = ordenMayor + 1;
            context.Add(paso);
            await context.SaveChangesAsync();

            return paso;
        }

    }
}
