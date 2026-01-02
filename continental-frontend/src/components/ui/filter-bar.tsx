import { Input } from './input';
import { Label } from './label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './select';
import { Search } from 'lucide-react';

// Interfaces
export interface FilterOption {
  value: string;
  label: string;
}

export interface FilterConfig {
  type: 'search' | 'select';
  key: string;
  label?: string;
  placeholder?: string;
  options?: FilterOption[];
  value: string;
  onChange: (value: string) => void;
  className?: string;
}

export interface FilterBarProps {
  filters: FilterConfig[];
  className?: string;
  gridCols?: number;
}

export const FilterBar = ({ 
  filters, 
  className = "",
}: FilterBarProps) => {
  // Usar grid-cols-3 fijo para los filtros de plantilla
  const gridClass = "grid-cols-3";

  return (
    <div className={`grid ${gridClass} gap-4 mb-6 ${className}`}>
      {filters.map((filter) => (
        <div key={filter.key} className={filter.className || ''}>
          {filter.type === 'search' && (
            <div className="relative">
              <Search 
                size={16} 
                className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" 
              />
              <Input
                type="text"
                placeholder={filter.placeholder || 'Buscar...'}
                value={filter.value}
                onChange={(e) => filter.onChange(e.target.value)}
                className="w-full border-continental-gray-3 pl-10"
              />
            </div>
          )}

          {filter.type === 'select' && (
            <div className="flex items-center gap-2 w-full">
              {filter.label && (
                <Label className="text-continental-black font-medium whitespace-nowrap min-w-fit">
                  {filter.label}:
                </Label>
              )}
              <Select value={filter.value} onValueChange={filter.onChange}>
                <SelectTrigger className="border-continental-gray-3 flex-1">
                  <SelectValue placeholder={filter.placeholder || 'Seleccionar...'} />
                </SelectTrigger>
                <SelectContent>
                  {filter.options?.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          )}
        </div>
      ))}
    </div>
  );
};