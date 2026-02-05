import Plot from "react-plotly.js";

export type HourlyEarningsProps = {
    data: {
        hours: string[],
        earnings: number[]
    };
};

export default function EarningsByHourChart({ data }: HourlyEarningsProps) {
  return (
    <div
      style={{
        minHeight: 450,
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
            marker: { color: "royalblue" },
            name: "Earnings",
            hoverTemplate: `$%{y:.2f}<br>%{x}`,
          },
        ]}
        layout={{
          autosize: true,
          title: {
            text: "Pay by Hour of Day (Past 7 Days)",
            font: { size: 20, weight: "bold" },
          },
          xaxis: {
            title: { text: "Hour", font: { size: 16 }, standoff: 10 },
            tickangle: -30,
            showgrid: true,
            zeroline: false,
            tickvals: data.hours,
            ticktext: data.hours.map(h => `${h.toString().padStart(2, '0')}:00`)
          },
          yaxis: {
            title: { text: "Average Pay", font: { size: 16 }, standoff: 10 },
            showgrid: true,
            zeroline: false,
            tickprefix: "$",
            tickformat: ".2f",
          },
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
