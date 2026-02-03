import GigBoard from "./GigBoard";
import Navigation from "./GigBoard/Navigation";
import Deliveries from "./GigBoard/Deliveries";
import Shifts from "./GigBoard/Shifts";
import Expenses from "./GigBoard/Expenses";
import Account from "./GigBoard/Account";
import IndividualShift from "./GigBoard/Shifts/IndividualShift";
import store from "./GigBoard/store";
import { Provider, useDispatch } from "react-redux";
import { useCallback, useEffect, useState } from "react";
import { HashRouter, Route, Routes, Navigate, useNavigate } from "react-router-dom";
import { setCurrentUser } from "./GigBoard/Account/reducer";
import { SignalRProvider } from "./GigBoard/SignalRProvider.tsx";
import {jwtDecode } from "jwt-decode";
import { useSignalR } from "./GigBoard/SignalRContext.ts";
import type { JwtPayload } from "jwt-decode";
import './App.css'

type GigBoardJwt = {
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": string;
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": string;
} & JwtPayload;

function AuthTokenListener() {
    const navigate = useNavigate();
    const dispatch = useDispatch();
    const { clearStats } = useSignalR();

    const processToken = useCallback((rawToken: string | null) => {
        if (!rawToken) {
            dispatch(setCurrentUser(null));
            clearStats();
            return;
        }

        try {
            const decoded = jwtDecode<GigBoardJwt>(rawToken);
            const user = {
                id: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
                username: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]
            };

            const exp = decoded.exp;
            const now = Math.floor(Date.now() / 1000);

            if (!exp || exp < now) {
                localStorage.removeItem("token");
                clearStats();
                dispatch(setCurrentUser(null));
                navigate("/");
                return;
            }

            dispatch(setCurrentUser(user));
        } catch {
            localStorage.removeItem("token");
            dispatch(setCurrentUser(null));
        }
    }, [dispatch, navigate, clearStats]);

    useEffect(() => {
        const onStorageChange = (event: StorageEvent) => {
            if (event.key === "token") {
                processToken(event.newValue);
                navigate("/")
            }
        };

        window.addEventListener("storage", onStorageChange);
        return () => {
            window.removeEventListener("storage", onStorageChange);
        }
    }, [navigate, processToken]);

    useEffect(() => {
        processToken(localStorage.getItem("token"));
    }, [processToken]);

    return null;
}

export default function App() {
    const [token, setToken] = useState(localStorage.getItem("token"));

    useEffect(() => {
        const handleLogout = () => {
            setToken(null);
        };

        window.addEventListener("logout", handleLogout);

        return () => {
            window.removeEventListener("logout", handleLogout);
        };
    }, []);

    useEffect(() => {
        const handleLogin = () => {
            setToken(localStorage.getItem("token"));
        };

        window.addEventListener("login", handleLogin);

        return () => {
            window.removeEventListener("login", handleLogin);
        };
    }, []);
    
    return (
        <HashRouter>
            <SignalRProvider token={token}>
                <Provider store={store}>
                    <AuthTokenListener />
                    <div id="da-main-app">
                        <Navigation />
                        <Routes>
                            <Route path="/" element={<Navigate to="GigBoard" />} />
                            <Route path="/GigBoard/*" element={<GigBoard />} />
                            <Route path="/GigBoard/MyDeliveries/*" element={<Deliveries />} />
                            <Route path="/GigBoard/Shifts/*" element={<Shifts />} />
                            <Route path="/GigBoard/Shifts/:shiftId" element={<IndividualShift />} />
                            <Route path="/GigBoard/Expenses/*" element={<Expenses />} />
                            <Route path="/GigBoard/Account/*" element={<Account />} />
                        </Routes>
                    </div>
                </Provider>
            </SignalRProvider>
        </HashRouter>
    );
}
