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

function confirmarAccion({ callBackAceptar, callBackCancelar, titulo }) {
    Swal.fire({
        title: titulo || '¿Realmente deseas hacer esto?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí',
        focusConfirm: true
    }).then((resultado) => {
        if (resultado.isConfirmed) {
            callBackAceptar();
        } else if(callBackCancelar) {
            //El usuario ha presionado el botón de cancelar
            callBackCancelar();
        }
    })
}