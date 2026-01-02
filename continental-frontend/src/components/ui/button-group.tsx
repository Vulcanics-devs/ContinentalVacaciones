import * as React from "react"
import { cn } from "@/lib/utils"

interface ButtonGroupProps {
  options: string[]
  value?: string
  onValueChange?: (value: string) => void
  className?: string
}

const ButtonGroup = React.forwardRef<HTMLDivElement, ButtonGroupProps>(
  ({ options, value, onValueChange, className }, ref) => {
    return (
      <div
        ref={ref}
        className={cn("flex gap-2", className)}
      >
        {options.map((option) => (
          <button
            key={option}
            onClick={() => onValueChange?.(option)}
            className={cn(
              "px-4 py-2 rounded-md text-sm font-medium transition-colors",
              value === option
                ? "bg-continental-yellow text-continental-black"
                : "bg-gray-100 text-gray-700 hover:bg-gray-200"
            )}
          >
            {option}
          </button>
        ))}
      </div>
    )
  }
)
ButtonGroup.displayName = "ButtonGroup"

export { ButtonGroup }