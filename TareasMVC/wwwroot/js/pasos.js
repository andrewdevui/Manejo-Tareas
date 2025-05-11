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
    }

}

async function manejarClickSalvarPaso(paso) {
    paso.modoEdicion(false);
    const esNuevo = paso.esNuevo();
    const idTarea = tareaEditarViewModel.id;
    const data = obtenerCuerpoPeticionPaso(paso);
    if (esNuevo) {
        await insertarPaso(paso, data, idTarea);
    } else {

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