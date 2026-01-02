export const PeriodOptions = {
  annual: 'annual',
  reprogramming: 'reprogramming',
  closed: 'closed'
} as const

export type Period = typeof PeriodOptions[keyof typeof PeriodOptions]

// API Period mapping
export const ApiPeriodMapping = {
  'ProgramacionAnual': PeriodOptions.annual,
  'Reprogramacion': PeriodOptions.reprogramming,
  'Cerrado': PeriodOptions.closed
} as const

export type ApiPeriod = keyof typeof ApiPeriodMapping


export interface Group {
  id: string;
  name: string;
  color: string;
}

export interface Manning {
  group: string;
  required: number;
  available: number;
  percentage: number;
  status: 'normal' | 'warning' | 'critical';
}

export interface CalendarDay {
  date: Date;
  day: number;
  manning: Manning[];
  averagePercentage: number;
  status: 'normal' | 'warning' | 'critical';
}

export interface CalendarData {
  month: number;
  year: number;
  days: CalendarDay[];
  groups: Group[];
}

export interface CalendarFilters {
  selectedGroups: string[];
  view: 'monthly' | 'weekly' | 'daily';
}

export interface ManningConfig {
  required: number;
  groupId: string;
  month: number;
  year: number;
}

export interface CalendarSettings {
  manningRequerido: number;
  area: string;
  lider: string;
}

// API Response interfaces (preparado para integración con backend)
export interface APICalendarResponse {
  success: boolean;
  data: CalendarData;
  message?: string;
}

export interface APIManningConfigResponse {
  success: boolean;
  data: ManningConfig[];
  message?: string;
}
