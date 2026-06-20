// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

/*----------------------------------------Mensaje Toast----------------------------------------*/
document.addEventListener("DOMContentLoaded", function () {
    if (window.toastMessage && window.toastMessage.trim() !== "") {
        var toastEl = document.getElementById('toastMensaje');
        if (toastEl) {
            var toast = new bootstrap.Toast(toastEl);
            toast.show();
        }
    }
});

function showToast(message, type = 'success') {
    var toastEl = document.getElementById('toastMensaje');
    var toastBody = toastEl.querySelector('.toast-body');

    toastBody.textContent = message;

    // Quitar clases previas y agregar la nueva según el tipo
    toastEl.classList.remove('bg-success', 'bg-danger', 'bg-warning', 'bg-info', 'bg-black', 'text-dark', 'text-white');
    if (type === 'success') toastEl.classList.add('bg-success', 'text-white');
    else if (type === 'error') toastEl.classList.add('bg-danger', 'text-white');
    else if (type === 'warning') toastEl.classList.add('bg-warning', 'text-dark');
    else if (type === 'black') toastEl.classList.add('bg-black', 'text-white');
    else if (type === 'info') toastEl.classList.add('bg-info', 'text-white');

    var toast = new bootstrap.Toast(toastEl);
    toast.show();
}
/*------------------------------------------DataTable------------------------------------------*/
function initializeDataTable(config) {
    return $('#' + config.tableId).DataTable({
        responsive: true,
        processing: true,
        autoWidth: false,

        language: { url: 'https://cdn.datatables.net/plug-ins/2.3.8/i18n/es-AR.json' },

        ajax: {
            url: typeof config.ajaxUrl === 'function' ? config.ajaxUrl() : config.ajaxUrl,
            type: config.method || 'GET',
            dataSrc: config.dataSrc || 'data'
        },

        columns: config.columns,
        order: config.order || [[0, 'asc']],
        pageLength: config.pageLength || 10,
        select: config.select || false,
        destroy: true
    });
}

function initializeCrudTable(config) {
    const tabla = initializeDataTable({
        tableId: config.tableId,
        ajaxUrl: config.ajaxUrl,
        columns: config.columns
    });

    if (config.filters) {
        config.filters.forEach(filter => {
            $(filter.selector).change(function () {
                updateTableUrl();
            });
        });
    }

    function updateTableUrl() {
        let url = config.ajaxUrl;
        const params = new URLSearchParams();

        config.filters.forEach(filter => {
            let value;

            if ($(filter.selector).is(':checkbox')) {
                value = $(filter.selector).is(':checked');
            }
            else {
                value = $(filter.selector).val();
            }

            if (value !== '' && value !== false) {
                params.append(filter.param, value);
            }
        });

        if (params.toString()) {
            url += '?' + params.toString();
        }

        tabla.ajax.url(url).load();
    }

    return tabla;
}

/*-------------------------------------------Modales-------------------------------------------*/
function initializeDetailsModal(config) {
    $(document).on('click', config.buttonClass, function () {
        const id = $(this).data('id');
        $(`#${config.modalId}`)
            .data('current-id', id);
        $.get(`${config.url}/${id}`, function (html) {
            $(config.contentContainer).html(html);
            const modalElement = document.getElementById(config.modalId);
            let modal = bootstrap.Modal.getInstance(modalElement);

            if (!modal) {
                modal = new bootstrap.Modal(modalElement);
            }
            modal.show();
        })
            .fail(xhr => handleAjaxError(xhr, 'Ocurrió un error al cargar el detalle.'));
    });
}

function initializeEditModal(config) {
    $(document).on('click', config.buttonClass, function (e) {
        e.preventDefault();
        const id = $(this).data('id');
        const url = id !== undefined ? `${config.url}/${id}` : config.url;

        $.get(url)
            .done(function (html) {
                $(config.contentContainer).html(html);

                if (config.formSelector) {
                    $.validator.unobtrusive.parse(config.formSelector);
                }

                if (config.onLoad) {
                    config.onLoad();
                }

                const modalElement = document.getElementById(config.modalId);
                let modal = bootstrap.Modal.getInstance(modalElement);

                if (!modal) {
                    modal = new bootstrap.Modal(modalElement);
                }

                modal.show();
            })
            .fail(xhr => handleAjaxError(xhr, 'Ocurrió un error al cargar el formulario.'));
    });
}

