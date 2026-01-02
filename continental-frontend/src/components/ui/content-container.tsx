import type { ReactNode } from 'react';

export interface ContentContainerProps {
  children: ReactNode;
  title?: string;
  className?: string;
  bordered?: boolean;
  padding?: string;
  maxWidth?: string;
}

export const ContentContainer = ({
  children,
  title,
  className = "",
  bordered = true,
  padding = "p-6",
}: ContentContainerProps) => {
  const borderClass = bordered ? "border border-continental-gray-3 rounded-lg" : "";

  return (
    <div className={`${borderClass} ${padding} ${className}`}>
      {title && (
        <h3 className="text-lg font-semibold text-continental-black mb-4">
          {title}
        </h3>
      )}
      {children}
    </div>
  );
};