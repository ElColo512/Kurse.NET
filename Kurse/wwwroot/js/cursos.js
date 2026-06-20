/*-------------------------------------------Modales-------------------------------------------*/
function initializeInscriptosModal(config) {

    $(document).on('click', config.buttonClass, function () {

        const id = $(this).data('id');

        $(config.modalId).data('current-id', id);

        $.get(`/Cursos/Inscriptos/${id}`)
            .done(function (html) {
                $(config.contentContainer).html(html);

                const modalElement = document.getElementById(config.modalId.replace('#', ''));
                let modal = bootstrap.Modal.getInstance(modalElement);

                if (!modal) {
                    modal = new bootstrap.Modal(modalElement);
                }

                modal.show();

                initInscriptosTable(id);
            })
            .fail(xhr => handleAjaxError(xhr, 'No se pudieron cargar los inscriptos.'));
    });
}

function initializeActaModal(config) {
    $(document).on('click', config.buttonClass, function () {

        const id = $(this).data('id');

        $(config.modalId).data('current-id', id);

        $.get(`/Cursos/Acta/${id}`)
            .done(function (html) {
                $(config.contentContainer).html(html);

                const modalElement = document.getElementById(config.modalId.replace('#', ''));
                let modal = bootstrap.Modal.getInstance(modalElement);

                if (!modal) {
                    modal = new bootstrap.Modal(modalElement);
                }

                modal.show();

                initActaTable(id);
            })
            .fail(xhr => handleAjaxError(xhr, 'No se pudieron cargar los inscriptos.'));
    });
}

function initializeReviewModal(config) {
    $(document).on('click', config.buttonClass, function (e) {
        e.preventDefault();

        const id = $(this).data('id');

        $.get(`${config.url}/${id}`)
            .done(function (html) {
                $(config.contentContainer).html(html);

                const modalElement = document.getElementById(config.modalId.replace('#', ''));
                let modal = bootstrap.Modal.getInstance(modalElement);

                if (!modal) {
                    modal = new bootstrap.Modal(modalElement);
                }

                modal.show();
            })
            .fail(xhr => handleAjaxError(xhr, 'No se pudo cargar la revisión.'));
    });
}

/*------------------------------------------DataTable------------------------------------------*/
function initInscriptosTable(cursoId) {
    if ($.fn.DataTable.isDataTable('#tablaInscriptos')) {
        $('#tablaInscriptos').DataTable().destroy();
    }

    initializeDataTable({
        tableId: 'tablaInscriptos',
        ajaxUrl: `/Cursos/GetInscriptos?cursoId=${cursoId}`,
        columns: [
            { data: 'nombreCompleto', className: 'text-center' },
            { data: 'email', className: 'text-center' },
            {
                data: 'fechaInscripcion', className: 'text-center',
                render: d => formatDate(d)
            }
        ]
    });
}

const estadoClasses = {
    'Inscripto': 'bg-secondary',
    'Regular': 'bg-primary',
    'Aprobado': 'bg-success',
    'Desaprobado': 'bg-danger',
    'Libre': 'bg-warning text-dark',
    'Baja': 'bg-dark'
};

function initActaTable(cursoId) {
    if ($.fn.DataTable.isDataTable('#tablaActa')) {
        $('#tablaActa').DataTable().destroy();
    }
    const estaFinalizado = $('#formActa').data('finalizado');
    const disabled = estaFinalizado ? 'disabled' : '';

    initializeDataTable({
        tableId: 'tablaActa',
        ajaxUrl: `/Cursos/GetActa?cursoId=${cursoId}`,
        columns: [
            {
                data: 'nombreCompleto',
                className: 'text-center'
            },
            {
                data: 'asistencias',
                className: 'text-center',
                render: function (data, type, row, meta) {
                    return `
                        <input type="number" min="0" max="${row.cantidadClases}" class="form-control text-center asistencia" name="Alumnos[${meta.row}].Asistencias" value="${data ?? ''}" ${disabled}/>
                        <input type="hidden" name="Alumnos[${meta.row}].CursoAlumnoId" value="${row.cursoAlumnoId}" />
                        `;
                }
            },
            {
                data: 'notaFinal',
                className: 'text-center',
                render: function (data, type, row, meta) {
                    return `
                        <input type="text" inputmode="decimal" data-filter="decimal" class="form-control text-center nota" name="Alumnos[${meta.row}].NotaFinal" value="${data ?? ''}" ${disabled}/>
                        `;
                }
            },
            // Estado (BADGE COLOREADO)
            {
                data: 'estado',
                className: 'text-center',
                render: function (data) {
                    const className = estadoClasses[data] || 'bg-secondary';
                    return `
                        <span class="badge ${className}">${data ?? 'Sin estado'}</span>
                    `;
                }
            },

            // Acciones (baja alumno, etc)
            {
                data: 'cursoAlumnoId',
                orderable: false,
                searchable: false,
                className: 'text-center',
                render: function (data, type, row) {
                    const isBaja = row.estado === 'Baja';
                    const disabled = isBaja || estaFinalizado ? 'disabled' : '';
                    return `
                        <button type="button" class="btn btn-sm btn-baja" data-id="${row.cursoAlumnoId}" title="Dar de baja" ${disabled}><i class="bi bi-person-dash"></i></button>
                    `;
                }
            }
        ]
    });
}

