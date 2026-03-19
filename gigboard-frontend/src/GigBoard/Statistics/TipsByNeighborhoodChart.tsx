import Plot from "react-plotly.js";

export type TipNeighborhoodsProps = {
    data: {
        neighborhoods: string[],
        tipPays: number[]
    };
};

export default function TipsByNeighborhoodChart({data}: TipNeighborhoodsProps) {
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
              x: data.neighborhoods,
              y: data.tipPays,
              type: "bar",
              marker: { color: "#6366F1" },
              name: "Earnings",
              hoverTemplate: `$%{y:.2f}<br>%{x}`,
            },
          ]}
          layout={{
            autosize: true,
            title: {
              text: "Average Tip By Neighborhood",
              font: { size: 20, weight: "bold" },
            },
            xaxis: {
              title: { text: "Neighborhood", font: { size: 16 }, standoff: 10 },
              tickangle: -30,
              showgrid: true,
              zeroline: false,
            },
            yaxis: {
              title: { text: "Average Tip", font: { size: 16 }, standoff: 10 },
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