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
import type { StatsType, ShiftStatsType, ExpenseStatsType } from "../SignalRProvider";
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
    const { stats, shiftStats, expenseStats, 
        setDeliveryStats, setShiftStats, setExpenseStats } = useSignalR();

    // Fetch Statistics
    const fetchStatistics = useCallback(async () => {
        // Delivery Statistics
        // If stats does not exist, fetch from API and set, else pull its values
        if (!stats) {
            try {
                const avgPay = await client.findAvgDeliveryPay();
                const avgBase = await client.findAvgBasePay();
                const avgTip = await client.findAvgTipPay();
                const dollarPerMile = await client.findDollarPerMile();
                const bestRestaurant = await client.findHighestPayingRestaurant();
                const tipPerMile = await client.findAverageTipPerMile();
                const restaurantWithMostOrders = await client.findRestaurantWithMostDeliveries();

                const userPlotlyEarningsData = await client.findPlotlyEarningsData();
                const userTipNeighborhoodsData = await client.findPlotlyTipNeighborhoodData();
                const userBaseByAppData = await client.findPlotlyBaseByApp();
                const userTipsByAppData = await client.findTipsByAppData();
                const userHourlyEarningsData = await client.findHourlyPayData();
                const userEarningsDonutData = await client.findDonutChartData();

                const initialDeliveryStats: StatsType = {
                    avgPay: avgPay,
                    avgBase: avgBase,
                    avgTip: avgTip,
                    dollarPerMile: dollarPerMile,
                    highestPayingRestaurant: bestRestaurant,
                    restaurantWithMost: restaurantWithMostOrders,
                    tipPerMile: tipPerMile,
                    plotlyEarningsData: userPlotlyEarningsData,
                    plotlyNeighborhoodsData: userTipNeighborhoodsData,
                    appsByBaseData: userBaseByAppData,
                    tipsByAppData: userTipsByAppData,
                    hourlyEarningsData: userHourlyEarningsData,
                    donutChartData: userEarningsDonutData
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
                    donutChartData: null
                };

                setDeliveryStats(initialDeliveryStats);
            }
        }

        // Shift Statistics
        // If shiftStats does not exist, fetch from API and set, else pull its values
        if (!shiftStats) {
            try {
                const averageUserShiftLength = await client.findAverageShiftLength();
                const appWithMostUserShifts = await client.findAppWithMostShifts();
                const avgOrdersPerShift = await client.findAverageDeliveriesPerShift();

                const initialShiftStats: ShiftStatsType = {
                    averageShiftLength: averageUserShiftLength,
                    appWithMostShifts: appWithMostUserShifts,
                    averageDeliveriesForShift: avgOrdersPerShift
                };

                setShiftStats(initialShiftStats);
            } catch {
                const initialShiftStats: ShiftStatsType = {
                    averageShiftLength: 0,
                    appWithMostShifts: "N/A",
                    averageDeliveriesForShift: 0
                };

                setShiftStats(initialShiftStats);
            }
        }

        if (!expenseStats) {
            try {
                const averageMonthlyExpenses = await client.findAverageMonthlySpending();
                const avgMonthlySpendingByType = await client.findMonthlySpendingByType();

                const initialExpenseStats: ExpenseStatsType = {
                    averageMonthlySpending: averageMonthlyExpenses,
                    averageSpendingByType: avgMonthlySpendingByType
                };

                setExpenseStats(initialExpenseStats);
            } catch {
                const initialExpenseStats: ExpenseStatsType = {
                    averageMonthlySpending: 0,
                    averageSpendingByType: []
                };

                setExpenseStats(initialExpenseStats);
            }
        }

        setLoading(false);
    }, [expenseStats, setDeliveryStats, setExpenseStats, setShiftStats, shiftStats, stats]);

    const getContent = () => {
        if (page === "stats") {
            return (
                <div id="stats" className="container-fluid">
                    <Row>
                        <Col xs={12} sm={12} md={6} lg={5} style={{ display: "flex" }}>
                            <Card sx={{
                                mb: 3,
                                borderRadius: 3,
                                boxShadow: 3,
                                position: "relative",
                                minHeight: 200,
                                flex: 1,
                                display: "flex",
                                flexDirection: "column"
                            }}>
                                <CardContent sx={{ p: 2, flex: 1 }}>
                                    {stats && stats.donutChartData &&
                                        <EarningsDonutChart data={stats.donutChartData} />
                                    }
                                </CardContent>
                            </Card>
                        </Col>
                        {/*Pay Statistics*/}
                        <Col xs={12} sm={12} md={6} lg={5} style={{ display: "flex" }}>
                            <Card sx={{
                                mb: 3,
                                textAlign: "start",
                                borderRadius: 3,
                                boxShadow: 3,
                                position: "relative",
                                minHeight: 200,
                                flex: 1,
                                display: "flex",
                                flexDirection: "column"
                            }}>
                                <CardContent sx={{ p: 2, flex: 1 }}>
                                    <Typography variant="h6" fontWeight="bold">
                                        Pay Statistics (Per Delivery)
                                    </Typography>
                                    <Typography variant="body1" component="div" sx={{ mt: 1 }}>
                                        {loading ?
                                            <div>
                                                <strong>Average total pay:</strong> Loading... <br />
                                                <strong>Average base pay:</strong> Loading... <br />
                                                <strong>Average tip pay:</strong> Loading...<br /> <br />
                                                <strong>Average dollar/mile:</strong> Loading...<br />
                                                <strong>Average tip/mile:</strong> Loading...<br />
                                            </div>
                                            :
                                            <div>
                                                <strong>Average total pay:</strong> ${stats && stats.avgPay.toFixed(2)} <br />
                                                <strong>Average base pay:</strong> ${stats && stats.avgBase.toFixed(2)} <br />
                                                <strong>Average tip pay:</strong> ${stats && stats.avgTip.toFixed(2)} <br /> <br />
                                                <strong>Average dollar/mile:</strong> ${stats && stats.dollarPerMile.toFixed(2)} <br />
                                                <strong>Average tip/mile</strong> ${stats && stats.tipPerMile.toFixed(2)} <br />
                                            </div>
                                        }
                                    </Typography>
                                </CardContent>
                            </Card>
                        </Col>

                        <Col xs={12} lg={10} style={{ display: "false", minWidth: 0 }}>
                            <div style={{ minWidth: 0, width: "100%" }}>
                                <Card sx={{
                                    mb: 3,
                                    borderRadius: 3,
                                    boxShadow: 3,
                                    position: "relative",
                                    minHeight: 200,
                                    flex: 1,
                                    display: "flex",
                                    flexDirection: "column",
                                    minWidth: 0
                                }}>
                                    <CardContent sx={{ p: 2, flex: 1 }}>
                                        {stats && stats.plotlyEarningsData &&
                                            <EarningsChart data={stats.plotlyEarningsData} />
                                        }
                                    </CardContent>
                                </Card>
                            </div>
                        </Col>

                        <Col xs={12} lg={10} style={{ display: "flex", minWidth: 0 }}>
                            <div style={{ minWidth: 0, width: "100%" }}>
                                <Card sx={{
                                    mb: 3,
                                    borderRadius: 3,
                                    boxShadow: 3,
                                    position: "relative",
                                    minHeight: 200,
                                    flex: 1,
                                    display: "flex",
                                    flexDirection: "column",
                                    minWidth: 0
                                }}>
                                    <CardContent sx={{ p: 2, flex: 1 }}>
                                        {stats && stats.hourlyEarningsData &&
                                            <EarningsByHourChart data={stats.hourlyEarningsData} />
                                        }
                                    </CardContent>
                                </Card>
                            </div>
                        </Col>

                        {/*Location Statistics*/}
                        <Col xs={12} sm={12} md={6} lg={5}>
                            <Card sx={{
                                mb: 4,
                                textAlign: "start",
                                borderRadius: 3,
                                boxShadow: 3,
                                position: "relative",
                                transition: "0.3s",
                                minHeight: 200,
                            }}>
                                <CardContent sx={{ p: 2 }}>
                                    <Typography variant="h6" fontWeight="bold">Location Statistics</Typography>
                                    <Typography variant="body1" component="div" sx={{ mt: 1 }}>
                                        {loading ?
                                            <div>
                                                <strong>Best paying restaurant:</strong> Loading... <br />
                                                <span style={{ marginLeft: "1rem" }}>
                                                    <strong>- Average Total:</strong> Loading... <br />
                                                </span>
                                                <strong>Restaurant with most orders:</strong> Loading... <br />
                                                <span style={{ marginLeft: "1rem" }}>
                                                    <strong>- Number of Orders:</strong> Loading...
                                                </span>
                                            </div>
                                            :
                                            <div>
                                                <strong>Best paying restaurant:</strong> {stats && stats.highestPayingRestaurant.restaurant} <br />
                                                <span style={{ marginLeft: "1rem" }}>
                                                    <strong>- Average Total:</strong> ${stats && stats.highestPayingRestaurant.avgTotalPay.toFixed(2)} <br />
                                                </span>
                                                <strong>Restaurant with most orders:</strong> {stats && stats.restaurantWithMost.restaurantWithMost} <br />
                                                <span style={{ marginLeft: "1rem" }}>
                                                    <strong>- Number of Orders:</strong> {stats && stats.restaurantWithMost.orderCount}
                                                </span>
                                            </div>
                                        }
                                    </Typography>
                                </CardContent>
                            </Card>
                        </Col>

                        <Col xs={12} sm={12} md={6} lg={5}>
                            <Card sx={{
                                mb: 3,
                                textAlign: "start",
                                borderRadius: 3,
                                boxShadow: 3,
                                position: "relative",
                                transition: "0.3s",
                                minHeight: 200
                            }}>
                                <CardContent sx={{ p: 2 }}>
                                    <Typography variant="h6" fontWeight="bold">Shift Statistics</Typography>
                                    <Typography variant="body1" component="div" sx={{ mt: 1 }}>
                                        <strong>Average shift length:</strong> {shiftStats && shiftStats.averageShiftLength?.toFixed(0)} minutes <br />
                                        <strong>Average number of deliveries per shift:</strong> {shiftStats && Math.floor(shiftStats.averageDeliveriesForShift)} <br />
                                        <strong>App with most shifts:</strong> {shiftStats && shiftStats.appWithMostShifts} <br />
                                    </Typography>
                                </CardContent>
                            </Card>
                        </Col>

                        {/*Tips By Neighborhood Chart*/}
                        <Col xs={12} sm={12} md={12} lg={10} style={{ display: "flex", minWidth: 0 }}>
                            <div style={{ minWidth: 0, width: "100%" }}>
                                <Card sx={{
                                    mb: 3,
                                    borderRadius: 3,
                                    boxShadow: 3,
                                    position: "relative",
                                    minHeight: 200,
                                    flex: 1,
                                    display: "flex",
                                    flexDirection: "column",
                                    minWidth: 0
                                }}>
                                    <CardContent sx={{ p: 2, flex: 1 }}>
                                        {stats && stats.plotlyNeighborhoodsData &&
                                            <TipsByNeighborhoodChart data={stats.plotlyNeighborhoodsData} />
                                        }
                                    </CardContent>
                                </Card>
                            </div>
                        </Col>

                        {/*App Statistics*/}
                        <Col xs={12} sm={12} md={6} lg={5}>
                            <Card sx={{
                                mb: 3,
                                textAlign: "start",
                                borderRadius: 3,
                                boxShadow: 3,
                                position: "relative",
                                minHeight: 450,
                                display: "flex",
                                flexDirection: "column",
                                minWidth: 0
                            }}>
                                <CardContent sx={{ p: 2, flex: 1 }}>
                                    {stats && stats.appsByBaseData &&
                                        <BaseByAppsChart data={stats.appsByBaseData} />
                                    }
                                </CardContent>
                            </Card>
                        </Col>

                        <Col xs={12} sm={12} md={6} lg={5}>
                            <Card sx={{
                                mb: 3,
                                textAlign: "start",
                                borderRadius: 3,
                                boxShadow: 3,
                                position: "relative",
                                minHeight: 450,
                                display: "flex",
                                flexDirection: "column",
                                minWidth: 0
                            }}>
                                <CardContent sx={{ p: 2, flex: 1 }}>
                                    {stats && stats.tipsByAppData &&
                                        <TipsByAppChart data={stats.tipsByAppData} />
                                    }
                                </CardContent>
                            </Card>
                        </Col>

                        <Col xs={12} sm={12} md={6} lg={5}>
                            <Card sx={{
                                mb: 3,
                                textAlign: "start",
                                borderRadius: 3,
                                boxShadow: 3,
                                position: "relative",
                                transition: "0.3s",
                                minHeight: 200
                            }}>
                                <CardContent sx={{ p: 2 }}>
                                    <Typography variant="h6" fontWeight="bold">Expense Statistics</Typography>
                                    <Typography variant="body1" component="div" sx={{ mt: 1 }}>
                                        <strong>Average monthly spending:</strong> ${expenseStats?.averageMonthlySpending.toFixed(2)} <br />
                                        <strong>Monthly spending by type:</strong>
                                        <div style={{ marginLeft: "1rem" }}>
                                            {expenseStats?.averageSpendingByType.map((average, idx) => (
                                                <div key={idx}>
                                                    <strong>- {average.type}:</strong> ${average.avgExpense.toFixed(2)}
                                                </div>
                                            ))}
                                        </div>
                                    </Typography>
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
    }

    useEffect(() => {
        fetchStatistics()
    }, [fetchStatistics])

    return (
        <div id="da-statistics">
            <h1 className="ms-2 mb-3">Your Statistics</h1>
            <Col sm={6}>
                <select onChange={(e) => setPage(e.target.value)} className="form-control ms-2 mb-4">
                    <option value="stats">Overall Statistics</option>
                    <option value="predict-earnings">Predict Earnings</option>
                </select>
            </Col>

            {getContent()}
        </div>
    );
}