/*--------------------------------Incripción de alumno a Curso---------------------------------*/
$(document).on('submit', '#formInscribirAlumno', function (e) {
    e.preventDefault();

    const form = $(this);

    $.ajax({
        url: '/Cursos/InscribirAlumno',
        type: 'POST',
        data: form.serialize(),
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        success: function (response) {

            if (response.success) {

                showToast(response.message, 'success');

                const cursoId = $('#modalCursos').data('current-id');

                $.get(`/Cursos/Inscriptos/${cursoId}`, function (html) {
                    initInscriptosTable(cursoId);
                    $('#tablaCursos')
                        .DataTable()
                        .ajax
                        .reload(null, false);
                });
            }
            else {
                showToast(response.message, 'error');
            }
        },
        error: xhr => handleAjaxError(xhr, 'No se pudo inscribir el alumno.')
    });
});

/*-----------------------------------Update Alumnos en Acta------------------------------------*/
$(document).on('submit', '#formActa', function (e) {
    e.preventDefault();

    $('#formActa .nota').each(function () {

        const value = $(this).val();

        if (value) {
            $(this).val(value.toString().replace('.', ','));
        }
    });

    const form = $(this);

    $.ajax({
        url: '/Cursos/GuardarActa',
        type: 'POST',
        data: form.serialize(),
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        success: function (response) {
            if (response.success) {
                showToast(response.message, 'success');
                const modal = bootstrap.Modal.getInstance(document.getElementById('modalCursos'));

                if (modal) {
                    modal.hide();
                }
            }
            else {
                showToast(response.message, 'warning');
            }
        },
        error: xhr => handleAjaxError(xhr, 'No se pudo guardar el acta.')
    });
});

$(document).on('click', '.btn-baja', function () {
    const id = $(this).data('id');

    Swal.fire({
        customClass: { popup: 'swal-dark' },
        title: 'Dar de baja',
        text: 'El alumno será marcado como baja.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, dar de baja',
        cancelButtonText: 'Cancelar',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: `/Cursos/CambiarEstadoAlumno`,
                type: 'POST',
                data: {
                    id: id,
                    estado: 5
                },
                headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
                success: function (response) {
                    if (response.success) {
                        showToast(response.message, 'success');
                        $('#tablaActa').DataTable().ajax.reload(null, false);
                    }
                    else {
                        showToast(response.message, 'warning');
                    }
                },
                error: xhr => handleAjaxError(xhr, 'No se pudo dar de baja al alumno.')
            });
        }
    });
});

/*----------------------------------------Review Curso-----------------------------------------*/
$(document).on('click', '#btnMostrarRechazo', function () {
    $('.btn-autorizar').prop('disabled', true);
    $('#panelRechazo').removeClass('d-none');
});

$(document).on('click', '#btnCancelarRechazo', function () {
    $('#panelRechazo').addClass('d-none');
    $('#motivoRechazo').val('');
    $('.btn-autorizar').prop('disabled', false);
});

$(document).on('click', '.btn-autorizar', function () {
    const id = $(this).data('id');

    $.ajax({
        url: '/Cursos/Autorizar',
        type: 'POST',
        data: {
            id: id
        },
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        success: function (response) {
            if (response.success) {
                showToast(response.message, 'success');
                bootstrap.Modal.getInstance(document.getElementById('modalCursos'))?.hide();
                $('#tablaCursos').DataTable().ajax.reload();
            } else {
                showToast(response.message, 'warning');
            }
        },
        error: xhr => handleAjaxError(xhr, 'No se pudo autorizar el curso.')
    });
});

$(document).on('click', '.btn-confirmar-rechazo', function () {
    const id = $(this).data('id');
    const motivo = $('#motivoRechazo').val();

    if (!motivo.trim()) {
        showToast('Debe ingresar un motivo.', 'warning');
        return;
    }

    $.ajax({
        url: '/Cursos/Rechazar',
        type: 'POST',
        data: {
            id: id,
            motivo: motivo
        },
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        success: function (response) {
            if (response.success) {
                showToast(response.message, 'success');
                bootstrap.Modal.getInstance(document.getElementById('modalCursos'))?.hide();
                $('#tablaCursos').DataTable().ajax.reload();
            } else {
                showToast(response.message, 'warning');
            }
        },
        error: xhr => handleAjaxError(xhr, 'No se pudo rechazar el curso.')
    });
});

$(document).on('click', '.btn-finalizar', function (e) {
    e.preventDefault();

    const cursoId = $(this).data('id');

    Swal.fire({
        customClass: { popup: 'swal-dark' },
        title: '¿Finalizar curso?',
        text: 'El curso quedará cerrado para nuevas inscripciones.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Finalizar',
        cancelButtonText: 'Cancelar',
        reverseButtons: true
    })
        .then(result => {
            if (!result.isConfirmed)
                return;

            $.ajax({
                url: '/Cursos/Finalizar',
                type: 'POST',
                data: {
                    id: cursoId
                },
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    if (response.success) {
                        showToast(response.message, 'success');
                        $('#tablaCursos').DataTable().ajax.reload(null, false);
                    }
                    else {
                        showToast(response.message, 'warning');
                    }
                },
                error: xhr => handleAjaxError(xhr, 'No se pudo finalizar el curso.')
            });
        });
});