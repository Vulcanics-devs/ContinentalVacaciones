import { Input } from "@/components/ui/input";
import { ContentContainer } from "@/components/ui/content-container";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useGroupLeaders } from "@/hooks/useGroupLeaders";
import { type User } from "@/interfaces/User.interface";

interface GroupData {
  id: number;
  grupo: string;
  identificadorSAP: string;
  personasPorTurno: string;
  duracionDeturno: string;
  liderGrupo: string; // Can store user ID or name
}

interface GroupsTableProps {
    groups: GroupData[];
    groupLeaders: User[];        // ← NUEVO
    loadingLeaders: boolean;
  onGroupChange: (groupId: number, field: keyof GroupData, value: string) => void;
  lastUpdate?: string;
}

export const GroupsTable = ({ groups, onGroupChange, lastUpdate }: GroupsTableProps) => {
  console.log("_--------GRUPOS----------")
  console.log({ groups });
  const { leaders, loading: leadersLoading } = useGroupLeaders();

  const handleInputChange = (groupId: number, field: keyof GroupData, value: string) => {
    // Prevent negative numbers for numeric fields
    if ((field === 'personasPorTurno' || field === 'duracionDeturno') && value !== '') {
      const numValue = parseInt(value, 10);
      if (isNaN(numValue) || numValue < 0) {
        return; // Don't update if not a number or negative
      }
    }
    onGroupChange(groupId, field, value);
  };

  const handleSelectChange = (groupId: number, field: keyof GroupData, value: string) => {
    onGroupChange(groupId, field, value);
  };

  const columns = [
    { key: 'grupo', label: 'No. Grupo', placeholder: 'Nombre del grupo' },
    { key: 'rol', label: 'Regla de grupo', placeholder: 'Regla de grupo' },
    { key: 'personasPorTurno', label: 'Personas por bloque (programación anual)', placeholder: 'Número de personas', type: 'number' },
    { key: 'duracionDeturno', label: 'Duración de bloque (programación anual X hrs)', placeholder: '24', type: 'number' },
    { key: 'liderGrupo', label: 'Líder de grupo', placeholder: 'Seleccionar líder' }
  ];

  return (
    <ContentContainer title="Tabla de grupos" className="bg-continental-white">
      {/* Table Header */}
      <div className="grid grid-cols-5 gap-4 mb-4">
        {columns.map((column) => (
          <div key={column.key} className="text-sm font-medium text-continental-gray-1">
            {column.label}
          </div>
        ))}
      </div>
      
      {/* Horizontal divider */}
      <div className="border-t border-continental-gray-3 mb-4"></div>
      
      {/* Table Rows */}
      <div className="space-y-4">
        {groups.map((group, index) => (
          <div key={index} className="grid grid-cols-5 gap-4">
            {columns.map((column) => (
              <div key={column.key}>
                {column.key === 'liderGrupo' ? (
                  <Select
                    value={String(group[column.key as keyof GroupData] || "")}
                    onValueChange={(value) => handleSelectChange(group.id, column.key as keyof GroupData, value)}
                    disabled={leadersLoading}
                  >
                    <SelectTrigger className="w-full">
                      <SelectValue 
                        placeholder={leadersLoading ? "Cargando..." : column.placeholder}
                      />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="0">Sin asignar</SelectItem>
                      {leaders.map((leader) => (
                        <SelectItem key={leader.id} value={leader.id.toString()}>
                          {leader.fullName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                ) :
                column.key === 'grupo'  ? (
                  <span className="w-full text-center p-2">{index + 1}</span>
                )
                : column.key === 'rol' ? (
                  <span className="w-full text-center p-2">{group.grupo}</span>
                )
                : (
                  <Input
                    value={group[column.key as keyof GroupData]}
                    onChange={(e) => handleInputChange(group.id, column.key as keyof GroupData, e.target.value)}
                    placeholder={column.placeholder}
                    type={column.type || 'text'}
                    className="w-full p-2"
                    min="0"
                    onKeyDown={(e) => {
                      // Prevent minus key
                      if (e.key === '-' || e.key === 'e' || e.key === 'E' || e.key === '+') {
                        e.preventDefault();
                      }
                    }}
                    onPaste={(e) => {
                      // Prevent pasting negative numbers
                      const pasteData = e.clipboardData.getData('text/plain');
                      if (pasteData.startsWith('-') || isNaN(parseInt(pasteData, 10))) {
                        e.preventDefault();
                      }
                    }}
                  />
                )}
              </div>
            ))}
          </div>
        ))}
      </div>
      
      {/* Last update info */}
      <div className="mt-6 text-left">
        <p className="text-sm text-continental-gray-2">
          Última actualización: {lastUpdate || '11-08-2025 13:32:21'}
        </p>
      </div>
    </ContentContainer>
  );
};