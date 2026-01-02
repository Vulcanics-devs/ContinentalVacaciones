import { useState, useMemo, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { DataTable, type Column, type PaginationConfig } from '@/components/ui/data-table';
import { FilterBar, type FilterConfig } from '@/components/ui/filter-bar';
import { ContentContainer } from '@/components/ui/content-container';
import { PlusCircle } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Register } from '@/components/Usuario/Register';
import { useAreas } from '@/hooks/useAreas';
import { useUsers } from '@/hooks/useUsers';
import type { User, UserArea, UserGrupo, Rol, UserAreaWithGroups } from '@/interfaces/User.interface';

// Componente principal Usuarios
export const Usuarios = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedArea, setSelectedArea] = useState('all');
  const [selectedGroup, setSelectedGroup] = useState('all');
  const [showForm, setShowForm] = useState(false);
  const navigate = useNavigate();
  const { areas, getAreaById } = useAreas();
  
  // Hook para manejo de usuarios con API real
  const {
    users,
    currentPage,
    pageSize,
    totalUsers,
    totalPages,
    loading,
    error,
    refetch,
    setPage,
    setPageSize
  } = useUsers(1, 10);


  // Los datos de usuarios ahora vienen del hook useUsers

  // State for groups from selected area
  const [availableGroups, setAvailableGroups] = useState<string[]>([]);
  
  // Load groups when area is selected
  const loadGroupsForArea = async (areaId: string) => {
    if (areaId === 'all' || !areaId) {
      setAvailableGroups([]);
      return;
    }
    
    try {
      const areaDetails = await getAreaById(parseInt(areaId));
      if (areaDetails.grupos && areaDetails.grupos.length > 0) {
        const groupRoles = areaDetails.grupos.map(grupo => grupo.rol);
        setAvailableGroups(groupRoles);
      } else {
        setAvailableGroups([]);
      }
    } catch (error) {
      console.error('Error loading groups for area:', error);
      setAvailableGroups([]);
    }
  };
  
  // Load groups when selectedArea changes
  useEffect(() => {
    loadGroupsForArea(selectedArea);
  }, [selectedArea]);


  // Configuración de filtros
  const filterConfigs: FilterConfig[] = [
    {
      type: 'search',
      key: 'search',
      placeholder: 'Buscar por nombre o ID',
      value: searchTerm,
      onChange: setSearchTerm
    },
    {
      type: 'select',
      key: 'area',
      label: 'Área',
      placeholder: 'Seleccionar área',
      value: selectedArea,
      onChange: setSelectedArea,
      options: [
        { value: 'all', label: 'Todas las áreas' },
        ...areas.map(area => ({ value: area.areaId.toString(), label: area.nombreGeneral }))
      ]
    },
    {
      type: 'select',
      key: 'group',
      label: 'Grupo',
      placeholder: 'Seleccionar grupo',
      value: selectedGroup,
      onChange: setSelectedGroup,
      options: [
        { value: 'all', label: 'Todos los grupos' },
        ...availableGroups.map(group => ({ value: group, label: group }))
      ]
    }
  ];

  // Configuración de columnas de la tabla
  const columns: Column<User>[] = [
    {
      key: 'id',
      label: 'ID',
      sortable: true,
      render: (value) => <span className="font-bold">{String(value)}</span>
    },
    {
      key: 'fullName',
      label: 'Nombre',
      sortable: true
    },
    {
      key: 'areas',
      label: 'Áreas',
      sortable: false,
      render: (_, row) => {
        // Si viene la lista de areas usarla, si no fallback al campo único area
        const areas: UserAreaWithGroups[] | undefined = row.areas as any;
        if (areas && areas.length > 0) {
          return (
            <div className="flex flex-col gap-1">
              {areas.map(a => (
                <div key={a.areaId} className="text-xs leading-tight">
                  <span className="font-semibold">{a.nombreGeneral}</span>
                </div>
              ))}
            </div>
          );
        }
        const single = row.area as UserArea | null;
        return single ? single.nombreGeneral : <span className="text-gray-500">Sin área</span>;
      }
    },
    {
      key: 'grupos',
      label: 'Grupos por área',
      sortable: false,
      render: (_, row) => {
        // Objetivo: Mostrar solo el length de grupos por cada área: "3 grupos".
        // Si no hay lista de areas, pero hay grupo único, mostrar 1 grupo del área.
        const areas: UserAreaWithGroups[] | undefined = row.areas as any;
        if (areas && areas.length > 0) {
          return (
            <div className="flex flex-col gap-1">
              {areas.map(a => {
                const count = a.grupos?.length || 0;
                return (
                  <div key={a.areaId} className="text-xs leading-tight">
                    {count > 0 ? `${count} grupo${count !== 1 ? 's' : ''}` : '0 grupos'}
                  </div>
                );
              })}
            </div>
          );
        }
        // Fallback al campo grupo único
        const singleGroup = row.grupo as UserGrupo | null;
        if (singleGroup) {
          return <span className="text-xs">1 grupo</span>;
        }
        return <span className="text-gray-500 text-xs">Sin grupos</span>;
      }
    },
    {
      key: 'roles',
      label: 'Roles',
      sortable: true,
      render: (value) => {
        const roles = value as Rol[];
        if (Array.isArray(roles) && roles.length > 0) {
          return (
            <div className="flex flex-wrap gap-1">
              {roles.map((role, index) => (
                <span key={index} className="bg-continental-yellow px-2 py-1 rounded text-xs font-medium">
                  {role.abreviation}
                </span>
              ))}
            </div>
          );
        }
        return <span className="text-gray-500">Sin roles</span>;
      }
    },
    {
      key: 'acciones',
      label: 'Acciones',
      sortable: false,
      render: (_, row) => (
        <div className="flex gap-2">
          <Button
            variant="continental"
            size="sm"
            onClick={() => handleViewUser(row.id.toString())}
          >
            Ver usuario
          </Button>
          {/* Comentado para uso futuro */}
          {/* <Button
            variant="outline"
            size="sm"
            onClick={() => handleEditUser(row.id)}
            className="p-2"
          >
            <Edit size={16} />
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() => handleDeleteUser(row.id)}
            className="p-2 text-red-600 hover:text-red-700"
          >
            <Trash2 size={16} />
          </Button> */}
        </div>
      )
    }
  ];

  // Filtrar datos basado en los filtros
  const filteredData = useMemo(() => {
    return users.filter(user => {
      const matchesSearch = user.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        user.id.toString().includes(searchTerm);

      // Área: si hay múltiples áreas revisar cualquiera coincida
      let matchesArea = true;
      if (selectedArea !== 'all') {
        if (user.areas && user.areas.length > 0) {
          matchesArea = user.areas.some(a => a.areaId.toString() === selectedArea);
        } else {
          matchesArea = !!(user.area && user.area.areaId.toString() === selectedArea);
        }
      }

      // Grupo: si hay múltiples áreas con grupos sumar todos y revisar rol
      let matchesGroup = true;
      if (selectedGroup !== 'all') {
        if (user.areas && user.areas.length > 0) {
          matchesGroup = user.areas.some(a => (a.grupos || []).some(g => g.rol === selectedGroup));
        } else {
          matchesGroup = !!(user.grupo && user.grupo.rol === selectedGroup);
        }
      }

      return matchesSearch && matchesArea && matchesGroup;
    });
  }, [users, searchTerm, selectedArea, selectedGroup]);

  // Para filtros locales, usamos los datos filtrados directamente
  // La paginación se maneja en el servidor
  const displayData = filteredData;

  // Configuración de paginación
  const paginationConfig: PaginationConfig = {
    currentPage,
    totalPages,
    pageSize,
    totalItems: totalUsers,
    onPageChange: (page: number) => {
      setPage(page);
    },
    onPageSizeChange: (newPageSize: number) => {
      setPageSize(newPageSize);
    }
  };

  // Handlers
  const handleNewUser = () => {
    setShowForm(true);
  };

  const handleViewUser = (id: string) => {
    // Aquí se implementará la lógica para ver el usuario
    navigate(`/admin/usuarios/${id}`);
  };

  // Comentado para uso futuro
  // const handleEditUser = (id: string) => {
  //   console.log(`Editar usuario: ${id}`);
  // };

  // const handleDeleteUser = (id: string) => {
  //   console.log(`Eliminar usuario: ${id}`);
  // };

  const handleSort = (column: string) => {
    console.log(`Sorting by: ${column}`);
  };

  // Handlers para el componente Register
  const handleRegisterSuccess = () => {
    setShowForm(false);
    // Refrescar la lista de usuarios después de crear uno nuevo
    refetch();
    console.log('Usuario registrado exitosamente');
  };

  const handleRegisterCancel = () => {
    setShowForm(false);
  };

  // Mostrar componente Register si showForm es true
  if (showForm) {
    return (
      <Register 
        onCancel={handleRegisterCancel}
        onSuccess={handleRegisterSuccess}
      />
    );
  }

  return (
    <div className="p-6 bg-white h-screen">
      <div className="max-w-7xl mx-auto w-full space-y-6">
        
        {/* Header con título y botón */}
        <div className="flex justify-between items-center">
          <div className="text-[25px] font-bold text-continental-black text-left">
            Usuarios
          </div>
          <Button
            variant="continental"
            className="flex items-center gap-2 h-[45px] px-6 rounded-lg"
            onClick={handleNewUser}
          >
            <PlusCircle size={20} />
            Nuevo usuario
          </Button>
        </div>

        {/* Contenedor con filtros y tabla */}
        <ContentContainer>
          {/* Filtros */}
          <FilterBar 
            filters={filterConfigs}
            gridCols={3}
          />

          {/* Loading y Error States */}
          {loading && (
            <div className="flex justify-center items-center py-8">
              <div className="text-lg">Cargando usuarios...</div>
            </div>
          )}
          
          {error && (
            <div className="flex justify-center items-center py-8">
              <div className="text-lg text-red-600">Error: {error}</div>
            </div>
          )}
          
          {/* Tabla */}
          {!loading && !error && (
            <DataTable
              columns={columns as any}
              data={displayData as any}
              keyField="id"
              emptyMessage="No se encontraron usuarios que coincidan con los filtros seleccionados."
              onSort={handleSort}
              pagination={paginationConfig}
            />
          )}
        </ContentContainer>
      </div>
    </div>
  );
};