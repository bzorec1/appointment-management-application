import { el } from "./utils.js";

export function readForm() {
  return {
    name:          el("name").value.trim(),
    phone:         el("phone").value.trim(),
    gender:        el("gender").value,
    service:       el("service").value,
    stylistId:     el("stylist").value || null,
    date:          el("bookDate").value || null,
    preferredTime: el("preferredTime").value || null,
  };
}

export function validate(form) {
  if (!form.name) return "Prosim vnesi ime stranke.";
  return null;
}
