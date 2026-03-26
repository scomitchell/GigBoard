import Plot from "react-plotly.js";
import { useIsMobile } from "../../hooks/useIsMobile";

export type BaseByAppProps = {
    data: {
        apps: string[],
        basePays: number[]
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

export default function BaseByAppsChart({data}: BaseByAppProps) {
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
              x: data.apps,
              y: data.basePays,
              type: "bar",
              marker: { color: "#6366F1" },
              name: "Base Pays by App",
              hoverTemplate: `$%{y:.2f}<br>%{x}`,
            },
          ]}
          layout={{
            title: {
              text: isMobile ? "Base Pay by App" : "Average Base Pay by App",
              font: { size: config.titleFontSize, weight: "bold" },
            },
            xaxis: {
              title: { text: "App", font: { size: config.axisLabelFontSize }, standoff: 20 },
              tickangle: config.tickAngle,
              showgrid: true,
              zeroline: false,
              tickfont: { size: config.tickFontSize },
            },
            yaxis: {
              title: {
                text: isMobile ? "Base Pay ($)" : "Average Base Pay ($)",
                font: { size: config.axisLabelFontSize },
                standoff: 20,
              },
              showgrid: true,
              zeroline: false,
              tickprefix: "$",
              tickformat: ".2f",
              tickfont: { size: config.tickFontSize },
            },
            plot_bgcolor: "white",
            paper_bgcolor: "white",
            autosize: true,
            automargin: true,
            margin: config.margins,
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