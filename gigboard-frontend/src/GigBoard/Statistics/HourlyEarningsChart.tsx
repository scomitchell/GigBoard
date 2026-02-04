import Plot from "react-plotly.js"

export type HourlyEarningsProps = {
    data: {
        hours: string[],
        earnings: number[]
    };
};

export default function HourlyPayChart({data}: HourlyEarningsProps) {

    const formattedHours = data.hours.map(h => {
        const hour = Number(h);
        const hh = hour.toString().padStart(2, '0');
        return `${hh}:00`;
    });

    return (
        <div style={{
            minHeight: 450,
            minWidth: 0,
            width: "100%",
            position: "relative",
            overflowX: "auto"
        }}>
            <Plot
                data={[{
                    x: data.hours,
                    y: data.earnings,
                    type: "scatter",
                    mode: "lines+markers",
                    marker: { color: "royalblue", size: 8 },
                    line: { width: 2 },
                    name: "Average earnings at this hour (Past 7 days)",
                    text: formattedHours,
                    hovertemplate: '%{text}<br />$%{y:.2f}',
                }]}
                layout={{
                    title: { text: "Total Earnings by Hour (Past 7 Days)", font: { size: 20, weight: "bold" } },
                    xaxis: {
                        title: { text: "Hour", font: { size: 16 } },
                        tickangle: -45,
                        showgrid: true,
                        zeroline: false,
                        tickformat: '%H:%M:%S',
                        hoverformat: '%H:%M:%S'
                    },
                    yaxis: {
                        title: { text: "Earnings ($)", font: { size: 16 } },
                        showgrid: true,
                        zeroline: true,
                        tickformat: ".1f"
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
                    displayModeBar: false
                }}
                style={{ width: "100%", height: "100%" }}
            />
        </div>
    );
}