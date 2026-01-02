import  { PeriodOptions, type Period } from "@/interfaces/Calendar.interface"





export const PeriodLight = ({currenPeriod}: {currenPeriod: Period}) => {
  return (
    <div className="">
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* Período de Solicitudes Anuales */}
        <div className={`relative overflow-hidden rounded-lg border p-3 transition-all duration-300 ${
          currenPeriod === PeriodOptions.annual 
            ? 'border-green-300 bg-green-50 shadow-md' 
            : 'border-gray-200 bg-gray-50'
        }`}>
          <div className="flex items-center justify-between mb-1">
            <div className="flex items-center gap-2">
              <div className={`relative w-2.5 h-2.5 rounded-full ${
                currenPeriod === PeriodOptions.annual ? 'bg-green-500' : 'bg-gray-400'
              }`}>
                {currenPeriod === PeriodOptions.annual && (
                  <div className="absolute inset-0 rounded-full bg-green-500 animate-pulse"></div>
                )}
              </div>
              <h4 className="font-medium text-gray-800 text-sm">Solicitudes Anuales</h4>
            </div>
            <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
              currenPeriod === PeriodOptions.annual 
                ? 'bg-green-100 text-green-800 border border-green-200' 
                : 'bg-gray-100 text-gray-600 border border-gray-200'
            }`}>
              {currenPeriod === PeriodOptions.annual ? '🟢 Activo' : '🔴 Inactivo'}
            </span>
          </div>
          <p className="text-xs text-gray-600">
            {currenPeriod === PeriodOptions.annual 
              ? 'Solicitar nuevas vacaciones'
              : 'No se pueden crear solicitudes'
            }
          </p>
          
          {/* Indicador visual de estado activo */}
          {currenPeriod === PeriodOptions.annual && (
            <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-green-400 to-green-600"></div>
          )}
        </div>

        {/* Período de Reprogramación */}
        <div className={`relative overflow-hidden rounded-lg border p-3 transition-all duration-300 ${
          currenPeriod === PeriodOptions.reprogramming 
            ? 'border-green-300 bg-green-50 shadow-md' 
            : 'border-gray-200 bg-gray-50'
        }`}>
          <div className="flex items-center justify-between mb-1">
            <div className="flex items-center gap-2">
              <div className={`relative w-2.5 h-2.5 rounded-full ${
                currenPeriod === PeriodOptions.reprogramming ? 'bg-green-500' : 'bg-gray-400'
              }`}>
                {currenPeriod === PeriodOptions.reprogramming && (
                  <div className="absolute inset-0 rounded-full bg-green-500 animate-pulse"></div>
                )}
              </div>
              <h4 className="font-medium text-gray-800 text-sm">Reprogramación</h4>
            </div>
            <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
              currenPeriod === PeriodOptions.reprogramming 
                ? 'bg-green-100 text-green-800 border border-green-200' 
                : 'bg-gray-100 text-gray-600 border border-gray-200'
            }`}>
              {currenPeriod === PeriodOptions.reprogramming ? '🟢 Activo' : '🔴 Inactivo'}
            </span>
          </div>
          <p className="text-xs text-gray-600">
            {currenPeriod === PeriodOptions.reprogramming 
              ? 'Reprogramar vacaciones existentes'
              : 'No se pueden modificar vacaciones'
            }
          </p>
          
          {/* Indicador visual de estado activo */}
          {currenPeriod === PeriodOptions.reprogramming && (
            <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-green-500 to-green-600"></div>
          )}
        </div>
      </div>

    </div>
  )
}
