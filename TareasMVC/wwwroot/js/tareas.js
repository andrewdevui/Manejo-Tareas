function agregarNuevaTareaAlListado() {
    tareaListadoViewModel.tareas.push(new tareaElementoListadoViewModel({ id: 0, titulo: '' }));

    $("[name=titulo-tarea]").last().focus();
}

async function manejarFocusoutTituloTarea(tarea) {
    const titulo = tarea.titulo();
    if (!titulo) {
        tareaListadoViewModel.tareas.pop();
        return;
    }

    const data = JSON.stringify(titulo);
    const respuesta = await fetch(urlTareas, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: data
    });

    if (respuesta.ok) {
        const json = await respuesta.json();
        tarea.id(json.id);
    } else {
        manejarErrorApi(respuesta);
    }
}

async function obtenerTareas() {
    tareaListadoViewModel.cargando(true);

    const respuesta = await fetch(urlTareas, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    if (!respuesta.ok) {
        manejarErrorApi(respuesta);
        tareaListadoViewModel.cargando(false);
        return;
    }

    const json = await respuesta.json();
    tareaListadoViewModel.tareas([]);

    json.forEach(valor => {
        tareaListadoViewModel.tareas.push(new tareaElementoListadoViewModel(valor));
    });

    tareaListadoViewModel.cargando(false);

}

async function actualizarOrdenTareas() {
    const ids = obtenerIdsTareas();
    await enviarIdsTareasAlBackend(ids);

    const arregloOrdenado = tareaListadoViewModel.tareas.sorted(function (a, b) {
        return ids.indexOf(a.id().toString()) - ids.indexOf(b.id().toString());
    });

    tareaListadoViewModel.tareas([]);
    tareaListadoViewModel.tareas(arregloOrdenado);

}

function obtenerIdsTareas() {
    const ids = $("[name=titulo-tarea]").map(function () {
        return $(this).attr("data-id");
    }).get();
    return ids;
}

async function enviarIdsTareasAlBackend(ids) {
    var data = JSON.stringify(ids);
    await fetch(`${urlTareas}/ordenar`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: data
    });

}

async function manejarClickTarea(tarea) {
    if (tarea.esNuevo()) {
        return;
    }
    const respuesta = await fetch(`${urlTareas}/${tarea.id()}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!respuesta.ok) {
        manejarErrorApi(respuesta);
        return;
    }

    const json = await respuesta.json();

    tareaEditarViewModel.id = json.id;
    tareaEditarViewModel.titulo(json.titulo);
    tareaEditarViewModel.descripcion(json.descripcion);

    tareaEditarViewModel.pasos([]);

    json.pasos.forEach(paso => {
        tareaEditarViewModel.pasos.push(
            new pasoViewModel({ ...paso, modoEdicion: false })
        )
    })

    modalEditarTareaBootstrap.show();
}

async function manejarCambioEditarTarea() {
    const obj = {
        id: tareaEditarViewModel.id,
        titulo: tareaEditarViewModel.titulo(),
        descripcion: tareaEditarViewModel.descripcion()
    };

    if (!obj.titulo) {
        return;
    }

    await editarTareaCompleta(obj);

    const indice = tareaListadoViewModel.tareas().findIndex(t => t.id() == obj.id);
    const tarea = tareaListadoViewModel.tareas()[indice];
    tarea.titulo(obj.titulo);

}

async function editarTareaCompleta(tarea) {
    const data = JSON.stringify(tarea);
    const respuesta = await fetch(`${urlTareas}/${tarea.id}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: data
    });
    if (!respuesta.ok) {
        manejarErrorApi(respuesta);
        throw "error";
    }

}

function intentarBorrarTarea(tarea) {
    modalEditarTareaBootstrap.hide();
    confirmarAccion({
        callBackAceptar: () => {
            borrarTarea(tarea);
        },
        callBackCancelar: () => {
            modalEditarTareaBootstrap.show();
        },
        titulo: `¿Desea borrar la tarea ${tarea.titulo()}?`
    })
}

async function borrarTarea(tarea) {
    const idTarea = tarea.id;
    const respuesta = await fetch(`${urlTareas}/${tarea.id}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (respuesta.ok) {
        const indice = obtenerIndiceTareaEnEdicion();
        tareaListadoViewModel.tareas.splice(indice, 1);
    }
}

function obtenerIndiceTareaEnEdicion() {
    return tareaListadoViewModel.tareas().findIndex(t => t.id() == tareaEditarViewModel.id);
}

function obtenerTareaEnEdicion() {
    const indice = obtenerIndiceTareaEnEdicion();
    return tareaListadoViewModel.tareas()[indice];
}

$(function () {
    $("#reordenable").sortable({
        axis: 'y',
        stop: async function () {
            await actualizarOrdenTareas();
        }
    })
})
