import { Link, useLocation } from "react-router-dom";
import { useSelector } from "react-redux";
import { FaHome } from "react-icons/fa";
import { BsBagFill, BsCurrencyDollar, BsClockFill } from "react-icons/bs";
import type { RootState } from "./store";

export default function Navigation() {
    const { pathname } = useLocation();
    const { currentUser } = useSelector((state: RootState) => state.accountReducer);

    return (
        <nav className="main-navbar d-flex flex-column position-fixed top-0 start-0 z-3 vh-100"
            style={{width: "100px"}}>
            <Link to="/GigBoard"
                className={`nav-link-hover d-flex flex-column align-items-center px-4 py-2 
                    text-decoration-none border-0 ms-2 mt-2 me-2 mb-2 rounded
                    ${pathname === "/GigBoard" ? "active-link" : "text-white"}`}>
                <FaHome size={32} className="mb-1" />
                <span>Home</span>
            </Link>

            {!currentUser ?
                <>
                </>

                :

                <>
                    <Link to="/GigBoard/MyDeliveries"
                        className={`nav-link-hover d-flex flex-column align-items-center px-4 py-2 
                            text-decoration-none border-0 ms-2 mt-2 me-2 mb-2 rounded
                    ${pathname.includes("MyDeliveries") ? "active-link" : "text-white"}`}>
                        <BsBagFill size={32} className="mb-1" />
                        <span>Deliveries</span>
                    </Link>

                    <Link to="/GigBoard/Shifts"
                        className={`nav-link-hover d-flex flex-column align-items-center px-4 py-2 
                            text-decoration-none border-0 ms-2 mt-2 me-2 mb-2 rounded
                    ${pathname.includes("Shifts") ? "active-link" : "text-white"}`}>
                        <BsClockFill size={32} className="mb-1" />
                        <span>Shifts</span>
                    </Link>

                    <Link to="/GigBoard/Expenses"
                        className={`nav-link-hover d-flex flex-column align-items-center px-4 py-2 
                            text-decoration-none border-0 ms-2 mt-2 me-2 mb-2 rounded
                    ${pathname.includes("Expenses") ? "active-link" : "text-white"}`}>
                        <BsCurrencyDollar size={32} className="mb-1" />
                        <span>Expenses</span>
                    </Link>
                </>
            }
        </nav>
    );
}