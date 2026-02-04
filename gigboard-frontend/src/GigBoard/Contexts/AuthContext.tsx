import {
  createContext,
  useContext,
  useEffect,
  useState,
  useCallback,
  useRef,
} from "react";
import type { ReactNode } from "react";
import { jwtDecode } from "jwt-decode";
import type { JwtPayload } from "jwt-decode";
import { useDispatch } from "react-redux";
import { setCurrentUser } from "../Account/reducer.ts";
import { useNavigate } from "react-router-dom";

type GigBoardJwt = {
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": string;
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": string;
} & JwtPayload;

type AuthContextType = {
  token: string | null;
  login: (token: string) => void;
  logout: () => void;
};

const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const [token, setToken] = useState<string | null>(() =>
    localStorage.getItem("token"),
  );
  const logoutTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const logout = useCallback(() => {
    console.log("Logging out");
    localStorage.removeItem("token");
    setToken(null);
    dispatch(setCurrentUser(null));

    if (logoutTimerRef.current) {
      clearTimeout(logoutTimerRef.current);
      logoutTimerRef.current = null;
  }
  }, [dispatch]);

  const processToken = useCallback(
    (rawToken: string) => {
      try {
        const decoded = jwtDecode<GigBoardJwt>(rawToken);
        const now = Math.floor(Date.now() / 1000);

        if (decoded.exp && decoded.exp < now) {
          logout();
          return;
        }

        setToken(rawToken);
        localStorage.setItem("token", rawToken);

        const expirationTime = decoded.exp || (now + 3600);
        const msRemaining = (expirationTime * 1000) - Date.now();

        if (logoutTimerRef.current) clearTimeout(logoutTimerRef.current);

        const timerId = setTimeout(() => {
          console.log('Token expired');
          logout();
          navigate('/GigBoard/Account/Login');
        }, msRemaining);

        logoutTimerRef.current = timerId;

        const user = {
          id: decoded[
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
          ],
          username:
            decoded[
              "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
            ],
        };
        dispatch(setCurrentUser(user));
      } catch (error) {
        console.error("Invalid token", error);
        logout();
      }
    },
    [dispatch, logout, navigate],
  );

  useEffect(() => {
    const storedToken = localStorage.getItem("token");
    if (storedToken) {
      processToken(storedToken);
    }
  }, [processToken]);

  useEffect(() => {
    const handleStorageChange = (event: StorageEvent) => {
      if (event.key === "token") {
        if (event.newValue === null) {
          logout();
        } else {
          processToken(event.newValue);
        }
      }
    };

    window.addEventListener("storage", handleStorageChange);

    return () => {
      window.removeEventListener("storage", handleStorageChange);
    };
  }, [logout, processToken]);

  return (
    <AuthContext.Provider value={{ token, login: processToken, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within an AuthProvider");
  return context;
};
