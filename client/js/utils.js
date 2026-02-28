export const el       = (id) => document.getElementById(id);
export const scrollTo = (elem) => elem.scrollIntoView({ behavior: "smooth", block: "start" });

export const fmt     = (iso) => new Date(iso).toLocaleString("sl-SI", { timeZone: "UTC" });
export const fmtTime = (iso) => new Date(iso).toLocaleTimeString("sl-SI", { hour: "2-digit", minute: "2-digit", timeZone: "UTC" });
export const fmtDate = (iso) => new Date(iso).toLocaleDateString("sl-SI", { weekday: "short", day: "numeric", month: "short", timeZone: "UTC" });

export const todayYMD = () => {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
};
