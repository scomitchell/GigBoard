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
    clearStats: () => void
};

type StatsType = {
    avgPay: number,
    avgBase: number,
    avgTip: number,
    dollarPerMile: number,
    highestPayingRestaurant: {restaurant: string, avgTotalPay: number},
    restaurantWithMost: {restaurantWithMost: string, orderCount: number},
    tipPerMile: number,
    plotlyEarningsData: {dates: string[], earnings: number[]},
    plotlyNeighborhoodsData: {neighborhoods: string[], tipPays: number[]},
    appsByBaseData: {apps: string[], basePays: number[]},
    tipsByAppData: {tipApps: string[], appTipPays: number[]}
};

const SignalRContext = createContext<SignalRContextType | null>(null);

export const SignalRProvider = ({ children, token }: SignalRProps) => {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null)
    const [stats, setStats] = useState<StatsType | null>(null);
    const [initialized, setInitialized] = useState(false);
    const REMOTE_SERVER = import.meta.env.VITE_REMOTE_SERVER;

    useEffect(() => {
        if (!token) {
            if (connection) {
                connection.stop();
                console.log("Connection Stopped");
                setConnection(null);
                setStats(null);
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
        })

        setConnection(conn);

        return () => {
            conn.stop();
          };
    }, [token]);

    useEffect(() => {
        if (initialized) {
            setStats(null);
        } else {
            setInitialized(true);
        }
    }, [token]);

    const clearStats = () => setStats(null);

    return (
        <SignalRContext.Provider value={{connection, stats, clearStats}}>
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