import { Link, useLocation } from "react-router-dom";
import { useSelector } from "react-redux";
import { useAuth } from "./GigBoard/Contexts/AuthContext";
import { useState } from "react";
import { FaHome, FaBars, FaTimes } from "react-icons/fa";
import { BsBagFill, BsCurrencyDollar, BsClockFill } from "react-icons/bs";
import type { RootState } from "./GigBoard/store";

export default function TopBar() {
  const { logout } = useAuth();
  const { pathname } = useLocation();
  const { currentUser } = useSelector(
    (state: RootState) => state.accountReducer,
  );
  const [menuOpen, setMenuOpen] = useState(false);

  const handleLogout = () => {
    logout();
  };

  const toggleMenu = () => {
    setMenuOpen(!menuOpen);
  };

  const closeMenu = () => {
    setMenuOpen(false);
  };

  return (
    <>
      <div className="top-bar-left">
        <button
          className="mobile-menu-btn d-lg-none"
          onClick={toggleMenu}
          aria-label="Toggle navigation menu"
          style={{
            background: "none",
            border: "none",
            color: "#4B5563",
            cursor: "pointer",
            padding: "0.5rem",
            display: "flex",
            alignItems: "center",
            marginRight: "1rem",
          }}
        >
          {menuOpen ? <FaTimes size={24} /> : <FaBars size={24} />}
        </button>
        <span className="page-title">
          {pathname.includes("MyDeliveries") ?
            "Deliveries" :
            pathname.includes("Signup") ?
            "Sign up" :
            Number.isInteger(Number(pathname.substring(pathname.lastIndexOf("/") + 1))) ?
            "Individual Shift" :
            pathname.substring(pathname.lastIndexOf("/") + 1)
          }
        </span>
      </div>

      <div
        className="top-bar-right"
        style={{ display: "flex", gap: "1.25rem", alignItems: "center" }}
      >
        {!currentUser ? (
          <>
            <Link
              to="/GigBoard/Account"
              style={{
                textDecoration: "none",
                color: pathname === "/GigBoard/Account" ? "#000000" : "#4B5563",
                fontWeight: 600,
              }}
            >
              Log in
            </Link>

            <Link
              to="/GigBoard/Account/Signup"
              style={{
                textDecoration: "none",
                backgroundColor: "#1E293B",
                color: "#FFFFFF",
                padding: "0.5rem 1.25rem",
                borderRadius: "8px",
                fontWeight: 600,
              }}
            >
              Sign up
            </Link>
          </>
        ) : (
          <>
            <Link
              to="/GigBoard/Account/Profile"
              style={{
                textDecoration: "none",
                color: pathname.includes("Profile") ? "#000000" : "#4B5563",
                fontWeight: 600,
              }}
            >
              Profile
            </Link>

            <button
              onClick={handleLogout}
              style={{
                border: "none",
                backgroundColor: "#F3F4F6",
                color: "#4B5563",
                padding: "0.5rem 1.25rem",
                borderRadius: "8px",
                fontWeight: 600,
                cursor: "pointer",
              }}
            >
              Logout
            </button>
          </>
        )}
      </div>

      {/* Mobile Dropdown Menu */}
      {menuOpen && currentUser && (
        <div className="mobile-nav-dropdown">
          <Link
            to="/GigBoard"
            onClick={closeMenu}
            className={`mobile-nav-item ${pathname === "/GigBoard" ? "active" : ""}`}
          >
            <FaHome size={20} />
            <span>Home</span>
          </Link>

          <Link
            to="/GigBoard/MyDeliveries"
            onClick={closeMenu}
            className={`mobile-nav-item ${pathname.includes("MyDeliveries") ? "active" : ""}`}
          >
            <BsBagFill size={20} />
            <span>Deliveries</span>
          </Link>

          <Link
            to="/GigBoard/Shifts"
            onClick={closeMenu}
            className={`mobile-nav-item ${pathname.includes("Shifts") ? "active" : ""}`}
          >
            <BsClockFill size={20} />
            <span>Shifts</span>
          </Link>

          <Link
            to="/GigBoard/Expenses"
            onClick={closeMenu}
            className={`mobile-nav-item ${pathname.includes("Expenses") ? "active" : ""}`}
          >
            <BsCurrencyDollar size={20} />
            <span>Expenses</span>
          </Link>
        </div>
      )}
    </>
  );
}
