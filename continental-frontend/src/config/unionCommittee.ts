/**
 * Números de nómina del Comité Sindical con permisos extendidos.
 * Se usan para habilitar la experiencia de Delegado Sindical en el frontend
 * aunque el backend aún no marque el rol.
 */
export const UNION_COMMITTEE_NOMINAS = [
  '32804388',
  '32804395',
  '32804415',
  '32804417',
  '32804735',
  '32804762',
  '32812474',
  '32813084',
];

export const isUnionCommitteeNomina = (nomina?: string | number | null): boolean => {
  if (nomina === undefined || nomina === null) return false;

  const normalized = nomina.toString().trim();
  return UNION_COMMITTEE_NOMINAS.includes(normalized);
};
