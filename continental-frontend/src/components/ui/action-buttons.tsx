import type { ReactNode } from 'react';
import { Button } from './button';

// Interfaces
export interface ActionButtonConfig {
  key: string;
  label: string;
  variant?: 'default' | 'destructive' | 'outline' | 'secondary' | 'ghost' | 'link' | 'continental';
  size?: 'default' | 'sm' | 'lg' | 'icon';
  onClick: () => void;
  disabled?: boolean;
  icon?: ReactNode;
  className?: string;
}

export interface ActionButtonsProps {
  buttons: ActionButtonConfig[];
  className?: string;
  alignment?: 'left' | 'center' | 'right';
  gap?: string;
}

export const ActionButtons = ({ 
  buttons, 
  className = "",
  alignment = 'center',
  gap = 'gap-4'
}: ActionButtonsProps) => {
  const alignmentClass = {
    left: 'justify-start',
    center: 'justify-center',
    right: 'justify-end'
  }[alignment];

  return (
    <div className={`flex ${alignmentClass} ${gap} pt-6 ${className}`}>
      {buttons.map((button) => (
        <Button
          key={button.key}
          onClick={button.onClick}
          variant={button.variant || 'default'}
          size={button.size || 'default'}
          disabled={button.disabled}
          className={`${button.className || ''}`}
        >
          {button.icon && <span className="mr-2">{button.icon}</span>}
          {button.label}
        </Button>
      ))}
    </div>
  );
};