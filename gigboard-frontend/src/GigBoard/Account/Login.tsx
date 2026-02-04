import { FormControl, Button } from "react-bootstrap";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../Contexts/AuthContext";
import * as client from "./client";

export default function Login() {
    const { login } = useAuth();
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");

    const [loading, setLoading] = useState(false);

    const navigate = useNavigate();

    const handleLogin = async () => {
        setLoading(true);
        try {
            const response = await client.loginUser({ username, password });
            login(response.token);
            navigate("/");
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
        <div style={{
            display: "flex",
            justifyContent: "center", 
            alignItems: "center",     
            height: "100vh",          
            paddingLeft: "100px", 
            boxSizing: "border-box",
            backgroundColor: "white"
        }}>
            <div id="da-signin-screen" style={{width: "50%", height: "100%"}}>
                <h1 style={{textAlign: "center"}}>Sign in</h1>
                <FormControl defaultValue={username}
                    onChange={(e) => setUsername(e.target.value)}
                    className="mb-2 mt-4" placeholder="Username" id="wd-username" />
                <FormControl defaultValue={password} type="password"
                    onChange={(e) => setPassword(e.target.value)}
                    className="mb-2" placeholder="Password" id="wd-password" />
                <Button onClick={handleLogin} id="da-signin-button"
                    className="btn btn-primary w-100 mb-2">
                    Sign in
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