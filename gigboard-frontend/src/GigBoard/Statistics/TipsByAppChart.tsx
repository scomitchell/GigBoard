import Plot from "react-plotly.js";
import { useIsMobile } from "../../hooks/useIsMobile";

export type TipsByAppProps = {
    data: {
        tipApps: string[],
        tipPays: number[]
    }
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

export default function TipsByAppChart({data}: TipsByAppProps) {
    const isMobile = useIsMobile();
    const config = getResponsiveConfig(isMobile);

    const chartData = [
      {
        x: data.tipApps,
        y: data.tipPays,
        type: "bar",
        marker: { color: "#6366F1" },
        name: "Tips by app",
        orientation: "v",
        hoverTemplate: `$%{y:.2f}<br>%{x}`,
      },
    ];

    const layout = {
        autosize: true,
        automargin: true,
        title: { text: isMobile ? "Tip by App" : "Average Tip by App", font: { size: config.titleFontSize, weight: "bold" }},
        xaxis: {
            title: {text: "App", font: { size: config.axisLabelFontSize }, standoff: 20 },
            tickangle: config.tickAngle,
            zeroline: false,
            showgrid: true,
            tickfont: { size: config.tickFontSize }
        },
        yaxis: {
            title: {
                text: isMobile ? "Tip ($)" : "Average Tip ($)", 
                font: { size: config.axisLabelFontSize }, 
                standoff: 20
            },
            showgrid: true,
            zeroline: false,
            tickprefix: "$",
            tickformat: ".2f",
            tickfont: { size: config.tickFontSize }
        },
        margin: config.margins,
        plot_bgcolor: "white",
        paper_bgcolor: "white",
        dragmode: false
    };

    return (
        <div style={{
            minHeight: isMobile ? 350 : 450,
            minWidth: 0,
            width: "100%",
            position: "relative",
            overflowX: "auto"
        }}>
            <Plot 
                data={chartData}
                layout={layout}
                config={{
                     responsive: true,
                     displayModeBar: false,
                     displaylogo: false,
                     scrollZoom: false,
                }}
                style={{height: "100%", width: "100%"}}
            />
        </div>
    )
}