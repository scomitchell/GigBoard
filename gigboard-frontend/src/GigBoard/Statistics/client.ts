import axios from "axios";

const axiosWithCredentials = axios.create({withCredentials: true});
export const REMOTE_SERVER = import.meta.env.VITE_REMOTE_SERVER;
export const STATISTICS_API = `${REMOTE_SERVER}/api/statistics`;

export const fetchDeliveryStats = async () => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${STATISTICS_API}/deliveries`, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });

    return response.data;
}

export const fetchShiftStats = async () => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${STATISTICS_API}/shifts`, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });

    return response.data;
}

export const fetchExpenseStats = async () => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.get(`${STATISTICS_API}/expenses`, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });

    return response.data;
}

export const predictShift = async (data: {startTime: string, endTime: string, app: string, neighborhood: string}) => {
    const token = localStorage.getItem("token");

    const response = await axiosWithCredentials.post(`${STATISTICS_API}/predict/shift-earnings`, data, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });

    return response.data;
}