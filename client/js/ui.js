import { el, scrollTo, fmt, fmtTime, fmtDate } from "./utils.js";
import { readForm } from "./form.js";
import { state } from "./state.js";

export const out = el("out");

export function showMessage(msg, kind = "success", payload = null) {
  out.className = `card ${kind}`;
  out.textContent = "";
  out.classList.remove("hidden");

  const text = document.createElement("div");
  text.textContent = msg;
  out.appendChild(text);

  if (payload?.customerGoogleCalendarUrl || payload?.customerIcsUrl) {
    out.appendChild(buildCalendarButtons(payload));
  }
  if (payload?.smsPreview) {
    out.appendChild(buildSmsPreview(payload.smsPreview));
  }
}

function buildCalendarButtons({ customerGoogleCalendarUrl, customerIcsUrl }) {
  const row = document.createElement("div");
  row.className = "cal-buttons";

  if (customerGoogleCalendarUrl) {
    row.appendChild(Object.assign(document.createElement("a"), {
      href: customerGoogleCalendarUrl,
      target: "_blank",
      rel: "noopener noreferrer",
      className: "btn-cal btn-gcal",
      textContent: "Dodaj v Google Koledar",
    }));
  }
  if (customerIcsUrl) {
    row.appendChild(Object.assign(document.createElement("a"), {
      href: customerIcsUrl,
      download: "",
      className: "btn-cal btn-ics",
      textContent: "Prenesi .ics",
    }));
  }

  return row;
}

function buildSmsPreview(text) {
  const wrap = document.createElement("div");
  wrap.className = "sms-preview";
  wrap.appendChild(Object.assign(document.createElement("p"), { className: "sms-label", textContent: "Predogled SMS sporočila:" }));
  wrap.appendChild(Object.assign(document.createElement("div"), { className: "sms-bubble", textContent: text }));
  return wrap;
}

export function renderSlots(slots) {
  const container = el("slotsContainer");
  container.innerHTML = "";
  el("noSlots").classList.add("hidden");

  if (!slots?.length) {
    el("noSlots").classList.remove("hidden");
    el("slotsSection").classList.remove("hidden");
    scrollTo(el("slotsSection"));
    return;
  }

  for (const slot of slots) {
    const btn = document.createElement("button");
    btn.type = "button";
    btn.className = "slot-btn";
    btn.innerHTML = `<span class="slot-date">${fmtDate(slot.start)}</span>
                     <span class="slot-time">${fmtTime(slot.start)} – ${fmtTime(slot.end)}</span>
                     <span class="slot-stylist">${slot.resourceName}</span>`;
    btn.onclick = () => selectSlot(slot, btn);
    container.appendChild(btn);
  }

  el("slotsSection").classList.remove("hidden");
  scrollTo(el("slotsSection"));
}

export function selectSlot(slot, btn) {
  document.querySelectorAll(".slot-btn.selected").forEach((b) => b.classList.remove("selected"));
  btn.classList.add("selected");
  state.selectedSlot = slot;
  renderConfirm(slot, readForm());
}

function renderConfirm(slot, form) {
  const details = el("confirmDetails");
  details.textContent = "";

  const rows = [
    ["Stranka", `${form.name} (${form.phone || "ni telefona"})`],
    ["Storitev", form.service],
    ["Termin", `${fmt(slot.start)} – ${fmtTime(slot.end)}`],
    ["Frizer", slot.resourceName],
  ];

  for (const [label, value] of rows) {
    const p = document.createElement("p");
    const strong = document.createElement("strong");
    strong.textContent = label + ": ";
    p.append(strong, document.createTextNode(value));
    details.appendChild(p);
  }

  el("confirmSection").classList.remove("hidden");
  scrollTo(el("confirmSection"));
}
