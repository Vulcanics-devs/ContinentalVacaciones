import type { EventType } from "../components/Calendar/useCalendar";

export const creaFechas = (
  start: Date,
  end: Date,
  rol: "R0144" | "N0439" | "R0135" | "R0229" | "R0154" | "R0267" | "R0130" | "N0440" | "N0A01" | "R0133"
): EventType[] => {
  const events: EventType[] = [];
  const currentDate = new Date(start);

  // Define patterns based on rol
  const patterns: Record<typeof rol, { workDays: number[]; restDays: number[]; specialDays: { day: number; type: string }[] }> = {
    "R0144": { workDays: [1, 2, 3, 4, 5], restDays: [0, 6], specialDays: [] }, // Lunes a viernes
    "N0439": { workDays: [1, 2, 3, 4, 5, 6], restDays: [0], specialDays: [] }, // Lunes a sábado
    "R0135": { workDays: [0, 1, 2, 3, 4], restDays: [5, 6], specialDays: [] }, // Domingo a jueves
    "R0229": { workDays: [1, 2, 3, 4, 5], restDays: [0, 6], specialDays: [{ day: 15, type: 'holiday' }] }, // Con día feriado
    "R0154": { workDays: [2, 3, 4, 5, 6], restDays: [0, 1], specialDays: [] }, // Martes a sábado
    "R0267": { workDays: [0, 1, 2, 3, 4, 5, 6], restDays: [], specialDays: [] }, // Todos los días
    "R0130": { workDays: [1, 3, 5], restDays: [0, 2, 4, 6], specialDays: [] }, // Lunes, miércoles, viernes
    "N0440": { workDays: [0, 2, 4, 6], restDays: [1, 3, 5], specialDays: [] }, // Domingo, martes, jueves, sábado
    "N0A01": { workDays: [1, 2, 3, 4, 5], restDays: [0, 6], specialDays: [{ day: 10, type: 'inability' }] }, // Con incapacidad
    "R0133": { workDays: [1, 2, 3, 4, 5], restDays: [0, 6], specialDays: [{ day: 20, type: 'holiday-boss' }] }, // Con vacaciones jefe
  };

  const pattern = patterns[rol] || patterns["R0144"];

  while (currentDate <= end) {
    const dayOfWeek = currentDate.getDay(); // 0 = Sunday, 1 = Monday, etc.
    const dayOfMonth = currentDate.getDate();

    let eventType: string;
    let turno: number | undefined;

    // Check special days
    const special = pattern.specialDays.find(s => s.day === dayOfMonth);
    if (special) {
      eventType = special.type;
    } else if (pattern.workDays.includes(dayOfWeek)) {
      eventType = 'work';
      turno = (dayOfWeek % 3) + 1; // Rotate turns 1,2,3
    } else if (pattern.restDays.includes(dayOfWeek)) {
      eventType = 'rest';
    } else {
      eventType = 'not-work';
    }

    const event: EventType = {
      day: new Date(currentDate),
      eventType,
      turno,
    };

    events.push(event);
    currentDate.setDate(currentDate.getDate() + 1);
  }

  return events;
};