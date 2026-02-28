const SERVICE_DURATIONS = {
  haircut: 30, "haircut-wash": 45, color: 60,
  highlights: 75, beard: 15, blowdry: 20,
};

function serviceDuration(service) {
  return SERVICE_DURATIONS[service] ?? 30;
}

export function mockGenerateSlots(date, stylistId, service) {
  const dur      = serviceDuration(service);
  const stylists = stylistId
    ? [{ id: parseInt(stylistId), name: stylistId === "1" ? "Ana" : "Tina" }]
    : [{ id: 1, name: "Ana" }, { id: 2, name: "Tina" }];

  return stylists
    .flatMap((stylist) =>
      Array.from({ length: 8 }, (_, h) => h + 9)
        .flatMap((hour) => [0, 30].map((min) => ({ hour, min, stylist })))
        .map(({ hour, min, stylist }) => {
          const start = new Date(
            `${date}T${String(hour).padStart(2, "0")}:${String(min).padStart(2, "0")}:00`
          );
          return {
            start:        start.toISOString(),
            end:          new Date(start.getTime() + dur * 60000).toISOString(),
            resourceId:   stylist.id,
            resourceName: stylist.name,
          };
        })
        .filter((slot) => new Date(slot.start) > new Date())
        .filter(() => Math.random() > 0.3)
    )
    .sort((a, b) => new Date(a.start) - new Date(b.start));
}

export function mockCreate(payload, chosen) {
  return {
    id:        Math.floor(Math.random() * 100000),
    title:     `${payload.service} – ${payload.name}`,
    ...chosen,
    createdAt: new Date().toISOString(),
  };
}