function initializeAjaxForm(config) {
    $(document).on('submit', config.formSelector, function (e) {
        e.preventDefault();
        const form = $(this);

        $.ajax({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            success: function (response) {
                if (response.success) {
                    const modalEl = document.getElementById(config.modalId);
                    bootstrap.Modal.getOrCreateInstance(modalEl).hide();
                    showToast(response.message, 'success');

                    if (config.tableId) {
                        $(config.tableId).DataTable().ajax.reload(null, false);
                    }
                }
                else if (response.success === false) {
                    showToast(response.message, 'error');
                }
                else {
                    $(config.contentContainer).html(response);
                    $.validator.unobtrusive.parse(config.formSelector);
                }
            },

            error: xhr => handleAjaxError(xhr)
        });
    });
}

function initializeDelete(config) {
    $(document).on('click', config.buttonClass, function () {
        const name = $(this).data('name');
        const description = $(this).data('description');
        const id = $(this).data('id');

        Swal.fire({
            customClass: { popup: 'swal-dark' },
            title: config.title,
            html: `
            <div class="text-center">
            <p class="mb-1 fw-semibold">
                ${name ?? ''}
            </p>

            ${description ? `<small class="text-muted">${description}</small>` : ''}

            <p class="mt-3 mb-0">
                ${config.text}
            </p>
            </div>
            `,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: `${config.url}/${id}`,
                    type: 'POST',
                    headers: { 'RequestVerificationToken': $('input[name=\"__RequestVerificationToken\"]').val() },
                    success: function (response) {
                        if (response.success) {
                            showToast(response.message, 'success');
                            $(config.tableId).DataTable().ajax.reload(null, false);
                        }
                        else {
                            showToast(response.message, 'error');
                        }
                    },

                    error: xhr => handleAjaxError(xhr)
                });
            }
        });
    });
}

/*---------------------------------------Filtros Inputs----------------------------------------*/
document.addEventListener("input", function (e) {
    const input = e.target;
    const tipo = input.dataset.filter;

    if (!tipo) return;

    const filtros = {
        numbers: /\D/g,
        letters: /[^A-Za-zÀ-ÿ\s]/g,
        decimal: /[^0-9.,]/g
    };

    let value = input.value;
    const regex = filtros[tipo];

    if (!regex) return;

    value = value.replace(regex, '');

    // evitar múltiples separadores decimales
    if (tipo === 'decimal') {
        const parts = value.split(/[.,]/);

        if (parts.length > 2) {
            value = parts[0] + ',' + parts.slice(1).join('');
        }
    }

    input.value = value;
});

/*------------------------------Activación de Input por Checkbox-------------------------------*/
function initializeToggleField(checkboxSelector, containerSelector) {
    toggle();

    $(checkboxSelector).change(toggle);

    function toggle() {
        if ($(checkboxSelector).is(':checked')) {
            $(containerSelector).removeClass('d-none');
        }
        else {
            $(containerSelector).addClass('d-none');
        }
    }
}

/*--------------------------------Activación de Input por Rol----------------------------------*/
function initializeRolFields(rolSelector, tituloContainer, carreraContainer) {
    function toggleFields() {
        const rol = $(rolSelector).val();
        $(tituloContainer).addClass('d-none');
        $(carreraContainer).addClass('d-none');

        if (rol === 'Alumno') {
            $(carreraContainer).removeClass('d-none');
        }
        else if (rol === 'Profesor') {
            $(tituloContainer).removeClass('d-none');
        }
    }

    // estado inicial
    toggleFields();

    // cambio
    $(document).on('change', rolSelector, function () {
        toggleFields();
    });
}

/*------------------------------------Funciones Auxiliares-------------------------------------*/
function formatDate(date) {
    if (!date) return '';

    return new Date(date).toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

function handleAjaxError(xhr, customMessage = null) {
    console.error(xhr);

    let message = customMessage || 'Ocurrió un error inesperado.';

    // si backend devuelve JSON
    if (xhr.responseJSON?.message) {
        message = xhr.responseJSON.message;
    }

    showToast(message, 'error');
}

/*----------------------------------------_PartialLogin----------------------------------------*/
initializeEditModal({
    buttonClass: '.btn-mi-perfil',
    url: '/Perfil/MisDatos',
    modalId: 'modalPerfil',
    contentContainer: '#contenidoModalPerfil',
    formSelector: '#formMiPerfil'
});

initializeEditModal({
    buttonClass: '.btn-cambiar-password',
    url: '/Perfil/CambiarPassword',
    modalId: 'modalPerfil',
    contentContainer: '#contenidoModalPerfil',
    formSelector: '#formPassword'
});

initializeAjaxForm({
    formSelector: '#formMiPerfil',
    modalId: 'modalPerfil',
    contentContainer: '#contenidoModalPerfil'
});

initializeAjaxForm({
    formSelector: '#formPassword',
    modalId: 'modalPerfil',
    contentContainer: '#contenidoModalPerfil'
});