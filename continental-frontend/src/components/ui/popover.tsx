import React, { useEffect, useMemo, useRef, useState } from 'react';

export type PopoverPlacement = 'top' | 'bottom' | 'left' | 'right';

export interface PopoverProps {
  trigger: React.ReactNode;
  content: React.ReactNode;
  open?: boolean; // controlled
  defaultOpen?: boolean; // uncontrolled default
  onOpenChange?: (open: boolean) => void;
  placement?: PopoverPlacement;
  offset?: number;
  className?: string;
  triggerMode?: 'click' | 'hover';
}

export const Popover: React.FC<PopoverProps> = ({
  trigger,
  content,
  open,
  defaultOpen = false,
  onOpenChange,
  placement = 'top',
  offset = 8,
  className = '',
  triggerMode = 'click',
}) => {
  const [uncontrolledOpen, setUncontrolledOpen] = useState(defaultOpen);
  const isControlled = typeof open === 'boolean';
  const isOpen = isControlled ? (open as boolean) : uncontrolledOpen;

  const rootRef = useRef<HTMLDivElement | null>(null);
  const contentRef = useRef<HTMLDivElement | null>(null);

  const setOpenInternal = (next: boolean) => {
    if (!isControlled) setUncontrolledOpen(next);
    onOpenChange?.(next);
  };
  const toggle = () => setOpenInternal(!isOpen);

  // Close on outside click
  useEffect(() => {
    const onDocClick = (e: MouseEvent) => {
      if (!isOpen) return;
      const target = e.target as Node;
      if (rootRef.current && !rootRef.current.contains(target)) {
        if (!isControlled) setUncontrolledOpen(false);
        onOpenChange?.(false);
      }
    };
    document.addEventListener('mousedown', onDocClick);
    return () => document.removeEventListener('mousedown', onDocClick);
  }, [isOpen, isControlled, onOpenChange]);

  // Close on Escape
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        if (!isControlled) setUncontrolledOpen(false);
        onOpenChange?.(false);
      }
    };
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, [isControlled, onOpenChange]);

  const positionClasses = useMemo(() => {
    switch (placement) {
      case 'bottom':
        return `top-full left-1/2 -translate-x-1/2 mt-[${offset}px]`;
      case 'left':
        return `right-full top-1/2 -translate-y-1/2 mr-[${offset}px]`;
      case 'right':
        return `left-full top-1/2 -translate-y-1/2 ml-[${offset}px]`;
      case 'top':
      default:
        return `bottom-full left-1/2 -translate-x-1/2 mb-[${offset}px]`;
    }
  }, [placement, offset]);

  const triggerProps =
    triggerMode === 'hover'
      ? {
          onMouseEnter: () => setOpenInternal(true),
          onMouseLeave: () => setOpenInternal(false),
        }
      : {
          onClick: toggle,
          onKeyDown: (e: React.KeyboardEvent) => e.key === 'Enter' && toggle(),
        };

  return (
    <div ref={rootRef} className={`relative inline-block ${className}`}>
      <div
        role="button"
        tabIndex={0}
        {...triggerProps}
      >
        {trigger}
      </div>
      {isOpen && (
        <div
          ref={contentRef}
          className={`absolute z-50 ${positionClasses}`}
          onMouseEnter={triggerMode === 'hover' ? () => setOpenInternal(true) : undefined}
          onMouseLeave={triggerMode === 'hover' ? () => setOpenInternal(false) : undefined}
        >
          <div className="rounded-md border border-gray-200 bg-white p-3 shadow-md text-sm text-gray-800 min-w-[12rem] max-w-[20rem]">
            {content}
          </div>
        </div>
      )}
    </div>
  );
};

export default Popover;
