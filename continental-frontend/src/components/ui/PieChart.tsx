import React from "react";

interface PieChartSegment {
  value: number;
  color: string;
  label: string;
}

interface PieChartProps {
  segments: PieChartSegment[];
  size?: number;
  centerContent?: React.ReactNode;
  showLegend?: boolean;
  legendPosition?: "right" | "bottom";
}

export const PieChart: React.FC<PieChartProps> = ({
  segments,
  size = 192,
  centerContent,
  showLegend = true,
  legendPosition = "right",
}) => {
  const total = segments.reduce((sum, segment) => sum + segment.value, 0);

  const createGradient = () => {
    let currentAngle = 0;
    const gradientParts: string[] = [];

    segments.forEach((segment) => {
      const percentage = (segment.value / total) * 100;
      const endAngle = currentAngle + (percentage * 3.6);

      if (percentage > 0) {
        gradientParts.push(
          `${segment.color} ${currentAngle}deg ${endAngle}deg`
        );
      }

      currentAngle = endAngle;
    });

    return `conic-gradient(${gradientParts.join(", ")})`;
  };

  const legendClasses = legendPosition === "right"
    ? "flex items-center justify-center gap-8"
    : "flex flex-col items-center gap-4";

  const legendContainerClasses = legendPosition === "right"
    ? "space-y-3"
    : "flex flex-wrap justify-center gap-4";

  return (
    <div className={showLegend ? legendClasses : "flex justify-center"}>
      <div
        className="relative"
        style={{ width: size, height: size }}
      >
        <div className="w-full h-full rounded-full border-8 border-gray-200 relative overflow-hidden">
          <div
            className="absolute inset-0 rounded-full"
            style={{
              background: createGradient(),
            }}
          />

          {centerContent && (
            <div className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 w-16 h-16 bg-white rounded-full border-4 border-gray-200 flex items-center justify-center">
              {centerContent}
            </div>
          )}
        </div>
      </div>

      {showLegend && (
        <div className={legendContainerClasses}>
          {segments.map((segment, index) => (
            <div key={index} className="flex items-center gap-2">
              <div
                className="w-3 h-3 rounded"
                style={{ backgroundColor: segment.color }}
              />
              <span className="text-sm text-gray-700">{segment.label}</span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};