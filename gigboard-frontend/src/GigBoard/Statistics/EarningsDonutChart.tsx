import Plot from "react-plotly.js";
import { useIsMobile } from "../../hooks/useIsMobile";

export type EarningsDonutProps = {
    data: {
        totalPay: number,
        totalBasePay: number,
        totalTipPay: number
    };
};

export default function EarningsDonutChart({data}: EarningsDonutProps) {
    const isMobile = useIsMobile();
    const { totalPay, totalBasePay, totalTipPay } = data;

    const chartData = [
      {
        values: [totalBasePay, totalTipPay],
        labels: ["Base Pay", "Tip Pay"],
        type: "pie",
        hole: 0.8,
        texttemplate: isMobile ? `%{label}<br />%{percent}` : `%{label}<br />%{percent}<br />$%{value:.2f}`,
        textposition: "outside",
        textfont: {
          size: isMobile ? 11 : 14,
        },
        marker: {
          colors: ["#6366F1", "#10B981"],
        },
        hovertemplate: `%{label}<br />%{percent}<br />$%{value:.2f}<extra></extra>`,
      },
    ];

    const layout = {
        autosize: true,
        showlegend: false,
        margin: { t: 0, b: 0, l: 0, r: 0 },
        annotations: [
            {
                text: `$${totalPay.toFixed(2)}`,
                x: 0.5,
                y: 0.5,
                font: {
                    size: isMobile ? 18 : 24,
                    color: "black",
                    weight: "bold"
                },
                showarrow: false,
            },
        ],
    };

    return (
        <div style={{width: "100%",
            height: "100%",
            minHeight: isMobile ? 300 : 250,
            display: "flex",
            justifyContent: "center",
            alignItems: "center"
        }}>
            <Plot
                data={chartData}
                layout={layout}
                useResizeHandler={true}
                config={{ displayModeBar: false }}
                style={{ height: "100%", width: "100%" }}
            />
        </div>
    );
}