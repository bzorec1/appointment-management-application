import { el, scrollTo, fmt, fmtTime, todayYMD } from "./utils.js";
import { initSettings } from "./settings.js";
import { readForm, validate } from "./form.js";
import { mockGenerateSlots, mockCreate } from "./mock.js";
import { apiSuggestions, apiCreate } from "./api.js";
import { out, showMessage, renderSlots } from "./ui.js";
import { state } from "./state.js";

initSettings();

document.addEventListener("submit", (e) => e.preventDefault());

const cfg = () => ({
  mock:    el("mockMode").checked,
  baseUrl: el("baseUrl").value.trim() || "",
});

async function searchSlots() {
  const form = readForm();
  const date = form.date;

  const btn = el("btnFindSlots");
  btn.disabled    = true;
  btn.textContent = "Iščem…";
  out.classList.add("hidden");
  el("slotsSection").classList.add("hidden");
  el("confirmSection").classList.add("hidden");
  state.selectedSlot = null;

  const c = cfg();

  try {
    const result = c.mock
      ? { slots: mockGenerateSlots(date || todayYMD(), form.stylistId, form.service) }
      : await apiSuggestions(c.baseUrl, date, form.preferredTime, form.service, form.stylistId);
    renderSlots(result.slots);
  } catch (err) {
    showMessage(err.message, "error");
    scrollTo(out);
  } finally {
    btn.disabled    = false;
    btn.textContent = "Poišči proste termine";
  }
}

el("btnFindSlots").addEventListener("click", async (e) => {
  e.preventDefault();
  e.stopPropagation();

  const form  = readForm();
  const error = validate(form);
  if (error) {
    showMessage(error, "warn");
    scrollTo(out);
    return;
  }

  await searchSlots();
});


el("btnConfirm").addEventListener("click", async (e) => {
  e.preventDefault();
  e.stopPropagation();

  if (!state.selectedSlot) {
    showMessage("Prosim izberi termin.", "warn");
    return;
  }

  const form  = readForm();
  const error = validate(form);
  if (error) {
    showMessage(error, "warn");
    return;
  }

  const c = cfg();

  try {
    const created = c.mock
      ? mockCreate(form, state.selectedSlot)
      : await apiCreate(c.baseUrl, form, state.selectedSlot);

    showMessage(
      `Rezervacija potrjena! ${fmt(state.selectedSlot.start)} pri ${state.selectedSlot.resourceName}.`,
      "success",
      created
    );

    el("slotsSection").classList.add("hidden");
    el("confirmSection").classList.add("hidden");
    state.selectedSlot = null;
    scrollTo(out);
  } catch (err) {
    showMessage(err.message, "error");
  }
});

el("btnCancel").addEventListener("click", (e) => {
  e.preventDefault();
  e.stopPropagation();
  el("confirmSection").classList.add("hidden");
  state.selectedSlot = null;
  document.querySelectorAll(".slot-btn.selected").forEach((b) => b.classList.remove("selected"));
});

el("btnClear").addEventListener("click", (e) => {
  e.preventDefault();
  e.stopPropagation();
  el("name").value          = "";
  el("phone").value         = "";
  el("service").value       = "haircut";
  el("stylist").value       = "";
  el("bookDate").value      = "";
  el("preferredTime").value = "";
  el("slotsSection").classList.add("hidden");
  el("confirmSection").classList.add("hidden");
  out.classList.add("hidden");
  state.selectedSlot = null;
  document.querySelectorAll(".slot-btn.selected").forEach((b) => b.classList.remove("selected"));
  window.scrollTo({ top: 0, behavior: "smooth" });
});
