import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { authService } from '@/services/authService';
import { useRoles } from '@/hooks/useRoles';
import { useAreas } from '@/hooks/useAreas';
import type { RegisterRequest } from '@/interfaces/Api.interface';
import { toast } from 'sonner';
import { Shuffle, Eye, EyeOff } from 'lucide-react';

interface RegisterProps {
  onCancel: () => void;
  onSuccess: () => void;
}

export const Register = ({ onCancel, onSuccess }: RegisterProps) => {
    const { roles, loading: rolesLoading, error: rolesError } = useRoles();
    const { areas, loading: areasLoading, error: areasError } = useAreas();

  // Estados para el formulario de nuevo usuario
  const [formData, setFormData] = useState<RegisterRequest>({
    username: '',
    fullName: '',
    password: '',
    roles: [],
    areaId: null,
  });
  
  // Estado para manejar errores
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  
  // Estados para el campo de contraseña
  const [showPassword, setShowPassword] = useState(false);
  
  // Función para generar contraseña aleatoria
  const generateRandomPassword = () => {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let password = '';
    for (let i = 0; i < 8; i++) {
      password += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return password;
  };
  
  const handleGeneratePassword = () => {
    const newPassword = generateRandomPassword();
    setFormData(prev => ({ ...prev, password: newPassword }));
    toast.success('Contraseña generada exitosamente');
  };

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => {
        if (field === 'roles') {
            // Almacenar roles como strings
            const currentRoles = prev.roles || [];
            const updatedRoles = currentRoles.includes(value)
                ? currentRoles.filter(r => r !== value)
                : [...currentRoles, value];

            return {
                ...prev,
                roles: updatedRoles
            };
        }
        // MULTI-AREAS (number[])
        if (field === 'areaId') {
            return {
                ...prev,
                areaId: Number(value)
            };
        }
      //if (field === 'AreaId') {
      //  const areaId = parseInt(value) || 0;
        
      //  return {
      //    ...prev,
      //    AreaId: areaId,
      //    GrupoId: 0 // Reset group when area changes
      //  };
      //}
    //  if (field === 'GrupoId') {
    //    return {
    //      ...prev,
    //      GrupoId: parseInt(value) || 0
    //    };
    //  }
      return {
        ...prev,
        [field]: value
      };
    }
    );
  };

  const handleSaveUser = async () => {
    if (!formData.fullName || !formData.username || !formData.roles.length || !formData.password) {
      setError('Todos los campos son obligatorios');
      return;
    }
    try {
      setIsLoading(true);
      setError(null);
      
      const data = {
          ...formData,
          password: formData.password || generateRandomPassword(),
          isValidated: true
      }
        console.log('Guardando usuario:', { data });
        console.log("ENVIANDO A BACKEND:", JSON.stringify(data, null, 2));
      const response = await authService.register(data);
      console.log('Usuario creado con éxito', {response});
      
      // Reset form and call success callback
      setFormData({
        username: '',
        fullName: '',
        password: '',
        roles: [],
        areaId: null,
      });
      toast.success('Usuario creado con éxito');
      setTimeout(() => {
        onSuccess();
      }, 1000);
    } catch (error) {
      console.error('Error al crear usuario:', error);
      if (error instanceof Error) {

        const isJson = error.message.startsWith('{') && error.message.endsWith('}');
        if(isJson) {
          const jsonError = JSON.parse(error.message);
          setError( Object.values(jsonError.errors).join('\n'));
        } else {
          setError(error.message);
        }
      } else if (typeof error === 'string') {
        setError(error);
      } else {
        setError('Error desconocido al crear usuario');
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleCancelForm = () => {
    // Resetear formulario y llamar callback de cancelación
    setFormData({
      username: '',
      fullName: '',
      password: '',
      roles: [],
      areaId: null,
    });
    onCancel();
  };

  // Filtrar roles no permitidos en el selector
  const filteredRoles = roles.filter(r => {
    const n = r.name.trim().toLowerCase();
    return !(
      n === 'super usuario' ||
      n === 'empleado sindicalizado' ||
      n === 'delegado sindical'
    );
  });

  return (
    <div className="p-6 bg-white h-screen">
      <div className="max-w-7xl mx-auto w-full space-y-8">
        {/* Título */}
        <div className="text-[25px] font-bold text-continental-black text-left">
          Nuevo Usuario
        </div>
        
        {/* Error Messages */}
        {(error || rolesError) && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md">
            <div className="flex items-center">
              <span className="text-red-500 mr-2">⚠️</span>
              <span>{error || rolesError}</span>
            </div>
          </div>
        )}

        {/* Formulario */}
        <div className="space-y-6">
          {/* Primera fila: Nombre y Username */}
          <div className="grid grid-cols-2 gap-6">
            <div className="space-y-2">
              <Label className="text-base font-medium text-continental-black">
                Nombre completo
              </Label>
              <Input
                type="text"
                value={formData.fullName}
                onChange={(e) => handleInputChange('fullName', e.target.value)}
                placeholder="Ingrese el nombre completo"
                className="w-full"
              />
            </div>
            <div className="space-y-2">
              <Label className="text-base font-medium text-continental-black">
                Correo electrónico
              </Label>
              <Input
                type="email"
                value={formData.username}
                onChange={(e) => handleInputChange('username', e.target.value)}
                placeholder="Ingrese el correo electrónico"
                className="w-full"
              />
            </div>
          </div>

          {/* Segunda fila: Roles */}
          <div className="grid grid-cols-2 gap-6">
            <div className="space-y-2">
              <Label className="text-base font-medium text-continental-black">
                Roles
              </Label>
              <div className="border rounded-md p-2 min-h-[40px] bg-white">
                {rolesLoading && (
                  <div className="text-gray-500 text-sm">Cargando roles...</div>
                )}
                {!rolesLoading && roles.length === 0 && (
                  <div className="text-gray-500 text-sm">No hay roles disponibles</div>
                )}
                {/* Display selected roles */}
                <div className="flex flex-wrap gap-1 mb-2">
                  {formData.roles.map((roleId, index) => {
                    // Find role by ID
                    const role = roles.find(r => r.id.toString() === roleId.toString());
                    const roleName = role ? role.name : `Role ${roleId}`;
                    
                    return (
                      <span
                        key={index}
                        className="bg-continental-yellow text-continental-black px-2 py-1 rounded-sm text-sm flex items-center gap-1"
                      >
                        {roleName}
                        <button
                          type="button"
                          onClick={() => handleInputChange('roles', roleId.toString())}
                          className="ml-1 text-red-500 hover:text-red-700"
                        >
                          ×
                        </button>
                      </span>
                    );
                  })}
                </div>
                
                {/* Role selection dropdown */}
                <Select onValueChange={(value) => handleInputChange('roles', value)}>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Seleccionar roles" />
                  </SelectTrigger>
                  <SelectContent>
                    {filteredRoles.map(role => {
                      const roleId = role?.id?.toString();
                        const isSelected = formData.roles.includes(roleId);
                      
                      return (
                        <SelectItem 
                          key={role?.id} 
                          value={roleId}
                          className={isSelected ? 'bg-gray-100' : ''}
                        >
                          <div className="flex items-center gap-2">
                            {isSelected && <span>✓</span>}
                            {role.name}
                          </div>
                        </SelectItem>
                      );
                    })}
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div className="space-y-2">
              <Label className="text-base font-medium text-continental-black">
                Contraseña
              </Label>
              <div className="relative">
                <Input
                  type={showPassword ? "text" : "password"}
                  value={formData.password}
                  onChange={(e) => handleInputChange('password', e.target.value)}
                  placeholder="Ingrese la contraseña o genere una"
                  className="w-full pr-20"
                />
                <div className="absolute right-2 top-1/2 transform -translate-y-1/2 flex gap-1">
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={handleGeneratePassword}
                    className="p-1 h-8 w-8 hover:bg-gray-100"
                    title="Generar contraseña aleatoria"
                  >
                    <Shuffle className="h-4 w-4" />
                  </Button>
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={() => setShowPassword(!showPassword)}
                    className="p-1 h-8 w-8 hover:bg-gray-100"
                    title={showPassword ? "Ocultar contraseña" : "Mostrar contraseña"}
                  >
                    {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                  </Button>
                </div>
              </div>
              {formData.password && (
                <p className="text-xs text-gray-600">
                  Longitud: {formData.password.length} caracteres
                </p>
              )}
            </div>
          </div>
                  {/* Selección de Área */}
                  <div className="space-y-2">
                      <Label className="text-base font-medium text-continental-black">
                          Área
                      </Label>

                      <Select
                          value={formData.areaId?.toString() ?? ""}
                          onValueChange={(value) => handleInputChange("areaId", value)}
                          disabled={areasLoading}
                      >
                          <SelectTrigger>
                              <SelectValue placeholder="Seleccionar área" />
                          </SelectTrigger>

                          <SelectContent>
                              {areas && areas.length > 0 ? (
                                  areas.map(a => (
                                      <SelectItem key={a.areaId} value={a.areaId.toString()}>
                                          {a.nombreGeneral}
                                      </SelectItem>
                                  ))
                              ) : (
                                  <SelectItem value="no-area" disabled>
                                      No hay áreas disponibles
                                  </SelectItem>
                              )}
                          </SelectContent>
                      </Select>

                      {areasLoading && (
                          <p className="text-sm text-gray-500">Cargando áreas...</p>
                      )}
                      {areasError && (
                          <p className="text-sm text-red-500">{areasError}</p>
                      )}
                  </div>
          {/* Texto informativo */}
          <div className="text-left">
            <p className="text-[16px] font-medium text-continental-black">
              Tras crear la cuenta, el usuario recibirá por correo su nombre de usuario y una contraseña temporal,<br />
              la cual podrá cambiar más adelante.
            </p>
          </div>

          {/* Botones */}
          <div className="flex justify-center gap-4 pt-6">
            <Button
              variant="outline"
              onClick={handleCancelForm}
              className="w-28 h-10 border-continental-black text-continental-black hover:bg-continental-gray-4"
            >
              Cancelar
            </Button>
            <Button
              variant="continental"
              onClick={handleSaveUser}
              disabled={isLoading}
              className="w-28 h-10"
            >
              {isLoading ? 'Guardando...' : 'Guardar'}
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};