import { useEffect, useRef, useState } from "react";
import type { ReactNode } from "react";
import * as signalR from "@microsoft/signalr";
import type { MonthlySpendingType } from "./Statistics";
import { SignalRContext } from "./SignalRContext";

type SignalRProps = {
    children: ReactNode,
    token: string | null
};

export type StatsType = {
    avgPay: number,
    avgBase: number,
    avgTip: number,
    dollarPerMile: number,
    highestPayingRestaurant: {restaurant: string, avgTotalPay: number},
    restaurantWithMost: {restaurantWithMost: string, orderCount: number},
    tipPerMile: number,
    plotlyEarningsData: {dates: string[], earnings: number[]} | null,
    plotlyNeighborhoodsData: {neighborhoods: string[], tipPays: number[]} | null,
    appsByBaseData: {apps: string[], basePays: number[]} | null,
    tipsByAppData: {tipApps: string[], appTipPays: number[]} | null,
    hourlyEarningsData: {hours: string[], earnings: number[]} | null,
    donutChartData: {totalPay: number, totalBasePay: number, totalTipPay: number} | null
};

export type ShiftStatsType = {
    averageShiftLength: number,
    appWithMostShifts: string,
    averageDeliveriesForShift: number
};

export type ExpenseStatsType = {
    averageMonthlySpending: number,
    averageSpendingByType: MonthlySpendingType[]
};

export const SignalRProvider = ({ children, token }: SignalRProps) => {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null)
    const [stats, setStats] = useState<StatsType | null>(null);
    const [shiftStats, setShiftStats] = useState<ShiftStatsType | null>(null);
    const [expenseStats, setExpenseStats] = useState<ExpenseStatsType | null>(null);
    const REMOTE_SERVER = import.meta.env.VITE_REMOTE_SERVER;

    useEffect(() => {
        if (!token) {
            return;
        }

        const conn = new signalR.HubConnectionBuilder()
            .withUrl(`${REMOTE_SERVER}/hubs/statistics`, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        conn.start()
            .then(() => console.log("SignalR connected"))
            .catch((err) => console.error("SignalR connection failed:", err));

        conn.on("StatisticsUpdated", (updatedStats) => {
            setStats(updatedStats);
        });

        conn.on("ShiftStatisticsUpdated", (updatedShiftStats) => {
            setShiftStats(updatedShiftStats);
        });

        conn.on("ExpenseStatisticsUpdated", (updatedExpenseStats) => {
            setExpenseStats(updatedExpenseStats);
        });

        setConnection(conn);

        return () => {
            conn.stop();
          };
    }, [REMOTE_SERVER, token]);

    useEffect(() => {
        if (!token && connection) {
            connection.stop();
            setConnection(null);
            setStats(null);
            setShiftStats(null);
            setExpenseStats(null);
        }
    }, [connection, token])

    const isInitialized = useRef(false);
    useEffect(() => {
        if (isInitialized.current) {
            clearStats();
        } else {
            isInitialized.current = true;
        }
    }, [token]);

    const clearStats = () => {
        setStats(null);
        setShiftStats(null);
        setExpenseStats(null);
    };

    const setDeliveryStats = (stats: StatsType) => {
        setStats(stats);
    }

    const setShiftsStats = (stats: ShiftStatsType) => {
        setShiftStats(stats);
    }

    const setExpensesStats = (stats: ExpenseStatsType) => {
        setExpenseStats(stats);
    }

    return (
        <SignalRContext.Provider value={{connection, stats, shiftStats, expenseStats,
            setDeliveryStats, setShiftsStats, setExpensesStats, clearStats}}>
            {children}
        </SignalRContext.Provider>
    );
};