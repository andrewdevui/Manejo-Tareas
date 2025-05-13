function manejarClickAgregarPaso() {

    const indice = tareaEditarViewModel.pasos().findIndex(p => p.esNuevo());
    if (indice !== -1) {
        return;
    }

    tareaEditarViewModel.pasos.push(
        new pasoViewModel({
            modoEdicion: true,
            realizado: false
        }));
    $("[name=txtPasoDescripcion]:visible").focus();
}

function manejarClickCancelarPaso(paso) {
    if (paso.esNuevo()) {
        tareaEditarViewModel.pasos.pop();
    } else {
        paso.modoEdicion(false);
        paso.descripcion(paso.descripcionAnterior);
    }

}

async function manejarClickSalvarPaso(paso) {
    paso.modoEdicion(false);
    const esNuevo = paso.esNuevo();
    const idTarea = tareaEditarViewModel.id;
    const data = obtenerCuerpoPeticionPaso(paso);

    const descripcion = paso.descripcion();
    if (!descripcion) {
        paso.descripcion(paso.descripcionAnterior);
        if (esNuevo) {
            tareaEditarViewModel.pasos.Pop();
        }
        return;
    }

    if (esNuevo) {
        await insertarPaso(paso, data, idTarea);
    } else {
        actualizarPaso(data, paso.id());
    }
}

async function insertarPaso(paso, data, idTarea) {
    const respuesta = await fetch(`${urlPasos}/${idTarea}`, {
        body: data,
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        }
    });
    if (respuesta.ok) {
        const jason = await respuesta.json();
        paso.id(jason.id);

        const tarea = obtenerTareaEnEdicion();

        tarea.pasosTotal(tarea.pasosTotal() + 1);
        if (paso.realizado()) {
            tarea.pasosRealizados(tarea.pasosRealizados() + 1);
        }

    } else {
        manejarErrorApi(respuesta);
    }
}


function obtenerCuerpoPeticionPaso(paso) {
    return JSON.stringify({
        descripcion: paso.descripcion(),
        realizado: paso.realizado()
    })
}

function manejarClickDescripcionPaso(paso) {
    paso.modoEdicion(true);
    paso.descripcionAnterior = paso.descripcion();

    $("[name=txtPasoDescripcion]:visible").focus();
}

async function actualizarPaso(data, id) {
    const respuesta = await fetch(`${urlPasos}/${id}`, {
        body: data,
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        }
    });

    if (!respuesta.ok) {
        manejarErrorApi(respuesta);
    }
}

function manejarClickCheckboxPaso(paso) {
    if (paso.esNuevo()) {
        return true;
    }

    const data = obtenerCuerpoPeticionPaso(paso);
    actualizarPaso(data, paso.id());
    const tarea = obtenerTareaEnEdicion();

    let pasosRealizadosActual = tarea.pasosRealizados();

    if (paso.realizado()) {
        pasosRealizadosActual++;
    }
    else {
        pasosRealizadosActual--;
    }

    tarea.pasosRealizados(pasosRealizadosActual);

    return true;
}

function manejarClickBorrarPaso(paso) {
    modalEditarTareaBootstrap.hide();
    confirmarAccion({
        callBackAceptar: () => {
            borrarPaso(paso);
            modalEditarTareaBootstrap.show();
        },
        callBackCancelar: () => {
            modalEditarTareaBootstrap.show();
        },
        titulo: `¿Desea Borrar este paso?`
    })
}

async function borrarPaso(paso) {
    const respuesta = await fetch(`${urlPasos}/${paso.id()}`, {
        method: "DELETE"
    });
    if (!respuesta.ok) {
        manejarErrorApi(respuesta);
        return;
    }

    tareaEditarViewModel.pasos.remove(function (item) { return item.id() == paso.id() });

    const tarea = obtenerTareaEnEdicion();
    tarea.pasosTotal(tarea.pasosTotal() - 1);

    if (paso.realizado()) {
        tarea.pasosRealizados(tarea.pasosRealizados() - 1);
    }

}

async function actualizarOrdenPasos() {
    const ids = obtenerIdsPasos();
    await enviarIdsPasosBackend(ids);

    const arregloOrganizado = tareaEditarViewModel.pasos.sorted(function (a, b) {
        return ids.indexOf(a.id().toString()) - ids.indexOf(b.id().toString());
    })

    tareaEditarViewModel.pasos(arregloOrganizado);
}

function obtenerIdsPasos() {
    const ids = $("[name=chbPaso]").map(function () {
        return $(this).attr('data-id')
    }).get();
    return ids;
}

async function enviarIdsPasosBackend(ids) {
    var data = JSON.stringify(ids);
     await fetch(`${urlPasos}/ordenar/${tareaEditarViewModel.id}`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: data
    });
}


$(function () {
    $("#reordenable-pasos").sortable({
        axis: 'y',
        stop: async function () {
            await actualizarOrdenPasos();
        }
    })
})