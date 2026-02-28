const apiUrl = (base, path) => base ? `${base}${path}` : path;

async function apiError(res, prefix) {
  let detail = "";
  try {
    const ct   = res.headers.get("content-type") ?? "";
    const body = await (ct.includes("application/json") ? res.json() : res.text());
    detail = typeof body === "string" ? body : (body.message ?? body.title ?? JSON.stringify(body));
  } catch (_) {}
  throw new Error(`${prefix} [${res.status}]${detail ? ": " + detail : ""}`);
}

export async function apiSuggestions(baseUrl, date, preferredTime, service, stylistId) {
  const body = {
    serviceId:      service,
    requestedBy:    0,
    timePreference: 0,
  };
  if (date)          body.targetDate    = `${date}T00:00:00.000Z`;
  if (preferredTime) body.preferredTime = preferredTime;

  const res = await fetch(apiUrl(baseUrl, "/api/v1/suggestions?api-version=1.0"), {
    method:  "POST",
    headers: { "Content-Type": "application/json" },
    body:    JSON.stringify(body),
  });

  if (!res.ok) await apiError(res, "Napaka pri iskanju terminov");
  const data = await res.json();

  const rid  = stylistId ? parseInt(stylistId) : 1;
  const name = stylistId === "2" ? "Tina" : "Ana";
  const slots = (data.slots || []).map((s) => ({
    start:        s.startUtc,
    end:          s.endUtc,
    resourceId:   rid,
    resourceName: name,
  }));

  return { slots, alternativeDates: data.alternativeDates || [] };
}

export async function apiCreate(baseUrl, payload, chosen) {
  const res = await fetch(apiUrl(baseUrl, "/api/v1/appointments?api-version=1.0"), {
    method:  "POST",
    headers: { "Content-Type": "application/json" },
    body:    JSON.stringify({
      title:        `${payload.service} – ${payload.name}`,
      start:        chosen.start,
      end:          chosen.end,
      resourceId:   chosen.resourceId,
      phone:        payload.phone || "",
      service:      payload.service,
      customerName: payload.name,
    }),
  });

  if (!res.ok) await apiError(res, "Napaka pri ustvarjanju rezervacije");
  return res.json();
}
