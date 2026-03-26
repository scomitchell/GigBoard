import Plot from "react-plotly.js";
import { useIsMobile } from "../../hooks/useIsMobile";

export type HourlyEarningsProps = {
    data: {
        hours: string[],
        earnings: number[]
    };
};

const getResponsiveConfig = (isMobile: boolean) => {
  if (isMobile) {
    return {
      margins: { l: 50, r: 20, t: 80, b: 110 },
      titleFontSize: 14,
      axisLabelFontSize: 12,
      tickAngle: -45,
      tickFontSize: 10,
    };
  }
  return {
    margins: { l: 70, r: 30, t: 80, b: 90 },
    titleFontSize: 20,
    axisLabelFontSize: 16,
    tickAngle: -30,
    tickFontSize: 12,
  };
};

export default function EarningsByHourChart({ data }: HourlyEarningsProps) {
  const isMobile = useIsMobile();
  const config = getResponsiveConfig(isMobile);

  return (
    <div
      style={{
        minHeight: isMobile ? 350 : 450,
        minWidth: 0,
        width: "100%",
        position: "relative",
        overflowX: "auto",
      }}
    >
      <Plot
        data={[
          {
            x: data.hours,
            y: data.earnings,
            type: "bar",
            marker: { color: "#6366F1" },
            name: "Earnings",
            hoverTemplate: `$%{y:.2f}<br>%{x}`,
          },
        ]}
        layout={{
          autosize: true,
          title: {
            text: isMobile ? "Earnings by Hour" : "Pay by Hour of Day (Past 7 Days)",
            font: { size: config.titleFontSize, weight: "bold" },
          },
          xaxis: {
            title: { text: "Hour", font: { size: config.axisLabelFontSize }, standoff: 20 },
            tickangle: config.tickAngle,
            showgrid: true,
            zeroline: false,
            tickvals: data.hours,
            tickfont: { size: config.tickFontSize },
            ticktext: data.hours.map(
              (h) => `${h.toString().padStart(2, "0")}:00`,
            ),
          },
          yaxis: {
            title: { text: "Average Pay", font: { size: config.axisLabelFontSize }, standoff: 20 },
            showgrid: true,
            zeroline: false,
            tickprefix: "$",
            tickformat: ".2f",
            tickfont: { size: config.tickFontSize },
          },
          margin: config.margins,
          plot_bgcolor: "white",
          paper_bgcolor: "white",
          automargin: true,
          dragmode: false,
        }}
        config={{
          responsive: true,
          displayModeBar: false,
          displaylogo: false,
          scrollZoom: false,
        }}
        style={{ width: "100%", height: "100%" }}
      />
    </div>
  );
}
