import { Link, useLocation } from "react-router-dom";
import { useSelector } from "react-redux";
import { useAuth } from "./GigBoard/Contexts/AuthContext";
import type { RootState } from "./GigBoard/store";

export default function TopBar() {
  const { logout } = useAuth();
  const { pathname } = useLocation();
  const { currentUser } = useSelector(
    (state: RootState) => state.accountReducer,
  );

  const handleLogout = () => {
    logout();
  };

  return (
    <>
      <div className="top-bar-left">
        <span className="page-title">Overview</span>
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
    </>
  );
}
