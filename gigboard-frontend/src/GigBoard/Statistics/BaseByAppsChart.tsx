import Plot from "react-plotly.js";

export type BaseByAppProps = {
    data: {
        apps: string[],
        basePays: number[]
    };
};

export default function BaseByAppsChart({data}: BaseByAppProps) {
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
              text: "Average Base Pay by App",
              font: { size: 20, weight: "bold" },
            },
            xaxis: {
              title: { text: "App", font: { size: 16 }, standoff: 20 },
              tickangle: -30,
              showgrid: true,
              zeroline: false,
            },
            yaxis: {
              title: {
                text: "Average Base Pay ($)",
                font: { size: 16 },
                standoff: 10,
              },
              showgrid: true,
              zeroline: false,
              tickprefix: "$",
              tickformat: ".2f",
            },
            plot_bgcolor: "white",
            paper_bgcolor: "white",
            autosize: true,
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