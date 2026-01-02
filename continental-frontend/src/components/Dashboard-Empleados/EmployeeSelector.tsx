import { useState, useEffect, useMemo } from "react";
import { Search, X, User } from "lucide-react";
import { Input } from "../ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "../ui/select";
import { empleadosService } from "@/services/empleadosService";
import type { UsuarioInfoDto } from "@/interfaces/Api.interface";

interface EmployeeSelectorProps {
  currentUser: UsuarioInfoDto | null;
  selectedEmployee: UsuarioInfoDto;
  onSelectEmployee: (employee: UsuarioInfoDto) => void;
  isDelegadoSindical: boolean;
}

export const EmployeeSelector = ({
  currentUser,
  selectedEmployee,
  onSelectEmployee,
  isDelegadoSindical
}: EmployeeSelectorProps) => {
  const [allEmployees, setAllEmployees] = useState<UsuarioInfoDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [isSelectOpen, setIsSelectOpen] = useState(false);

  // Cargar TODOS los empleados al montar el componente
  useEffect(() => {
    const fetchAllEmployees = async () => {
      setLoading(true);
      try {
        console.log('🔄 Cargando todos los empleados...');

        // Cargar todos los empleados con paginación
        let allEmps: UsuarioInfoDto[] = [];
        let currentPage = 1;
        let hasMore = true;
        const pageSize = 100; // Cargar de 100 en 100

        while (hasMore) {
          const resp = await empleadosService.getEmpleadosSindicalizados({
            Page: currentPage,
            PageSize: pageSize
          });

          allEmps = [...allEmps, ...resp.usuarios];
          hasMore = resp.hasNextPage;
          currentPage++;

          // Log de progreso
          console.log(`📋 Página ${currentPage - 1}: ${resp.usuarios.length} empleados (Total: ${allEmps.length})`);
        }

        setAllEmployees(allEmps);
        console.log(`✅ Total de empleados cargados: ${allEmps.length}`);

      } catch (error) {
        console.error('❌ Error cargando empleados:', error);
        setAllEmployees([]);
      } finally {
        setLoading(false);
      }
    };

    fetchAllEmployees();
  }, []);

  // Filtrar empleados basado en el término de búsqueda
  const filteredEmployees = useMemo(() => {
    if (!searchTerm.trim()) {
      // Si no hay búsqueda, mostrar los primeros 20 empleados
      return allEmployees.slice(0, 20);
    }

    const term = searchTerm.toLowerCase().trim();

    // Filtrar por nombre, nómina, área o grupo
    const filtered = allEmployees.filter(emp => {
      const fullName = emp.fullName?.toLowerCase() || '';
      const nomina = emp.nomina?.toString() || '';
      const username = emp.username?.toLowerCase() || '';
      const area = emp.area?.nombreGeneral?.toLowerCase() || '';
      const grupo = emp.grupo?.rol?.toLowerCase() || '';

      return fullName.includes(term) ||
             nomina.includes(term) ||
             username.includes(term) ||
             area.includes(term) ||
             grupo.includes(term);
    });

    // Limitar a 50 resultados para mantener el rendimiento del UI
    return filtered.slice(0, 50);
  }, [searchTerm, allEmployees]);

  // Manejar la selección de empleado
  const handleSelectEmployee = (value: string) => {
    const employee = allEmployees.find(emp => emp.username === value);
    if (employee) {
      onSelectEmployee(employee);
      setIsSelectOpen(false);
      setSearchTerm('');
      console.log('👤 Empleado seleccionado:', employee);
    }
  };

  // Resetear al usuario actual
  const handleReset = () => {
    if (currentUser) {
      console.log('👤 Resetear a:', currentUser);
      onSelectEmployee(currentUser);
      setSearchTerm('');
      // Limpiar localStorage cuando se resetea al usuario actual
      localStorage.removeItem('selectedEmployee');
    }
  };

  // Si no es delegado sindical, no mostrar el selector
  if (!isDelegadoSindical) {
    return null;
  }

  return (
    <div className="mb-6 ">
      <div className="flex items-center justify-between mb-3">
        <h3 className="text-sm font-medium text-gray-700">
          Representando a:
        </h3>
        {selectedEmployee?.fullName && selectedEmployee.id !== currentUser?.id && (
          <button
            onClick={handleReset}
            className="text-xs cursor-pointer text-blue-600 hover:text-blue-700 flex items-center gap-1"
          >
            <X size={14} />
            Volver a mi cuenta
          </button>
        )}
      </div>

      <Select
        open={isSelectOpen}
        onOpenChange={setIsSelectOpen}
        value={selectedEmployee?.username || ''}
        onValueChange={handleSelectEmployee}
      >
        <SelectTrigger className="w-full">
          <SelectValue>
            {selectedEmployee?.fullName ? (
              <div className="flex items-center gap-2">
                <User size={16} className="text-gray-500" />
                <span className="font-medium">{selectedEmployee.fullName}</span>
                <span className="text-gray-500 text-sm">({selectedEmployee.nomina ?? selectedEmployee.username})</span>
              </div>
            ) : (
              <span className="text-gray-500">Seleccionar empleado...</span>
            )}
          </SelectValue>
        </SelectTrigger>
        <SelectContent className="max-h-[400px]">
          {/* Campo de búsqueda dentro del Select */}
          <div className="sticky top-0 bg-white border-b p-2 z-10">
            <div className="relative">
              <Search className="absolute left-2 top-2.5 h-4 w-4 text-gray-500" />
              <Input
                placeholder="Buscar por nombre, nómina, área o grupo..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-8 h-9"
                onClick={(e) => e.stopPropagation()}
                onKeyDown={(e) => e.stopPropagation()}
              />
            </div>
          </div>

          {/* Lista de empleados */}
          <div className="p-1">
            {loading ? (
              <div className="py-8 text-center text-gray-500">
                <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-gray-900 mx-auto mb-2"></div>
                <p className="text-sm">Cargando empleados...</p>
              </div>
            ) : filteredEmployees.length === 0 ? (
              <div className="py-8 text-center text-gray-500">
                <p className="text-sm">
                  {searchTerm ? 'No se encontraron empleados' : 'No hay empleados disponibles'}
                </p>
              </div>
            ) : (
              <>
                {/* Opción para volver al usuario actual */}
                {currentUser && (
                  <SelectItem value={currentUser.username} className="mb-1">
                    <div className="flex flex-col">
                      <div className="flex items-center gap-2">
                        <span className="font-medium">{currentUser.fullName}</span>
                        <span className="text-xs text-blue-600">(Yo)</span>
                      </div>
                      <div className="text-xs text-gray-500">
                        {currentUser.nomina} • {currentUser.area?.nombreGeneral} • {currentUser.grupo?.rol}
                      </div>
                    </div>
                  </SelectItem>
                )}

                {/* Separador si es necesario */}
                {currentUser && filteredEmployees.some(emp => emp.id !== currentUser.id) && (
                  <div className="border-t my-2"></div>
                )}

                {/* Lista de empleados filtrados */}
                {filteredEmployees
                  .filter(emp => emp.id !== currentUser?.id) // No duplicar el usuario actual
                  .map((emp) => (
                    <SelectItem key={emp.id} value={emp.username}>
                      <div className="flex flex-col py-1">
                        <div className="flex items-center gap-2">
                          <span className="font-medium">{emp.fullName}</span>
                        </div>
                        <div className="text-xs text-gray-500">
                          {emp.nomina} • {emp.area?.nombreGeneral} • {emp.grupo?.rol}
                        </div>
                      </div>
                    </SelectItem>
                  ))}
              </>
            )}

            {/* Mensaje si hay más resultados */}
            {searchTerm && allEmployees.filter(emp => {
              const term = searchTerm.toLowerCase().trim();
              const fullName = emp.fullName?.toLowerCase() || '';
              const nomina = emp.nomina?.toString() || '';
              const username = emp.username?.toLowerCase() || '';
              const area = emp.area?.nombreGeneral?.toLowerCase() || '';
              const grupo = emp.grupo?.rol?.toLowerCase() || '';

              return fullName.includes(term) ||
                     nomina.includes(term) ||
                     username.includes(term) ||
                     area.includes(term) ||
                     grupo.includes(term);
            }).length > 50 && (
              <div className="text-center py-2 text-xs text-gray-500 border-t">
                Mostrando los primeros 50 resultados. Sea más específico en su búsqueda.
              </div>
            )}
          </div>
        </SelectContent>
      </Select>

      {/* Información del empleado seleccionado */}
      {selectedEmployee?.fullName && (
        <div className="mt-3 p-3 bg-gray-50 rounded-md text-xs">
          <div className="grid grid-cols-2 gap-2">
            <div>
              <span className="text-gray-500">Área:</span>
              <p className="font-medium">{selectedEmployee.area?.nombreGeneral || 'N/A'}</p>
            </div>
            <div>
              <span className="text-gray-500">Grupo:</span>
              <p className="font-medium">{selectedEmployee.grupo?.rol || 'N/A'}</p>
            </div>
          </div>
        </div>
      )}

      {/* Contador de empleados */}
      {!loading && (
        <div className="mt-2 text-xs text-gray-500 text-center">
          Total de empleados: {allEmployees.length}
        </div>
      )}
    </div>
  );
};