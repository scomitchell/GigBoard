import GigBoard from "./GigBoard";
import Navigation from "./GigBoard/Navigation";
import Deliveries from "./GigBoard/Deliveries";
import Shifts from "./GigBoard/Shifts";
import Expenses from "./GigBoard/Expenses";
import Account from "./GigBoard/Account";
import IndividualShift from "./GigBoard/Shifts/IndividualShift";
import store from "./GigBoard/store";
import { Provider } from "react-redux";
import { HashRouter, Route, Routes, Navigate } from "react-router-dom";
import "./App.css";
import { AuthProvider } from "./GigBoard/Contexts/AuthContext.tsx";
import { SignalRProvider } from "./GigBoard/SignalRProvider.tsx";

export default function App() {
  return (
    <HashRouter>
      <Provider store={store}>
        <AuthProvider>
          <SignalRProvider>
            <div id="da-main-app">
              <Navigation />
              <Routes>
                <Route path="/" element={<Navigate to="GigBoard" />} />
                <Route path="/GigBoard/*" element={<GigBoard />} />
                <Route
                  path="/GigBoard/MyDeliveries/*"
                  element={<Deliveries />}
                />
                <Route path="/GigBoard/Shifts/*" element={<Shifts />} />
                <Route
                  path="/GigBoard/Shifts/:shiftId"
                  element={<IndividualShift />}
                />
                <Route path="/GigBoard/Expenses/*" element={<Expenses />} />
                <Route path="/GigBoard/Account/*" element={<Account />} />
              </Routes>
            </div>
          </SignalRProvider>
        </AuthProvider>
      </Provider>
    </HashRouter>
  );
}
