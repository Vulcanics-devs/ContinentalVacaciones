import type { ReactNode } from 'react';
import { ArrowUpDown, ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from 'lucide-react';
import { Button } from './button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './select';

// Interfaces
export interface Column<T = Record<string, any>> {
  key: string;
  label: string;
  sortable?: boolean;
  render?: (value: unknown, row: T) => ReactNode;
  className?: string;
}

export interface PaginationConfig {
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalItems: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
}

export interface DataTableProps<T = Record<string, any>> {
  columns: Column<T>[];
  data: T[];
  keyField: string;
  emptyMessage?: string;
  className?: string;
  headerClassName?: string;
  rowClassName?: string;
  showSortIcons?: boolean;
  onSort?: (column: string) => void;
  pagination?: PaginationConfig;
}

export const DataTable = <T extends Record<string, any>>({
  columns,
  data,
  keyField,
  emptyMessage = "No hay datos disponibles",
  className = "",
  headerClassName = "bg-continental-gray-4",
  rowClassName = "",
  showSortIcons = true,
  onSort,
  pagination
}: DataTableProps<T>) => {

  return (
    <div className={`overflow-hidden ${className}`}>
      {/* Header de la tabla */}
     <div
        className={`${headerClassName} grid`}
        style={{ gridTemplateColumns: `repeat(${columns.length}, minmax(0, 1fr))` }}
      >
        {columns.map((column) => (
          <div
            key={column.key}
            className={`p-4 text-sm font-medium text-continental-black flex items-center justify-between ${column.className || ''}`}
            onClick={() => column.sortable && onSort?.(column.key)}
          >
            <span>{column.label}</span>
            {showSortIcons && column.sortable && (
              <ArrowUpDown
                size={14}
                className={`text-continental-gray-1 ${column.sortable ? 'cursor-pointer' : ''}`}
              />
            )}
          </div>
        ))}
      </div>

      {/* Línea horizontal negra debajo de los títulos */}
      <div className="border-b-2 border-continental-black"></div>

      {/* Filas de la tabla */}
      {data.map((row) => (
        <div
          key={String(row[keyField])}
          className={`grid border-b border-continental-gray-3 last:border-b-0 ${rowClassName}`}
          style={{ gridTemplateColumns: `repeat(${columns.length}, minmax(0, 1fr))` }}
        >
          {columns.map((column) => (
            <div
              key={column.key}
              className={`p-2 text-sm text-continental-black ${column.className || ''}`}
            >
              {column.render ? column.render(row[column.key], row) : String(row[column.key] ?? '')}
            </div>
          ))}
        </div>
      ))}

      {/* Mensaje cuando no hay resultados */}
      {data.length === 0 && (
        <div className="p-8 text-center text-continental-gray-1">
          {emptyMessage}
        </div>
      )}

      {/* Paginación */}
      {pagination && (
        <div className="flex items-center justify-between px-4 py-4 border-t border-continental-gray-3 bg-white">
          {/* Información de registros */}
          <div className="flex items-center gap-4">
            <span className="text-sm text-continental-black">
              Mostrando {pagination?.currentPage ? ((pagination.currentPage - 1) * pagination.pageSize) + 1 : 0} a{' '}
              {pagination?.currentPage ? Math.min(pagination.currentPage * pagination.pageSize, pagination.totalItems) : 0} de{' '}
              {pagination?.totalItems?.toLocaleString() || 0} registros
            </span>
            
            {/* Selector de tamaño de página */}
            <div className="flex items-center gap-2">
              <span className="text-sm text-continental-black">Mostrar:</span>
              <Select
                value={pagination?.pageSize?.toString()}
                onValueChange={(value) => pagination?.onPageSizeChange(parseInt(value))}
              >
                <SelectTrigger className="w-20 h-8">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="10">10</SelectItem>
                  <SelectItem value="25">25</SelectItem>
                  <SelectItem value="50">50</SelectItem>
                  <SelectItem value="1000">1000</SelectItem>
                
                </SelectContent>
              </Select>
            </div>
          </div>

          {/* Controles de navegación */}
          <div className="flex items-center gap-2">
            {/* Primera página */}
            <Button
              variant="outline"
              size="sm"
              onClick={() => pagination?.onPageChange(1)}
              disabled={pagination?.currentPage === 1}
              className="h-8 w-8 p-0"
            >
              <ChevronsLeft size={16} />
            </Button>

            {/* Página anterior */}
            <Button
              variant="outline"
              size="sm"
              onClick={() => pagination?.onPageChange(pagination?.currentPage - 1)}
              disabled={pagination?.currentPage === 1}
              className="h-8 w-8 p-0"
            >
              <ChevronLeft size={16} />
            </Button>

            {/* Números de página */}
            <div className="flex items-center gap-1">
              {getPageNumbers(pagination?.currentPage, pagination?.totalPages).map((pageNum, index) => (
                pageNum === '...' ? (
                  <span key={`ellipsis-${index}`} className="px-2 text-continental-gray-1">...</span>
                ) : (
                  <Button
                    key={pageNum}
                    variant={pagination?.currentPage === pageNum ? "continental" : "outline"}
                    size="sm"
                    onClick={() => pagination?.onPageChange(pageNum as number)}
                    className="h-8 w-8 p-0"
                  >
                    {pageNum}
                  </Button>
                )
              ))}
            </div>

            {/* Página siguiente */}
            <Button
              variant="outline"
              size="sm"
              onClick={() => pagination?.onPageChange(pagination?.currentPage + 1)}
              disabled={pagination?.currentPage === pagination?.totalPages}
              className="h-8 w-8 p-0"
            >
              <ChevronRight size={16} />
            </Button>

            {/* Última página */}
            <Button
              variant="outline"
              size="sm"
              onClick={() => pagination?.onPageChange(pagination?.totalPages)}
              disabled={pagination?.currentPage === pagination?.totalPages}
              className="h-8 w-8 p-0"
            >
              <ChevronsRight size={16} />
            </Button>
          </div>
        </div>
      )}
    </div>
  );
};

// Función auxiliar para generar números de página con elipsis
const getPageNumbers = (currentPage: number, totalPages: number): (number | string)[] => {
  const pages: (number | string)[] = [];
  const maxVisiblePages = 5;

  if (totalPages <= maxVisiblePages) {
    // Si hay pocas páginas, mostrar todas
    for (let i = 1; i <= totalPages; i++) {
      pages.push(i);
    }
  } else {
    // Siempre mostrar la primera página
    pages.push(1);

    if (currentPage > 3) {
      pages.push('...');
    }

    // Mostrar páginas alrededor de la actual
    const start = Math.max(2, currentPage - 1);
    const end = Math.min(totalPages - 1, currentPage + 1);

    for (let i = start; i <= end; i++) {
      if (i !== 1 && i !== totalPages) {
        pages.push(i);
      }
    }

    if (currentPage < totalPages - 2) {
      pages.push('...');
    }

    // Siempre mostrar la última página
    if (totalPages > 1) {
      pages.push(totalPages);
    }
  }

  return pages;
};