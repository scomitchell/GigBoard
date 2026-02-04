import { useEffect, useState } from "react";
import type { ReactNode } from "react";
import * as signalR from "@microsoft/signalr";
import type { MonthlySpendingType } from "./Statistics";
import { SignalRContext } from "./Contexts/SignalRContext";
import { useAuth } from "./Contexts/AuthContext";

export type StatsType = {
  avgPay: number;
  avgBase: number;
  avgTip: number;
  dollarPerMile: number;
  highestPayingRestaurant: { restaurant: string; avgTotalPay: number };
  restaurantWithMost: { restaurantWithMost: string; orderCount: number };
  tipPerMile: number;
  plotlyEarningsData: { dates: string[]; earnings: number[] } | null;
  plotlyNeighborhoodsData: {
    neighborhoods: string[];
    tipPays: number[];
  } | null;
  appsByBaseData: { apps: string[]; basePays: number[] } | null;
  tipsByAppData: { tipApps: string[]; appTipPays: number[] } | null;
  hourlyEarningsData: { hours: string[]; earnings: number[] } | null;
  donutChartData: {
    totalPay: number;
    totalBasePay: number;
    totalTipPay: number;
  } | null;
};

export type ShiftStatsType = {
  averageShiftLength: number;
  appWithMostShifts: string;
  averageDeliveriesForShift: number;
};

export type ExpenseStatsType = {
  averageMonthlySpending: number;
  averageSpendingByType: MonthlySpendingType[];
};

export const SignalRProvider = ({ children }: { children: ReactNode }) => {
  const { token, logout } = useAuth();
  const [connection, setConnection] = useState<signalR.HubConnection | null>(
    null,
  );

  // State holders
  const [stats, setStats] = useState<StatsType | null>(null);
  const [shiftStats, setShiftStats] = useState<ShiftStatsType | null>(null);
  const [expenseStats, setExpenseStats] = useState<ExpenseStatsType | null>(
    null,
  );

  // Remote Server
  const REMOTE_SERVER = import.meta.env.VITE_REMOTE_SERVER;

  const clearStats = () => {
    setStats(null);
    setShiftStats(null);
    setExpenseStats(null);
  };

  useEffect(() => {
    if (!token) {
      if (connection) {
        connection.stop().then(() => setConnection(null));
      }
      clearStats();
      return;
    }

    const conn = new signalR.HubConnectionBuilder()
      .withUrl(`${REMOTE_SERVER}/hubs/statistics`, {
        accessTokenFactory: () => token,
        transport:
          signalR.HttpTransportType.WebSockets |
          signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .build();

    conn.on("StatisticsUpdated", setStats);

    conn.on("ShiftStatisticsUpdated", setShiftStats);

    conn.on("ExpenseStatisticsUpdated", setExpenseStats);

    conn
      .start()
      .then(() => {
        console.log("SignalR connected");
        setConnection(conn);
      })
      .catch((err) => {
        console.error("SignalR connection failed:", err);
        if (err.message && err.message.includes("401")) {
          logout();
        }
      });

    return () => {
      conn.stop();
      setConnection(null);
    };
  }, [REMOTE_SERVER, logout, token]);

  return (
    <SignalRContext.Provider
      value={{
        connection,
        stats,
        shiftStats,
        expenseStats,
        setDeliveryStats: setStats,
        setShiftStats,
        setExpenseStats,
        clearStats,
      }}
    >
      {children}
    </SignalRContext.Provider>
  );
};
