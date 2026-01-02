import Logo from "@/assets/Logo.webp";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../ui/select";
import { Button } from "../ui/button";
import { useNavigate } from "react-router-dom";
import { useState } from "react";

export const SeleccionRepresentante = () => {
    const [representante, setRepresentante] = useState('')
  const navigate = useNavigate()

  const handleSelectRepresentante = (representante: string) => {
    //guardar representante en localStorage
    localStorage.setItem('representante', representante)
    navigate('/empleados')
  }

  const handleSelectEmployee = () => {
    navigate('/empleados')
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-continental-yellow">
      <div className="w-[500px] h-[520px] bg-continental-white rounded-lg shadow-lg flex flex-col items-center p-8">
        {/* Logo - 1/3 del contenedor */}
        <div className="h-1/3 flex items-center justify-center mb-6">
          <img
            src={Logo}
            alt="Continental Logo"
            className="max-h-full max-w-full object-contain"
          />
        </div>
        <div className="flex flex-col gap-4 items-center justify-between h-full">
          <p>
            Eres representante sindical, por favor selecciona tu representante o
            entra con tu número de nómina
          </p>
          <div className="h-2/3 flex flex-col ">
            <Select value={representante} onValueChange={setRepresentante}>
              <SelectTrigger>
                <SelectValue placeholder="Seleccione un representante" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="representante1">Representante 1</SelectItem>
                <SelectItem value="representante2">Representante 2</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div className="w-full flex justify-between">
            <Button className="cursor-pointer" variant="continentalOutline" onClick={() => handleSelectEmployee()}>Entrar como empleado</Button>
            <Button className="cursor-pointer" variant="continental" onClick={() => handleSelectRepresentante(representante)}>Entrar como representante</Button>
          </div>
        </div>
      </div>
    </div>
  );
};
