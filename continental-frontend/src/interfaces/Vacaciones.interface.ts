export interface VacacionesConfig {
  id: number;
  porcentajeAusenciaMaximo: number; // Ej: 4.5
  periodoActual: 'ProgramacionAnual' | 'Reprogramacion' | 'Cerrado';
  anioVigente: number;
  createdAt: string;
  updatedAt: string;
}

export interface VacacionesConfigUpdateRequest {
  porcentajeAusenciaMaximo: number;
  periodoActual: 'ProgramacionAnual' | 'Reprogramacion' | 'Cerrado';
  anioVigente: number;
}
