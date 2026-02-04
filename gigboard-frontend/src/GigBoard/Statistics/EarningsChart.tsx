import Plot from "react-plotly.js";

export type EarningsChartProps = {
    data: {
        dates: string[],
        earnings: number[]
    };
};

export default function EarningsChart({ data }: EarningsChartProps)
{
    return (
        <div style={{
            minHeight: 450,
            minWidth: 0,
            width: "100%",
            position: "relative",
            overflowX: "auto"
        }}>
            <Plot
                data={[
                    {
                        x: data.dates,
                        y: data.earnings,
                        type: "scatter",
                        mode: "lines+markers",
                        marker: { color: "royalBlue", size: 8 },
                        line: { width: 2 },
                        name: "Earnings",
                        hovertemplate: '$%{y:.2f}<br>%{x}',
                    },
                ]}
                layout={{
                    title: { text: "Earnings Over Time", font: { size: 20, weight: "bold" } },
                    xaxis: {
                        title: { text: "Date", font: { size: 16 }, standoff: 30 },
                        type: 'date',
                        tickangle: -30,
                        showgrid: true,
                        zeroline: false,
                    },
                    yaxis: {
                        title: { text: "Earnings ($)", font: { size: 16 }, standoff: 20 },
                        showgrid: true,
                        zeroline: false,
                        tickformat: ".1f",
                    },
                    plot_bgcolor: "white",
                    paper_bgcolor: "white",
                    autosize: true,
                    automargin: true,
                    dragmode: false
                }}
                config={{
                    responsive: true,
                    displaylogo: false,
                    scrollZoom: false,
                    displayModeBar: false,
                }}
                style={{ width: "100%", height: "100%" }}
            />
        </div>
    );
}