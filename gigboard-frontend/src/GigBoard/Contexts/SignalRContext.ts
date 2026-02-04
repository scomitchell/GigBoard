import { createContext, useContext } from "react";
import type { ExpenseStatsType, ShiftStatsType, StatsType } from "../SignalRProvider";
import * as signalR from "@microsoft/signalr";

type SignalRContextType = {
    connection: signalR.HubConnection | null,
    stats: StatsType | null,
    shiftStats: ShiftStatsType | null,
    expenseStats: ExpenseStatsType | null,
    clearStats: () => void,
    setDeliveryStats: (stats: StatsType) => void,
    setShiftStats: (stats: ShiftStatsType) => void,
    setExpenseStats: (stats: ExpenseStatsType) => void
};

export const SignalRContext = createContext<SignalRContextType | null>(null);

export const useSignalR = () => {
    const context = useContext(SignalRContext);
    if (!context) {
        throw new Error("useSignalR must be used inside a SignalRProvider")
    }

    return context;
};