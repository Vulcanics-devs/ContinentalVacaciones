
export const Info = ({nomina, nombre, area, grupo}: {nomina: string, nombre: string, area: string, grupo: string}) => {
  return (
    <div className="flex flex-col gap-2 border border-continental-gray-3 rounded-md p-4 bg-continental-white justify-center items-center ">
      <p className="text-continental-gray-1 font-bold text-2xl">{nomina}</p>
      <p className="text-continental-gray-1">{nombre}</p>
      <p className="text-continental-gray-1 ">{area}</p>
      <p className="text-continental-gray-1 ">{grupo}</p>
    </div>
  )
}

