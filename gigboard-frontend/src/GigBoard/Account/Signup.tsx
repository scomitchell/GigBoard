import { useState } from "react";
import { FormControl, Button } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { useDispatch } from "react-redux";
import { setCurrentUser } from "./reducer";
import * as client from "./client";

export default function Signup() {
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [email, setEmail] = useState("");
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");

    const [loading, setLoading] = useState(false);

    const navigate = useNavigate();
    const dispatch = useDispatch();

    function scheduleAutoLogout() {
        setTimeout(() => {
            // Clear token and log out user
            localStorage.removeItem("token");
            navigate("/GigBoard/Account/Login");
        }, 60 * 60 * 1000);
    }

    const signup = async () => {
        setLoading(true);
        try {
            const response = await client.registerUser({ firstName, lastName, email, username, password });
            localStorage.setItem("token", response.token);
            scheduleAutoLogout();

            dispatch(setCurrentUser(response.user));
            window.dispatchEvent(new Event("login"));
            navigate("/");
            setLoading(false);
        } catch (err: Error | unknown) {
            if (err instanceof Error) {
                setError(err.message);
            } else {
                setError("An unexpected error occurred");
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div id="da-signup-form" style={{
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            height: "100vh",
            paddingLeft: "100px",
            boxSizing: "border-box",
            backgroundColor: "white"
        }}>
            <div style={{width: "50%", height: "100%"}}>
                <h1 style={{textAlign: "center"}}>Sign up</h1>
                <FormControl defaultValue={firstName} type="text"
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => setFirstName(e.target.value)}
                    className="mb-2 mt-4" placeholder="First Name" id="da-firstname" />
                <FormControl defaultValue={lastName} type="text"
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => setLastName(e.target.value)}
                    className="mb-2" placeholder="Last Name" id="da-lastname" />
                <FormControl defaultValue={email} type="email"
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => setEmail(e.target.value)}
                    className="mb-2" placeholder="Email" id="da-email" />
                <FormControl defaultValue={username} type="text"
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => setUsername(e.target.value)}
                    className="mb-2" placeholder="Username" id="da-username" />
                <FormControl defaultValue={password} type="password"
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => setPassword(e.target.value)}
                    className="mb-2" placeholder="Password" id="da-password" />
                <Button onClick={signup} id="da-signin-button"
                    className="btn btn-primary w-100 mb-2">
                    Sign Up
                </Button>
                {error.length > 0 ? <p>{error}</p> : null}
                {loading ?
                    <p>Loading, please allow up to 50s spinup time</p>
                    :
                    null
                }
            </div>
        </div>
    );
}
