import { FormControl, FormGroup, FormLabel } from "react-bootstrap";
import { Button } from "@mui/material";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../Contexts/AuthContext";
import * as client from "./client";
import axios from "axios";

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
            setError("");
            const response = await client.loginUser({ username, password });
            login(response.token);
            navigate("/");
        } catch (err: unknown) {
          if (axios.isAxiosError(err)) {
            const respData = err.response?.data;
            setError(
              typeof respData === "string"
                ? respData
                : respData != null
                ? JSON.stringify(respData)
                : "An unexpected error occurred"
            );
          } else {
            setError("An unexpected error occurred");
          }
        } finally {
            setLoading(false);
        }
    };

    return (
      <div className="profile-container">
        <h1>Sign in</h1>
        <div id="login-form" className="profile-card">
          <FormGroup className="mb-4">
            <FormLabel className="profile-form-label">Username</FormLabel>
            <FormControl
              defaultValue={username}
              onChange={(e) => setUsername(e.target.value)}
              className="profile-form-control"
              placeholder="Username"
              id="wd-username"
            />
          </FormGroup>

          <FormGroup className="mb-4">
            <FormLabel className="profile-form-label">Password</FormLabel>
            <FormControl
              defaultValue={password}
              type="password"
              onChange={(e) => setPassword(e.target.value)}
              className="profile-form-control"
              placeholder="Password"
              id="wd-password"
            />
          </FormGroup>

          <div className="login-footer">
            <div className="login-message-area">
              {error && error.length > 0 && <p className="login-error-msg">{error}</p>}
              {loading && (
                <p className="login-loading-msg">
                  Loading, please allow up to 50s spinup time
                </p>
              )}
            </div>

            <Button
              onClick={handleLogin}
              variant="contained"
              className="profile-submit-btn"
              disableElevation
            >
              Sign in
            </Button>
          </div>
        </div>
      </div>
    );
}