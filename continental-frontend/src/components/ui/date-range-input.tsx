import * as React from "react"
import { cn } from "@/lib/utils"
import { Input } from "./input"
import { Label } from "./label"

interface DateRangeInputProps {
    startDate?: string
    endDate?: string
    onStartDateChange?: (date: string) => void
    onEndDateChange?: (date: string) => void
    className?: string
    title?: string
    startLabel?: string
    endLabel?: string
    minDate?: string
}

const DateRangeInput = React.forwardRef<HTMLDivElement, DateRangeInputProps>(
    ({
        startDate,
        endDate,
        onStartDateChange,
        onEndDateChange,
        className,
        title = "Periodo del reporte",
        startLabel = "Fecha inicio",
        endLabel = "Fecha fin",
        minDate
    }, ref) => {
        return (
            <div ref={ref} className={cn("space-y-4", className)}>
                {title && (
                    <h3 className="text-xl font-bold text-continental-black text-left">
                        {title}
                    </h3>
                )}
                <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                        <Label className="text-base font-bold text-gray-600">
                            {startLabel}
                        </Label>
                        <Input
                            type="date"
                            value={startDate}
                            min={minDate}
                            onChange={(e) => onStartDateChange?.(e.target.value)}
                            className="rounded-xl border-gray-300"
                        />
                    </div>
                    <div className="space-y-2">
                        <Label className="text-base font-bold text-gray-600">
                            {endLabel}
                        </Label>
                        <Input
                            type="date"
                            value={endDate}
                            min={minDate}
                            onChange={(e) => onEndDateChange?.(e.target.value)}
                            className="rounded-xl border-gray-300"
                        />
                    </div>
                </div>
            </div>
        )
    }
)
DateRangeInput.displayName = "DateRangeInput"

export { DateRangeInput }
