import type { Grupo } from '@/interfaces/Areas.interface';

interface GroupData {
  grupo: string;
  identificadorSAP: string;
  personasPorTurno: string;
  duracionTurno: string;
  liderGrupo: string;
}

// General function to sort API groups by rol
export const sortApiGroups = (groups: Grupo[]) => {
  return groups.sort((a, b) => {
    const parseRol = (rol: string) => {
      // Check if it has underscore suffix (e.g., "R0144_02")
      if (rol.includes('_')) {
        const parts = rol.split('_');
        const baseRol = parts[0];
        const suffix = parseInt(parts[1], 10) || 0;
        const baseNumber = parseInt(baseRol.replace(/\D/g, ''), 10) || 0;
        return { baseNumber, suffix, hasUnderscore: true };
      } else {
        // Base rol without underscore (e.g., "R0144")
        const baseNumber = parseInt(rol.replace(/\D/g, ''), 10) || 0;
        return { baseNumber, suffix: 0, hasUnderscore: false };
      }
    };

    const rolA = parseRol(a.rol);
    const rolB = parseRol(b.rol);
    
    // First sort by base number
    if (rolA.baseNumber !== rolB.baseNumber) {
      return rolA.baseNumber - rolB.baseNumber;
    }
    
    // If same base number, base rol (without underscore) comes first
    if (rolA.hasUnderscore !== rolB.hasUnderscore) {
      return rolA.hasUnderscore ? 1 : -1;
    }
    
    // If both have underscores, sort by suffix
    return rolA.suffix - rolB.suffix;
  });
};

// Existing function for mapped GroupData
export const sortGroups = (groups: GroupData[]) => {
  const sortedGroups = groups.sort((a, b) => {
    const parseRol = (rol: string) => {
      const parts = rol.split("_");
      const identifier = parts[0];
      const suffix = parts[1] ? parseInt(parts[1], 10) : 0;
      return { identifier, suffix };
    };

    const aRol = parseRol(a.grupo);
    const bRol = parseRol(b.grupo);

    // First sort by identifier
    if (aRol.identifier !== bRol.identifier) {
      return aRol.identifier.localeCompare(bRol.identifier);
    }

    // Then sort by suffix (numeric)
    return aRol.suffix - bRol.suffix;
  });
  return sortedGroups;
};
