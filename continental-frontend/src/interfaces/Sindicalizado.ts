import type { Rol } from "./User.interface";

export interface Sindicalizado {
    noNomina: string;
    nombre: string;
    area: string;
    grupo: string;
    antiguedad: string;
    fechaIngreso: string;
    maquina?: string;
    roles: Rol[];
    [key: string]: unknown;
}