import { Link, useLocation, useNavigate } from "react-router-dom";
import { useSelector, useDispatch } from "react-redux";
import { setCurrentUser } from "./Account/reducer"
import { FaHome } from "react-icons/fa";
import { useSignalR } from "./SignalRContext";
import { BsBagFill, BsPersonFillGear, BsCurrencyDollar, BsClockFill, BsArrowBarLeft, BsArrowBarUp } from "react-icons/bs";

export default function Navigation() {
    const navigate = useNavigate();
    const dispatch = useDispatch();
    const { pathname } = useLocation();
    const { currentUser } = useSelector((state: any) => state.accountReducer);
    const { clearStats } = useSignalR();

    const handleLogout = () => {
        clearStats();
        localStorage.removeItem("token");

        window.dispatchEvent(new Event("logout"));
        dispatch(setCurrentUser(null));
        navigate("/");
    };

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
                    <Link to="/GigBoard/Account"
                        className={`nav-link-hover d-flex flex-column align-items-center px-4 py-2 
                            text-decoration-none border-0 ms-2 mt-2 me-2 mb-2 rounded
                        ${pathname.includes("Login") ? "active-link" : "text-white"}`}>

                        <BsPersonFillGear size={32} className="mb-1" />
                        <span>Login</span>
                    </Link>

                    <Link to="/GigBoard/Account/Signup"
                        className={`nav-link-hover d-flex flex-column align-items-center px-4 py-2 
                            text-decoration-none border-0 ms-2 mt-2 me-2 mb-2 rounded
                        ${pathname.includes("Signup") ? "active-link" : "text-white"}`}>

                        <BsArrowBarUp size={32} className="mb-1" />
                        <span>Signup</span>
                    </Link>
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

                    <Link to="/GigBoard/Account/Profile"
                        className={`nav-link-hover d-flex flex-column align-items-center px-4 py-2 
                            text-decoration-none border-0 ms-2 mt-2 me-2 mb-2 rounded
                    ${pathname.includes("Profile") ? "active-link" : "text-white"}`}>
                        <BsPersonFillGear size={32} className="mb-1" />
                        <span>Profile</span>
                    </Link>

                    <Link to="/GigBoard"
                        className="nav-link-hover d-flex flex-column align-items-center px-4 py-2 
                            text-decoration-none border-0 text-white ms-2 mt-2 me-2 rounded mb-2"
                        onClick={handleLogout}
                        style={{ cursor: "pointer" }}>

                        <BsArrowBarLeft size={32} className="mb-1" />
                        <span>Logout</span>
                    </Link>
                </>
            }
        </nav>
    );
}