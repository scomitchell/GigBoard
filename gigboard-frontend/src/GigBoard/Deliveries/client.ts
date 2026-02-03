import axios from "axios";
import type { Delivery } from "../types";

const axiosWithCredentials = axios.create({ withCredentials: true });
export const REMOTE_SERVER = import.meta.env.VITE_REMOTE_SERVER;
export const DELIVERIES_API = `${REMOTE_SERVER}/api/userdelivery`;

export const findUserDeliveries = async () => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${DELIVERIES_API}/my-deliveries`, {
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export const findUnassignedUserDeliveries = async () => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${DELIVERIES_API}/unassigned-deliveries`, {
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export const findUserNeighborhoods = async () => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${DELIVERIES_API}/delivery-neighborhoods`, {
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export const findUserApps = async () => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${DELIVERIES_API}/delivery-apps`, {
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export const addUserDelivery = async (userDelivery: Delivery) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.post(`${DELIVERIES_API}`, userDelivery, {
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export type DeliveryFilters = {
    app?: string | null;
    basePay?: number | null;
    tipPay?: number | null;
    totalPay?: number | null;
    mileage?: number | null;
    restaurant?: string | null;
    customerNeighborhood?: string | null;
}

export const getFilteredDeliveries = async (filters: DeliveryFilters) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${DELIVERIES_API}/filtered-deliveries`, {
        params: filters,
        headers: {
            Authorization: `Bearer ${token}`
        },
    });
    return response.data;
}

export const deleteUserDelivery = async (deliveryId: number) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.delete(`${DELIVERIES_API}/${deliveryId}`, {
        headers: {
            Authorization: `Bearer ${token}`,
        }
    });

    return response.data;
}

export const updateUserDelivery = async (delivery: Delivery) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.put(`${DELIVERIES_API}`, delivery, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });

    return response.data;
}