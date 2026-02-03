import axios from "axios";
import type { Expense } from "../types";

const axiosWithCredentials = axios.create({withCredentials: true});
const REMOTE_SERVER = import.meta.env.VITE_REMOTE_SERVER;
const EXPENSES_API = `${REMOTE_SERVER}/api/userexpense`;

export const addExpense = async (userExpense: Expense) => {
    const token = localStorage.getItem("token");
    const response = await axiosWithCredentials.post(`${EXPENSES_API}`, userExpense, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });

    return response.data;
}

export const findMyExpenses = async () => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${EXPENSES_API}/my-expenses`, {
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export const findExpenseById = async (expenseId: number) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${EXPENSES_API}/${expenseId}`, {
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export const deleteExpense = async (expenseId: number) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.delete(`${EXPENSES_API}/${expenseId}`, {
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export type ExpenseFilters = {
    amount?: number | null;
    date?: string | null;
    type?: string | null;
}

export const findFilteredExpenses = async (filters: ExpenseFilters) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${EXPENSES_API}/filtered-expenses`, {
        params: filters,
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export const findExpenseTypes = async () => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${EXPENSES_API}/types`, {
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export const UpdateUserExpense = async (expense: Expense) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.put(`${EXPENSES_API}`, expense, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });

    return response.data;
}