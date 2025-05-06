﻿async function manejarErrorApi(respuesta) {
    let mensajeError = '';
    if (respuesta.status === 400) {
        mensajeError = await respuesta.text();
    } else if (respuesta.status === 404) {
        mensajeError = recursoNoEncontrado;
    } else {
        mensajeError = errorInesperado;
    }
    mostrarMensajeError(mensajeError);
}

function mostrarMensajeError(Mensaje) {
    Swal.fire({
        icon: 'error',
        title: 'Error...',
        text: Mensaje
    });
}