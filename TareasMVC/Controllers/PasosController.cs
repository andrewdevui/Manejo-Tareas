﻿using Microsoft.AspNetCore.Mvc;
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

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] PasoCrearDTO pasoCrearDTO)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var paso = await context.Pasos.Include(p => p.Tarea).FirstOrDefaultAsync(p => p.Id == id);

            if(paso is null)
            {
                return NotFound();
            }

            if(paso.Tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            paso.Descripcion = pasoCrearDTO.Descripcion;
            paso.Realizado = pasoCrearDTO.Realizado;

            await context.SaveChangesAsync();

            return Ok();

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var paso = await context.Pasos.Include(p => p.Tarea).FirstOrDefaultAsync(p => p.Id == id);
            if (paso is null)
            {
                return NotFound();
            }
            if (paso.Tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }
            context.Remove(paso);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("ordenar/{tareaId:int}")]
        public async Task<IActionResult> Ordenar(int tareaId, [FromBody] Guid[] ids)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var tarea = await context.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId && t.UsuarioCreacionId == usuarioId);

            if(tarea is null)
            {
                return NotFound();
            }

            var pasos = await context.Pasos.Where(p => p.TareaId == tareaId).ToListAsync();

            var pasosIds = pasos.Select(p => p.Id);
            var idsPasosNoPertenecientes = ids.Except(pasosIds).ToList();

            if (idsPasosNoPertenecientes.Any())
            {
                return BadRequest("No todos los pasos están presentes");
            }

            var pasosDiccionario = pasos.ToDictionary(p => p.Id);

            for(int i = 0; i < ids.Length; i++)
            {
                var pasoId = ids[i];
                var paso = pasosDiccionario[pasoId];
                paso.Orden = i + 1;
            }

            await context.SaveChangesAsync();
            return Ok();

        }

    }
}
