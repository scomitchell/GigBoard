import { createContext, useContext, useEffect, useState } from "react";
import type { ReactNode } from "react";
import * as signalR from "@microsoft/signalr";

type SignalRProps = {
    children: ReactNode,
    token: string | null
};

type SignalRContextType = {
    connection: signalR.HubConnection | null,
    stats: StatsType | null,
    shiftStats: ShiftStatsType | null,
    clearStats: () => void,
    setDeliveryStats: (stats: StatsType) => void,
    setShiftsStats: (stats: ShiftStatsType) => void
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
    tipsByAppData: {tipApps: string[], appTipPays: number[]} | null
};

export type ShiftStatsType = {
    averageShiftLength: number,
    appWithMostShifts: string,
    averageDeliveriesForShift: number
};

const SignalRContext = createContext<SignalRContextType | null>(null);

export const SignalRProvider = ({ children, token }: SignalRProps) => {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null)
    const [stats, setStats] = useState<StatsType | null>(null);
    const [shiftStats, setShiftStats] = useState<ShiftStatsType | null>(null);
    const [initialized, setInitialized] = useState(false);
    const REMOTE_SERVER = import.meta.env.VITE_REMOTE_SERVER;

    useEffect(() => {
        if (!token) {
            if (connection) {
                connection.stop();
                console.log("Connection Stopped");
                setConnection(null);
                setStats(null);
                setShiftStats(null);
            }
            return;
        }

        const conn = new signalR.HubConnectionBuilder()
            .withUrl(`${REMOTE_SERVER}/hubs/statistics`, {
                accessTokenFactory: () => localStorage.getItem("token") ?? ""
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

        setConnection(conn);

        return () => {
            conn.stop();
          };
    }, [token]);

    useEffect(() => {
        if (initialized) {
            setStats(null);
            setShiftStats(null);
        } else {
            setInitialized(true);
        }
    }, [token]);

    const clearStats = () => {
        setStats(null);
        setShiftStats(null);
    };

    const setDeliveryStats = (stats: StatsType) => {
        setStats(stats);
    }

    const setShiftsStats = (stats: ShiftStatsType) => {
        setShiftStats(stats);
    }

    return (
        <SignalRContext.Provider value={{connection, stats, shiftStats, setDeliveryStats, setShiftsStats, clearStats}}>
            {children}
        </SignalRContext.Provider>
    );
};

export const useSignalR = () => {
    const context = useContext(SignalRContext);
    if (!context) {
        throw new Error("useSignalR must be used inside a SignalRProvider")
    }

    return context;
};