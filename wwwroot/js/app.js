// La clave NO se escribe en este archivo: cualquiera que abra la página podría leerla.
// Se pide una vez y se guarda solo en esta pestaña (se borra al cerrarla).
let API_KEY = sessionStorage.getItem("porthubApiKey");
if (!API_KEY) {
    API_KEY = prompt("Ingresá la API key de PortHub (la del archivo .env):") || "";
    sessionStorage.setItem("porthubApiKey", API_KEY);
}

const HEADERS = {
    "Authorization": `ApiKey ${API_KEY}`,
    "Content-Type": "application/json"
};

// Convierte < > & " ' en texto inofensivo, para que un dato guardado nunca se ejecute como código.
function escapar(valor) {
    return String(valor ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#39;");
}

function switchTab(tabId, el) {
    document.querySelectorAll('.tab-content').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.menu li').forEach(m => m.classList.remove('active'));
    document.getElementById(tabId).classList.add('active');
    el.classList.add('active');
    if (tabId === 'tab-aerolineas') cargarAerolineas();
}

function log(msg) {
    const consoleBox = document.getElementById("console-log");
    consoleBox.innerHTML = `> ${new Date().toLocaleTimeString()} - ${escapar(msg)}<br>` + consoleBox.innerHTML;
}

async function cargarSlots() {
    try {
        const res = await fetch('/api/Vuelos', { headers: HEADERS });
        if (res.status === 401) {
            sessionStorage.removeItem("porthubApiKey");
            log("Clave incorrecta. Recargá la página (F5) y escribila de nuevo.");
            return;
        }
        if (!res.ok) throw new Error("Error obteniendo vuelos");
        const data = await res.json();

        const tbody = document.getElementById("tabla-slots");
        tbody.innerHTML = "";

        // Mapear mapa de gates
        const gatesState = { "G01": null, "G02": null, "G03": null, "G04": null };

        if (data.length === 0) {
            tbody.innerHTML = `<tr><td colspan="7" class="text-center text-muted">No hay slots registrados.</td></tr>`;
        } else {
            data.forEach(s => {
                const esReservado = s.estadoSlot.toLowerCase() === 'reservado' || s.estadoSlot.toLowerCase() === 'confirmado';
                // El código de vuelo va en data-vuelo (escapado), no metido adentro del onclick.
                const botonAccion = esReservado
                    ? `<button class="btn-cancel" data-vuelo="${escapar(s.codigoVuelo)}" onclick="cancelarSlot(this.dataset.vuelo)">❌ Liberar</button>`
                    : `<span class="text-muted text-small">-</span>`;

                if (gatesState.hasOwnProperty(s.gateAsignado)) {
                    gatesState[s.gateAsignado] = { vuelo: s.codigoVuelo, estado: s.estadoSlot.toLowerCase() };
                }

                tbody.innerHTML += `
                    <tr>
                        <td><strong>${escapar(s.codigoVuelo)}</strong></td>
                        <td>${escapar(s.codigoAerolinea)}</td>
                        <td>Pista ${escapar(s.pistaAsignada)}</td>
                        <td>${escapar(s.gateAsignado)}</td>
                        <td>${new Date(s.horarioProgramado).toLocaleString()}</td>
                        <td><span class="tag tag-${escapar(s.estadoSlot)}">${escapar(s.estadoSlot)}</span></td>
                        <td>${botonAccion}</td>
                    </tr>
                `;
            });
        }

        renderGateMap(gatesState);
        log("Slots y mapa 2D actualizados.");
    } catch (err) {
        log(`Error: ${err.message}`);
    }
}

function renderGateMap(gatesState) {
    const container = document.getElementById("gate-map-container");
    container.innerHTML = "";
    Object.keys(gatesState).forEach(gate => {
        const info = gatesState[gate];
        let statusClass = "available";
        let text = "LIBRE";

        if (info) {
            statusClass = info.estado === 'liberado' ? 'released' : 'occupied';
            text = `${info.vuelo} (${info.estado.toUpperCase()})`;
        }

        container.innerHTML += `
            <div class="gate-node ${statusClass}">
                <div class="gate-title">🛫 ${escapar(gate)}</div>
                <div class="gate-info">${escapar(text)}</div>
            </div>
        `;
    });
}

async function reservarSlot() {
    const body = {
        codigoVuelo: document.getElementById("slot-vuelo").value,
        codigoAerolinea: document.getElementById("slot-aerolinea").value,
        pista: parseInt(document.getElementById("slot-pista").value),
        gate: document.getElementById("slot-gate").value,
        horarioProgramado: new Date(document.getElementById("slot-horario").value).toISOString()
    };

    const res = await fetch('/api/Slots/solicitudes', { method: 'POST', headers: HEADERS, body: JSON.stringify(body) });
    const data = await res.json();

    if (res.status === 201) {
        log(`✅ Slot Reservado con éxito para vuelo ${data.codigoVuelo}`);
        cargarSlots();
    } else {
        log(`❌ Conflict (${res.status}): ${data.message}`);
    }
}

async function cancelarSlot(codigoVuelo) {
    const res = await fetch('/api/Slots/cancelaciones', {
        method: 'POST', headers: HEADERS, body: JSON.stringify({ codigoVuelo: codigoVuelo })
    });
    const data = await res.json();
    if (res.ok) {
        log(`🚫 Slot liberado: ${codigoVuelo}`);
        cargarSlots();
    } else {
        log(`❌ Error al liberar: ${data.message}`);
    }
}

async function validarEmbarque() {
    const body = {
        numeroTicket: document.getElementById("emb-ticket").value,
        gate: document.getElementById("emb-gate").value
    };
    const res = await fetch('/api/Embarques', { method: 'POST', headers: HEADERS, body: JSON.stringify(body) });
    const data = await res.json();

    if (res.status === 201) {
        log(`🎉 ACCESO AUTORIZADO! Ticket: ${data.numeroTicket} en Gate ${data.gate}`);
    } else {
        log(`🚫 ACCESO RECHAZADO: ${data.message}`);
    }
}

async function cargarAerolineas() {
    try {
        const res = await fetch('/api/Aerolineas', { headers: HEADERS });
        const data = await res.json();
        const tbody = document.getElementById("tabla-aerolineas");
        tbody.innerHTML = "";
        data.forEach(a => {
            tbody.innerHTML += `
                <tr>
                    <td>${escapar(a.id)}</td>
                    <td><strong>${escapar(a.codigo)}</strong></td>
                    <td>${escapar(a.nombre)}</td>
                    <td>${escapar(a.pais)}</td>
                    <td><code>${escapar(a.direccionBase)}</code></td>
                </tr>
            `;
        });
    } catch (e) {
        log(`Error aerolineas: ${e.message}`);
    }
}

async function crearAerolinea() {
    const body = {
        nombre: document.getElementById("aero-nombre").value,
        codigo: document.getElementById("aero-codigo").value,
        pais: document.getElementById("aero-pais").value,
        direccionBase: document.getElementById("aero-url").value
    };
    const res = await fetch('/api/Aerolineas', { method: 'POST', headers: HEADERS, body: JSON.stringify(body) });
    if (res.status === 201) {
        log(`🏢 Aerolínea ${body.codigo} creada correctamente.`);
        cargarAerolineas();
    } else {
        const err = await res.json();
        log(`❌ Error: ${err.message}`);
    }
}

async function verificarDisponibilidad() {
    const pista = document.getElementById("disp-pista").value;
    const horario = new Date(document.getElementById("disp-horario").value).toISOString();
    const res = await fetch(`/api/Slots/disponibilidad?pista=${encodeURIComponent(pista)}&horario=${horario}`, { headers: HEADERS });
    const data = await res.json();

    const div = document.getElementById("disp-resultado");
    if (data.disponible) {
        div.style.color = "var(--success)";
        div.innerHTML = `✅ Pista ${escapar(pista)} DISPONIBLE para ${new Date(horario).toLocaleString()}`;
    } else {
        div.style.color = "var(--danger)";
        div.innerHTML = `❌ Pista ${escapar(pista)} OCUPADA para esa hora`;
    }
}

// Init defaults
const now = new Date();
document.getElementById('slot-horario').value = now.toISOString().slice(0, 16);
document.getElementById('disp-horario').value = now.toISOString().slice(0, 16);
setInterval(() => {
    document.getElementById('current-time').innerText = new Date().toLocaleTimeString();
}, 1000);

cargarSlots();