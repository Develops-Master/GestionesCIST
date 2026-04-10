// Gestiones CIST - Scripts Globales

// Configuración de SignalR
let connection = null;

function inicializarSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/kanbanHub")
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on("OrdenAsignada", (data) => {
        mostrarToast("Orden asignada exitosamente", "success");
    });

    connection.on("OrdenMovida", (data) => {
        mostrarToast(`Orden movida a ${data.estado}`, "info");
    });

    connection.on("NuevoTicketCreado", (data) => {
        mostrarToast(`Nuevo ticket: ${data.codigoTicket}`, "info");
    });

    connection.on("AlertaIA", (data) => {
        Swal.fire({
            icon: 'warning',
            title: '🤖 Alerta IA',
            text: data.mensaje,
            timer: 5000
        });
    });

    connection.on("MensajeRecibido", (data) => {
        console.log('Mensaje recibido:', data);
    });

    connection.start()
        .then(() => {
            console.log("SignalR conectado");
            connection.invoke("UnirseAGrupo", obtenerRolUsuario());
        })
        .catch(err => console.error("Error SignalR:", err));
}

// Notificaciones
async function cargarNotificaciones() {
    try {
        const response = await fetch('/api/notificaciones/no-leidas', {
            headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
        });

        if (response.ok) {
            const data = await response.json();
            actualizarPanelNotificaciones(data.data);
            actualizarContadorNotificaciones(data.data.length);
        }
    } catch (error) {
        console.error('Error cargando notificaciones:', error);
    }
}

function actualizarPanelNotificaciones(notificaciones) {
    const container = $('#notificacionesList');

    if (notificaciones.length === 0) {
        container.html(`
            <div class="p-3 text-center text-muted">
                <i class="bi bi-bell-slash"></i>
                <p class="mb-0">No hay notificaciones nuevas</p>
            </div>
        `);
        return;
    }

    let html = '<div style="max-height: 400px; overflow-y: auto;">';
    notificaciones.forEach(n => {
        html += `
            <div class="notification-item ${n.leida ? '' : 'unread'}" onclick="marcarLeida(${n.id})">
                <div class="d-flex">
                    <div class="flex-shrink-0">
                        <i class="bi ${getIconoNotificacion(n.tipo)} fs-5"></i>
                    </div>
                    <div class="flex-grow-1 ms-2">
                        <strong>${n.titulo}</strong>
                        <p class="mb-1 small">${n.mensaje}</p>
                        <span class="notification-time">${n.tiempoTranscurrido}</span>
                    </div>
                </div>
            </div>
        `;
    });
    html += '</div>';

    container.html(html);
}

function actualizarContadorNotificaciones(count) {
    $('#notificacionesCount').text(count).toggle(count > 0);
}

async function marcarLeida(id) {
    try {
        await fetch(`/api/notificaciones/${id}/leida`, {
            method: 'PUT',
            headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
        });
        cargarNotificaciones();
    } catch (error) {
        console.error('Error:', error);
    }
}

function getIconoNotificacion(tipo) {
    const iconos = {
        'TICKET_CREADO': 'bi-ticket-detailed',
        'TICKET_ASIGNADO': 'bi-person-check',
        'ORDEN_ASIGNADA': 'bi-kanban',
        'EQUIPO_LISTO': 'bi-check-circle',
        'ALERTA_RETRASO': 'bi-exclamation-triangle',
        'IA_PREDICCION': 'bi-robot'
    };
    return iconos[tipo] || 'bi-bell';
}

// Utilidades
function mostrarToast(mensaje, tipo = 'info') {
    Swal.fire({
        toast: true,
        position: 'top-end',
        icon: tipo,
        title: mensaje,
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true
    });
}

function obtenerRolUsuario() {
    // Obtener del token JWT o de una variable global
    return 'JEFE_SOPORTE';
}

function formatearFecha(fecha) {
    return new Date(fecha).toLocaleDateString('es-PE', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function confirmarAccion(mensaje, callback) {
    Swal.fire({
        title: '¿Estás seguro?',
        text: mensaje,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí, continuar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            callback();
        }
    });
}

// Inicializar al cargar la página
$(document).ready(function () {
    if (typeof signalR !== 'undefined' && document.querySelector('[data-authenticated="true"]')) {
        inicializarSignalR();
    }

    // Activar tooltips de Bootstrap
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});