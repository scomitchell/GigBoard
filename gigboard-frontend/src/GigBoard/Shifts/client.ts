import axios from "axios";
import type { Shift } from "../types";

const axiosWithCredentials = axios.create({withCredentials: true});
export const REMOTE_SERVER = import.meta.env.VITE_REMOTE_SERVER;
export const SHIFTS_API = `${REMOTE_SERVER}/api/usershift`;
export const SHIFTDELIVERIES_API = `${REMOTE_SERVER}/api/shiftdelivery`;

export const findUserShifts = async () => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${SHIFTS_API}/my-shifts`, {
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export const findShiftById = async (id: number) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${SHIFTS_API}/${id}`, {
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export const addShift = async (userShift: Shift) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.post(`${SHIFTS_API}`, userShift, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });

    return response.data;
}

export const getUserApps = async () => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${SHIFTS_API}/apps`, {
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export type ShiftFilters = {
    startTime?: string | null;
    endTime?: string | null;
    app?: string | null;
}

export const getFilteredShifts = async (filters: ShiftFilters) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${SHIFTS_API}/filtered-shifts`, {
        params: filters,
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export const deleteUserShift = async (shiftId: number) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.delete(`${SHIFTS_API}/${shiftId}`, {
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export const AddShiftDelivery = async (shiftId: number, deliveryId: number) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.post(`${SHIFTDELIVERIES_API}/${shiftId}?deliveryId=${deliveryId}`, {}, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });

    return response.data;
}

export const findDeliveriesForShift = async (shiftId: number) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${SHIFTDELIVERIES_API}/${shiftId}`, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });

    return response.data;
}

export const updateUserShift = async (shift: Shift) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.put(`${SHIFTS_API}`, shift, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });

    return response.data;
}