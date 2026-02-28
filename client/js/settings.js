import { el } from "./utils.js";

export function initSettings() {
  const mockEl    = el("mockMode");
  const urlEl     = el("baseUrl");
  const saved     = localStorage.getItem("hsaBaseUrl");
  const savedMock = localStorage.getItem("hsaMock");

  if (saved     !== null) urlEl.value    = saved;
  if (savedMock !== null) mockEl.checked = savedMock === "true";

  urlEl.addEventListener("input",   () => localStorage.setItem("hsaBaseUrl", urlEl.value.trim()));
  mockEl.addEventListener("change", () => localStorage.setItem("hsaMock", mockEl.checked));
}
