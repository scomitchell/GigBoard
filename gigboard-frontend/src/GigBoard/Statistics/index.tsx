import * as client from "./client";
import { useEffect, useState } from "react";
import { Row, Col } from "react-bootstrap";
import { Card, CardContent, Typography } from "@mui/material";
import PredictEarnings from "./PredictEarnings";
import EarningsChart from "./EarningsChart";
import TipsByNeighborhoodChart from "./TipsByNeighborhoodChart";
import BaseByAppsChart from "./BaseByAppsChart";
import HourlyEarningsChart from "./HourlyEarningsChart";
import EarningsDonutChart from "./EarningsDonutChart";
import TipsByAppChart from "./TipsByAppChart";
import type { HourlyEarningsProps } from "./HourlyEarningsChart";
import type { EarningsChartProps } from "./EarningsChart";
import type { TipNeighborhoodsProps } from "./TipsByNeighborhoodChart";
import type { BaseByAppProps } from "./BaseByAppsChart";
import type { EarningsDonutProps } from "./EarningsDonutChart";
import type { TipsByAppProps } from "./TipsByAppChart";
import type { StatsType, ShiftStatsType } from "../SignalRContext";
import { useSignalR } from "../SignalRContext";
import "../../index.css";

type MonthlySpendingType = {
    type: string;
    avgExpense: number;
};

