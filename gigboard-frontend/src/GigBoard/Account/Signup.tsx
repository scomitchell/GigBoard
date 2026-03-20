import { useState } from "react";
import { Col, FormControl, FormGroup, FormLabel, Row } from "react-bootstrap";
import { Button } from "@mui/material";
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
    setTimeout(
      () => {
        // Clear token and log out user
        localStorage.removeItem("token");
        navigate("/GigBoard/Account/Login");
      },
      60 * 60 * 1000,
    );
  }

  const signup = async () => {
    setLoading(true);
    try {
      const response = await client.registerUser({
        firstName,
        lastName,
        email,
        username,
        password,
      });
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
    <div id="da-signup-form" className="profile-container">
      <h1>Sign up</h1>
      <div className="profile-card">
        <Row>
          <Col md={6}>
            <FormGroup className="mb-4">
              <FormLabel className="profile-form-label">First Name</FormLabel>
              <FormControl
                defaultValue={firstName}
                type="text"
                onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                  setFirstName(e.target.value)
                }
                className="profile-form-control"
                placeholder="First Name"
                id="da-firstname"
              />
            </FormGroup>
          </Col>

          <Col>
            <FormGroup className="mb-4">
              <FormLabel className="profile-form-label">Last Name</FormLabel>
              <FormControl
                defaultValue={lastName}
                type="text"
                onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                  setLastName(e.target.value)
                }
                className="profile-form-control"
                placeholder="Last Name"
                id="da-lastname"
              />
            </FormGroup>
          </Col>
        </Row>
        <FormGroup className="mb-4">
          <FormLabel className="profile-form-label">Email Address</FormLabel>
          <FormControl
            defaultValue={email}
            type="email"
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setEmail(e.target.value)
            }
            className="profile-form-control"
            placeholder="youremail@example.com"
            id="da-email"
          />
        </FormGroup>

        <FormGroup className="mb-4">
          <FormLabel className="profile-form-label">Username</FormLabel>
          <FormControl
            defaultValue={username}
            type="text"
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setUsername(e.target.value)
            }
            className="profile-form-control"
            placeholder="Username"
            id="da-username"
          />
        </FormGroup>

        <FormGroup className="mb-4">
          <FormLabel className="profile-form-label">Password</FormLabel>
          <FormControl
            defaultValue={password}
            type="password"
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setPassword(e.target.value)
            }
            className="profile-form-control"
            placeholder="Password"
            id="da-password"
          />
        </FormGroup>

        <div className="login-footer">
          <div className="login-message-area">
            {error.length > 0 && <p className="login-error-msg">{error}</p>}
            {loading && (
              <p className="login-loading-msg">
                Loading, please allow up to 50s spinup time
              </p>
            )}
          </div>

          <Button
            onClick={signup}
            variant="contained"
            className="profile-submit-btn"
            disableElevation
          >
            Sign Up
          </Button>
        </div>
      </div>
    </div>
  );
}
