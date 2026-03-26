import * as client from "./client";
import { useCallback, useEffect, useState } from "react";
import { Row, Col } from "react-bootstrap";
import { Card, CardContent, Typography } from "@mui/material";
import PredictEarnings from "./PredictEarnings";
import EarningsChart from "./EarningsChart";
import TipsByNeighborhoodChart from "./TipsByNeighborhoodChart";
import BaseByAppsChart from "./BaseByAppsChart";
import EarningsDonutChart from "./EarningsDonutChart";
import TipsByAppChart from "./TipsByAppChart";
import type {
  StatsType,
  ShiftStatsType,
  ExpenseStatsType,
} from "../SignalRProvider";
import { useSignalR } from "../Contexts/SignalRContext";
import "../../index.css";
import EarningsByHourChart from "./EarningsByHourChart";

export type MonthlySpendingType = {
  type: string;
  avgExpense: number;
};

export default function Statistics() {
  // Loading
  const [loading, setLoading] = useState(true);

  // Page Control
  const [page, setPage] = useState("stats");

  // Remote server
  const {
    stats,
    shiftStats,
    expenseStats,
    setDeliveryStats,
    setShiftStats,
    setExpenseStats,
  } = useSignalR();

  // Fetch Statistics
  const fetchStatistics = useCallback(async () => {
    // Delivery Statistics
    // If stats does not exist, fetch from API and set, else pull its values
    if (!stats) {
      try {
        const deliveryStats = await client.fetchDeliveryStats();

        const initialDeliveryStats: StatsType = {
          avgPay: deliveryStats.avgPay,
          avgBase: deliveryStats.avgBase,
          avgTip: deliveryStats.avgTip,
          dollarPerMile: deliveryStats.dollarPerMile,
          tipPerMile: deliveryStats.tipPerMile,
          highestPayingRestaurant: deliveryStats.highestPayingRestaurant,
          restaurantWithMost: deliveryStats.restaurantWithMost,
          plotlyEarningsData: deliveryStats.plotlyEarningsData,
          plotlyNeighborhoodsData: deliveryStats.plotlyNeighborhoodsData,
          appsByBaseData: deliveryStats.appsByBaseData,
          tipsByAppData: deliveryStats.tipsByAppData,
          hourlyEarningsData: deliveryStats.hourlyEarningsData,
          donutChartData: deliveryStats.donutChartData,
        };

        setDeliveryStats(initialDeliveryStats);
      } catch {
        const initialDeliveryStats: StatsType = {
          avgPay: 0,
          avgBase: 0,
          avgTip: 0,
          dollarPerMile: 0,
          highestPayingRestaurant: { restaurant: "N/A", avgTotalPay: 0 },
          restaurantWithMost: { restaurantWithMost: "N/A", orderCount: 0 },
          tipPerMile: 0,
          plotlyEarningsData: null,
          plotlyNeighborhoodsData: null,
          appsByBaseData: null,
          tipsByAppData: null,
          hourlyEarningsData: null,
          donutChartData: null,
        };

        setDeliveryStats(initialDeliveryStats);
      }
    }

    // Shift Statistics
    // If shiftStats does not exist, fetch from API and set, else pull its values
    if (!shiftStats) {
      try {
        const shiftStats = await client.fetchShiftStats();

        setShiftStats(shiftStats);
      } catch {
        const initialShiftStats: ShiftStatsType = {
          averageShiftLength: 0,
          appWithMostShifts: "N/A",
          averageDeliveriesForShift: 0,
        };

        setShiftStats(initialShiftStats);
      }
    }

    if (!expenseStats) {
      try {
        const expenseStats = await client.fetchExpenseStats();

        setExpenseStats(expenseStats as ExpenseStatsType);
      } catch {
        const fallbackExpenseStats: ExpenseStatsType = {
          averageMonthlySpending: 0,
          averageSpendingByType: [],
        };

        setExpenseStats(fallbackExpenseStats);
      }
    }

    setLoading(false);
  }, [
    expenseStats,
    setDeliveryStats,
    setExpenseStats,
    setShiftStats,
    shiftStats,
    stats,
  ]);

  const getContent = () => {
    if (page === "stats") {
      return (
        <div id="stats" className="container-fluid">
          <Row>
            {/* Donut Chart Card */}
            <Col xs={12} sm={12} md={6} lg={5} style={{ display: "flex" }}>
              <Card
                sx={{
                  mb: 3,
                  borderRadius: 3,
                  boxShadow: 3,
                  position: "relative",
                  minHeight: 200,
                  flex: 1,
                  display: "flex",
                  flexDirection: "column",
                }}
              >
                <CardContent sx={{ p: 2, flex: 1 }}>
                  {stats && stats.donutChartData && (
                    <EarningsDonutChart data={stats.donutChartData} />
                  )}
                </CardContent>
              </Card>
            </Col>

            {/* Pay Statistics */}
            <Col xs={12} sm={12} md={6} lg={5} style={{ display: "flex" }}>
              <Card
                sx={{
                  mb: 3,
                  textAlign: "start",
                  borderRadius: 3,
                  boxShadow: 3,
                  position: "relative",
                  minHeight: 200,
                  flex: 1,
                  display: "flex",
                  flexDirection: "column",
                }}
              >
                <CardContent sx={{ p: 3, flex: 1 }}>
                  <Typography
                    variant="h6"
                    fontWeight="bold"
                    sx={{ color: "#111827", mb: 3 }}
                  >
                    Pay Statistics (Per Delivery)
                  </Typography>

                  <div
                    style={{
                      display: "grid",
                      gridTemplateColumns: "1fr 1fr",
                      gap: "1.5rem",
                    }}
                  >
                    <div>
                      <Typography
                        variant="body2"
                        color="text.secondary"
                        fontWeight="500"
                      >
                        Avg Total Pay
                      </Typography>
                      <Typography
                        variant="h5"
                        fontWeight="bold"
                        sx={{ color: "#111827" }}
                      >
                        {loading || !stats
                          ? "..."
                          : `$${stats.avgPay.toFixed(2)}`}
                      </Typography>
                    </div>
                    <div>
                      <Typography
                        variant="body2"
                        color="text.secondary"
                        fontWeight="500"
                      >
                        Avg Base Pay
                      </Typography>
                      <Typography
                        variant="h5"
                        fontWeight="bold"
                        sx={{ color: "#111827" }}
                      >
                        {loading || !stats
                          ? "..."
                          : `$${stats.avgBase.toFixed(2)}`}
                      </Typography>
                    </div>
                    <div>
                      <Typography
                        variant="body2"
                        color="text.secondary"
                        fontWeight="500"
                      >
                        Avg Tip Pay
                      </Typography>
                      <Typography
                        variant="h5"
                        fontWeight="bold"
                        sx={{ color: "#111827" }}
                      >
                        {loading || !stats
                          ? "..."
                          : `$${stats.avgTip.toFixed(2)}`}
                      </Typography>
                    </div>
                    <div>
                      <Typography
                        variant="body2"
                        color="text.secondary"
                        fontWeight="500"
                      >
                        Avg $/Mile
                      </Typography>
                      <Typography
                        variant="h5"
                        fontWeight="bold"
                        sx={{ color: "#111827" }}
                      >
                        {loading || !stats
                          ? "..."
                          : `$${stats.dollarPerMile.toFixed(2)}`}
                      </Typography>
                    </div>
                    <div>
                      <Typography
                        variant="body2"
                        color="text.secondary"
                        fontWeight="500"
                      >
                        Avg Tip/Mile
                      </Typography>
                      <Typography
                        variant="h5"
                        fontWeight="bold"
                        sx={{ color: "#111827" }}
                      >
                        {loading || !stats
                          ? "..."
                          : `$${stats.tipPerMile.toFixed(2)}`}
                      </Typography>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </Col>

            {/* Earnings Chart */}
            <Col xs={12} lg={10} style={{ display: "false", minWidth: 0 }}>
              <div style={{ minWidth: 0, width: "100%" }}>
                <Card
                  sx={{
                    mb: 3,
                    borderRadius: 3,
                    boxShadow: 3,
                    position: "relative",
                    minHeight: 200,
                    flex: 1,
                    display: "flex",
                    flexDirection: "column",
                    minWidth: 0,
                  }}
                >
                  <CardContent sx={{ p: 2, flex: 1 }}>
                    {stats && stats.plotlyEarningsData && (
                      <EarningsChart data={stats.plotlyEarningsData} />
                    )}
                  </CardContent>
                </Card>
              </div>
            </Col>

            {/* Earnings By Hour Chart */}
            <Col xs={12} lg={10} style={{ display: "flex", minWidth: 0 }}>
              <div style={{ minWidth: 0, width: "100%" }}>
                <Card
                  sx={{
                    mb: 3,
                    borderRadius: 3,
                    boxShadow: 3,
                    position: "relative",
                    minHeight: 200,
                    flex: 1,
                    display: "flex",
                    flexDirection: "column",
                    minWidth: 0,
                  }}
                >
                  <CardContent sx={{ p: 2, flex: 1 }}>
                    {stats && stats.hourlyEarningsData && (
                      <EarningsByHourChart data={stats.hourlyEarningsData} />
                    )}
                  </CardContent>
                </Card>
              </div>
            </Col>

            {/* Location Statistics */}
            <Col xs={12} sm={12} md={6} lg={5}>
              <Card
                sx={{
                  mb: 4,
                  textAlign: "start",
                  borderRadius: 3,
                  boxShadow: 3,
                  position: "relative",
                  transition: "0.3s",
                  minHeight: 300,
                }}
              >
                <CardContent sx={{ p: 3 }}>
                  <Typography
                    variant="h6"
                    fontWeight="bold"
                    sx={{ color: "#111827", mb: 3 }}
                  >
                    Location Statistics
                  </Typography>

                  <div
                    style={{
                      display: "flex",
                      flexDirection: "column",
                      gap: "1.5rem",
                    }}
                  >
                    <div>
                      <Typography
                        variant="body2"
                        color="text.secondary"
                        fontWeight="500"
                      >
                        Best Paying Restaurant
                      </Typography>
                      <Typography
                        variant="h6"
                        fontWeight="bold"
                        sx={{ color: "#111827" }}
                      >
                        {loading || !stats
                          ? "..."
                          : stats.highestPayingRestaurant.restaurant}
                      </Typography>
                      <Typography
                        variant="body2"
                        sx={{ color: "#10B981", fontWeight: "600" }}
                      >
                        {loading || !stats
                          ? ""
                          : `Avg Total: $${stats.highestPayingRestaurant.avgTotalPay.toFixed(2)}`}
                      </Typography>
                    </div>

                    <div>
                      <Typography
                        variant="body2"
                        color="text.secondary"
                        fontWeight="500"
                      >
                        Most Frequent Restaurant
                      </Typography>
                      <Typography
                        variant="h6"
                        fontWeight="bold"
                        sx={{ color: "#111827" }}
                      >
                        {loading || !stats
                          ? "..."
                          : stats.restaurantWithMost.restaurantWithMost}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        {loading || !stats
                          ? ""
                          : `${stats.restaurantWithMost.orderCount} total orders`}
                      </Typography>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </Col>

            {/* Shift Statistics */}
            <Col xs={12} sm={12} md={6} lg={5}>
              <Card
                sx={{
                  mb: 3,
                  textAlign: "start",
                  borderRadius: 3,
                  boxShadow: 3,
                  position: "relative",
                  transition: "0.3s",
                  minHeight: 300,
                }}
              >
                <CardContent sx={{ p: 3 }}>
                  <Typography
                    variant="h6"
                    fontWeight="bold"
                    sx={{ color: "#111827", mb: 3 }}
                  >
                    Shift Statistics
                  </Typography>

                  <div
                    style={{
                      display: "grid",
                      gridTemplateColumns: "1fr 1fr",
                      gap: "1.5rem",
                    }}
                  >
                    <div>
                      <Typography
                        variant="body2"
                        color="text.secondary"
                        fontWeight="500"
                      >
                        Avg Shift Length
                      </Typography>
                      <Typography
                        variant="h5"
                        fontWeight="bold"
                        sx={{ color: "#111827" }}
                      >
                        {loading || !shiftStats
                          ? "..."
                          : `${shiftStats.averageShiftLength?.toFixed(0)} min`}
                      </Typography>
                    </div>
                    <div>
                      <Typography
                        variant="body2"
                        color="text.secondary"
                        fontWeight="500"
                      >
                        Avg Deliveries/Shift
                      </Typography>
                      <Typography
                        variant="h5"
                        fontWeight="bold"
                        sx={{ color: "#111827" }}
                      >
                        {loading || !shiftStats
                          ? "..."
                          : Math.floor(shiftStats.averageDeliveriesForShift)}
                      </Typography>
                    </div>
                    <div style={{ gridColumn: "span 2" }}>
                      <Typography
                        variant="body2"
                        color="text.secondary"
                        fontWeight="500"
                      >
                        Most Used App
                      </Typography>
                      <Typography
                        variant="h5"
                        fontWeight="bold"
                        sx={{ color: "#111827" }}
                      >
                        {loading || !shiftStats
                          ? "..."
                          : shiftStats.appWithMostShifts}
                      </Typography>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </Col>

            {/* Tips By Neighborhood Chart */}
            <Col
              xs={12}
              sm={12}
              md={12}
              lg={10}
              style={{ display: "flex", minWidth: 0 }}
            >
              <div style={{ minWidth: 0, width: "100%" }}>
                <Card
                  sx={{
                    mb: 3,
                    borderRadius: 3,
                    boxShadow: 3,
                    position: "relative",
                    minHeight: 200,
                    flex: 1,
                    display: "flex",
                    flexDirection: "column",
                    minWidth: 0,
                  }}
                >
                  <CardContent sx={{ p: 2, flex: 1 }}>
                    {stats && stats.plotlyNeighborhoodsData && (
                      <TipsByNeighborhoodChart
                        data={stats.plotlyNeighborhoodsData}
                      />
                    )}
                  </CardContent>
                </Card>
              </div>
            </Col>

            {/* Apps By Base Chart */}
            <Col xs={12} sm={12} md={6} lg={5}>
              <Card
                sx={{
                  mb: 3,
                  textAlign: "start",
                  borderRadius: 3,
                  boxShadow: 3,
                  position: "relative",
                  minHeight: 450,
                  display: "flex",
                  flexDirection: "column",
                  minWidth: 0,
                }}
              >
                <CardContent sx={{ p: 2, flex: 1 }}>
                  {stats && stats.appsByBaseData && (
                    <BaseByAppsChart data={stats.appsByBaseData} />
                  )}
                </CardContent>
              </Card>
            </Col>

            {/* Tips By App Chart */}
            <Col xs={12} sm={12} md={6} lg={5}>
              <Card
                sx={{
                  mb: 3,
                  textAlign: "start",
                  borderRadius: 3,
                  boxShadow: 3,
                  position: "relative",
                  minHeight: 450,
                  display: "flex",
                  flexDirection: "column",
                  minWidth: 0,
                }}
              >
                <CardContent sx={{ p: 2, flex: 1 }}>
                  {stats && stats.tipsByAppData && (
                    <TipsByAppChart data={stats.tipsByAppData} />
                  )}
                </CardContent>
              </Card>
            </Col>

            {/* Expense Statistics */}
            <Col xs={12} sm={12} md={6} lg={5}>
              <Card
                sx={{
                  mb: 3,
                  textAlign: "start",
                  borderRadius: 3,
                  boxShadow: 3,
                  position: "relative",
                  transition: "0.3s",
                  minHeight: 200,
                }}
              >
                <CardContent sx={{ p: 3 }}>
                  <Typography
                    variant="h6"
                    fontWeight="bold"
                    sx={{ color: "#111827", mb: 3 }}
                  >
                    Expense Statistics
                  </Typography>

                  <div
                    style={{
                      marginBottom: "1.5rem",
                      paddingBottom: "1.5rem",
                      borderBottom: "1px solid #E5E7EB",
                    }}
                  >
                    <Typography
                      variant="body2"
                      color="text.secondary"
                      fontWeight="500"
                    >
                      Avg Monthly Spending
                    </Typography>
                    <Typography
                      variant="h4"
                      fontWeight="bold"
                      sx={{ color: "#F43F5E" }}
                    >
                      {" "}
                      {/* Red tint for expenses */}
                      {loading || !expenseStats
                        ? "..."
                        : `$${expenseStats.averageMonthlySpending.toFixed(2)}`}
                    </Typography>
                  </div>

                  <Typography
                    variant="body2"
                    color="text.secondary"
                    fontWeight="600"
                    sx={{
                      mb: 2,
                      textTransform: "uppercase",
                      fontSize: "0.75rem",
                      letterSpacing: "0.05em",
                    }}
                  >
                    Spending Breakdown
                  </Typography>

                  <div
                    style={{
                      display: "flex",
                      flexDirection: "column",
                      gap: "0.75rem",
                    }}
                  >
                    {loading || !expenseStats
                      ? "Loading..."
                      : expenseStats.averageSpendingByType.map(
                          (average, idx) => (
                            <div
                              key={idx}
                              style={{
                                display: "flex",
                                justifyContent: "space-between",
                                alignItems: "center",
                              }}
                            >
                              <Typography
                                variant="body2"
                                sx={{ color: "#4B5563", fontWeight: 500 }}
                              >
                                {average.type}
                              </Typography>
                              <Typography
                                variant="body2"
                                fontWeight="bold"
                                sx={{ color: "#111827" }}
                              >
                                ${average.avgExpense.toFixed(2)}
                              </Typography>
                            </div>
                          ),
                        )}
                  </div>
                </CardContent>
              </Card>
            </Col>
          </Row>
        </div>
      );
    } else if (page === "predict-earnings") {
      return (
        <div id="predict-earnings">
          <PredictEarnings />
        </div>
      );
    }
  };

  useEffect(() => {
    fetchStatistics();
  }, [fetchStatistics]);

  return (
    <div id="da-statistics">
      <h1 className="ms-2 mb-3 page-header">Your Statistics</h1>
      <Col sm={6}>
        <select
          onChange={(e) => setPage(e.target.value)}
          className="form-control ms-2 mb-4"
        >
          <option value="stats">Overall Statistics</option>
          <option value="predict-earnings">Predict Earnings</option>
        </select>
      </Col>

      {getContent()}
    </div>
  );
}