export default function Statistics() {
    // Pay statistics
    const [averagePay, setAveragePay] = useState(0);
    const [averageBase, setAverageBase] = useState(0);
    const [averageTip, setAverageTip] = useState(0);
    const [avgDollarPerMile, setAvgDollarPerMile] = useState(0);
    const [avgTipPerMile, setAvgTipPerMile] = useState(0);

    // Location Statistics
    const [restaurant, setRestaurant] = useState({ restaurant: "", avgTotalPay: 0 });
    const [restaurantWithMost, setRestaurantWithMost] = useState({ restaurantWithMost: "", orderCount: 0 });

    // Expense statistics
    const [monthlySpending, setMonthlySpending] = useState(0);
    const [monthlySpendingByType, setMonthlySpendingByType] = useState<MonthlySpendingType[]>([]);

    // Shift statistics
    const [averageShiftLength, setAverageShiftLength] = useState<number | null>(null);
    const [appWithMostShifts, setAppWithMostShifts] = useState<string | null>(null);
    const [avgDeliveriesPerShift, setAvgDeliveriesPerShift] = useState(0);

    // Charts
    const [plotlyEarningsData, setPlotlyEarningsData] = useState<EarningsChartProps["data"] | null>(null);
    const [plotlyTipNeighborhoodsData, setPlotlyTipNeighborhoodsData] = useState<TipNeighborhoodsProps["data"] | null>(null);
    const [plotlyBaseByAppData, setPlotlyBaseByAppData] = useState<BaseByAppProps["data"] | null>(null);
    const [hourlyEarningsData, setHourlyEarningsData] = useState<HourlyEarningsProps["data"] | null>(null);
    const [donutChartData, setDonutChartData] = useState<EarningsDonutProps["data"] | null>(null);
    const [tipsByAppData, setTipsByAppData] = useState<TipsByAppProps["data"] | null>(null);

    // Loading
    const [loading, setLoading] = useState(true);

    // Page Control
    const [page, setPage] = useState("stats");

    // Remote server
    const { stats, shiftStats, setDeliveryStats, setShiftsStats } = useSignalR();

    // Fetch Statistics
    const fetchStatistics = async () => {
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
                    tipsByAppData: userTipsByAppData
                };

                setDeliveryStats(initialDeliveryStats);

                setAveragePay(avgPay ?? 0);
                setAverageBase(avgBase ?? 0);
                setAverageTip(avgTip ?? 0);
                setAvgDollarPerMile(dollarPerMile ?? 0);
                setAvgTipPerMile(tipPerMile ?? 0);
                setRestaurant(bestRestaurant ?? { restaurant: "N/A", avgTotalPay: 0 });
                setRestaurantWithMost(restaurantWithMostOrders ?? { restaurantWithMost: "N/A", orderCount: 0 });
                setPlotlyEarningsData(userPlotlyEarningsData);
                setPlotlyTipNeighborhoodsData(userTipNeighborhoodsData);
                setPlotlyBaseByAppData(userBaseByAppData);
                setTipsByAppData(userTipsByAppData);
            } catch {
                setAveragePay(0)
                setAverageBase(0)
                setAverageTip(0);
                setAvgDollarPerMile(0);
                setAvgTipPerMile(0);
                setRestaurant({ restaurant: "N/A", avgTotalPay: 0 });
                setRestaurantWithMost({ restaurantWithMost: "N/A", orderCount: 0 });
                setPlotlyEarningsData(null);
                setPlotlyTipNeighborhoodsData(null);
                setPlotlyBaseByAppData(null);
                setTipsByAppData(null);
            }
        } else {
            setAveragePay(stats.avgPay);
            setAverageBase(stats.avgBase);
            setAverageTip(stats.avgTip);
            setAvgDollarPerMile(stats.dollarPerMile);
            setRestaurant(stats.highestPayingRestaurant);
            setRestaurantWithMost(stats.restaurantWithMost);
            setAvgTipPerMile(stats.tipPerMile);
            setPlotlyEarningsData(stats.plotlyEarningsData);
            setPlotlyTipNeighborhoodsData(stats.plotlyNeighborhoodsData);
            setPlotlyBaseByAppData(stats.appsByBaseData);
            setTipsByAppData(stats.tipsByAppData);
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

                setShiftsStats(initialShiftStats);

                setAverageShiftLength(averageUserShiftLength);
                setAppWithMostShifts(appWithMostUserShifts);
                setAvgDeliveriesPerShift(avgOrdersPerShift);
            } catch {
                setAverageShiftLength(0);
                setAppWithMostShifts("N/A");
                setAvgDeliveriesPerShift(0);
            }
        } else {
            setAverageShiftLength(shiftStats.averageShiftLength);
            setAppWithMostShifts(shiftStats.appWithMostShifts);
            setAvgDeliveriesPerShift(shiftStats.averageDeliveriesForShift);
        }

        try {
            const averageMonthlyExpenses = await client.findAverageMonthlySpending();
            const avgMonthlySpendingByType = await client.findMonthlySpendingByType();

            setMonthlySpending(averageMonthlyExpenses ?? 0);
            setMonthlySpendingByType(avgMonthlySpendingByType ?? []);
        } catch {
            setMonthlySpending(0);
            setMonthlySpendingByType([]);
        }

        try {
            const userHourlyEarningsData = await client.findHourlyPayData();

            setHourlyEarningsData(userHourlyEarningsData);
        } catch {
            setHourlyEarningsData(null);
        }

        try {
            const userEarningsDonutData = await client.findDonutChartData();

            setDonutChartData(userEarningsDonutData);
        } catch {
            setDonutChartData(null);
        }

        setLoading(false);
    }

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
                                    {donutChartData &&
                                        <EarningsDonutChart data={donutChartData} />
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
                                                <strong>Average total pay:</strong> ${averagePay.toFixed(2)} <br />
                                                <strong>Average base pay:</strong> ${averageBase.toFixed(2)} <br />
                                                <strong>Average tip pay:</strong> ${averageTip.toFixed(2)} <br /> <br />
                                                <strong>Average dollar/mile:</strong> ${avgDollarPerMile.toFixed(2)} <br />
                                                <strong>Average tip/mile</strong> ${avgTipPerMile.toFixed(2)} <br />
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
                                        {plotlyEarningsData &&
                                            <EarningsChart data={plotlyEarningsData} />
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
                                        {hourlyEarningsData &&
                                            <HourlyEarningsChart data={hourlyEarningsData} />
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
                                                <strong>Best paying restaurant:</strong> {restaurant.restaurant} <br />
                                                <span style={{ marginLeft: "1rem" }}>
                                                    <strong>- Average Total:</strong> ${restaurant.avgTotalPay.toFixed(2)} <br />
                                                </span>
                                                <strong>Restaurant with most orders:</strong> {restaurantWithMost.restaurantWithMost} <br />
                                                <span style={{ marginLeft: "1rem" }}>
                                                    <strong>- Number of Orders:</strong> {restaurantWithMost.orderCount}
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
                                        <strong>Average shift length:</strong> {averageShiftLength?.toFixed(0)} minutes <br />
                                        <strong>Average number of deliveries per shift:</strong> {Math.floor(avgDeliveriesPerShift)} <br />
                                        <strong>App with most shifts:</strong> {appWithMostShifts} <br />
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
                                        {plotlyTipNeighborhoodsData &&
                                            <TipsByNeighborhoodChart data={plotlyTipNeighborhoodsData} />
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
                                    {plotlyBaseByAppData &&
                                        <BaseByAppsChart data={plotlyBaseByAppData} />
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
                                    {tipsByAppData &&
                                        <TipsByAppChart data={tipsByAppData} />
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
                                        <strong>Average monthly spending:</strong> ${monthlySpending.toFixed(2)} <br />
                                        <strong>Monthly spending by type:</strong>
                                        <div style={{ marginLeft: "1rem" }}>
                                            {monthlySpendingByType.map((average, idx) => (
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
    }, [])

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