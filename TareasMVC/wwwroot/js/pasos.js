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