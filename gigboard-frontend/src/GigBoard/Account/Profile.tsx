import { useCallback, useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { setCurrentUser } from "./reducer";
import { Row, Col, FormLabel, FormGroup, FormControl } from "react-bootstrap";
import { Button } from "@mui/material";
import * as client from "./client";
import type { User } from "../types";
import type { RootState } from "../store";
import "./account.css";

export default function Profile() {
  const [user, setUser] = useState<User>();
  const [password, setPassword] = useState("");
  const { currentUser } = useSelector(
    (state: RootState) => state.accountReducer,
  );
  const [loading, setLoading] = useState(true);

  const dispatch = useDispatch();

  const fetchProfile = useCallback(async () => {
    if (!currentUser) return;
    if (!currentUser.username) return;
    const user = await client.getUserByUsername(currentUser.username);
    setUser(user);
    setLoading(false);
  }, [currentUser]);

  const updateProfile = async () => {
    const payload = { ...user };
    if (password.length > 0) {
      payload.password = password;
    }

    const updatedProfile = await client.updateUser(payload);
    dispatch(setCurrentUser(updatedProfile));
  };

  useEffect(() => {
    setLoading(true);
    fetchProfile();
  }, [currentUser, fetchProfile]);

  if (loading) {
    return <p>Loading...</p>;
  }

  return (
    <div className="profile-container">
      <h1>Your Profile</h1>

      <div className="profile-card">
        <Row>
          <Col md={6}>
            <FormGroup className="mb-4">
              <FormLabel className="profile-form-label">First Name</FormLabel>
              <FormControl
                placeholder="First Name"
                id="da-firstname"
                defaultValue={user?.firstName}
                onChange={(e) =>
                  setUser(
                    user ? { ...user, firstName: e.target.value } : undefined,
                  )
                }
                className="profile-form-control"
              />
            </FormGroup>
          </Col>

          <Col md={6}>
            <FormGroup className="mb-4">
              <FormLabel className="profile-form-label">Last Name</FormLabel>
              <FormControl
                placeholder="Last Name"
                id="da-lastname"
                defaultValue={user?.lastName}
                onChange={(e) =>
                  setUser(
                    user ? { ...user, lastName: e.target.value } : undefined,
                  )
                }
                className="profile-form-control"
              />
            </FormGroup>
          </Col>
        </Row>

        <FormGroup className="mb-4">
          <FormLabel className="profile-form-label">Email Address</FormLabel>
          <FormControl
            placeholder="Email"
            type="email"
            id="da-email"
            defaultValue={user?.email}
            onChange={(e) =>
              setUser(user ? { ...user, email: e.target.value } : undefined)
            }
            className="profile-form-control"
          />
        </FormGroup>

        <FormGroup className="mb-4">
          <FormLabel className="profile-form-label">Username</FormLabel>
          <FormControl
            placeholder="Username"
            id="da-username"
            defaultValue={user?.username}
            onChange={(e) =>
              setUser(user ? { ...user, username: e.target.value } : undefined)
            }
            className="profile-form-control"
          />
        </FormGroup>

        <hr className="profile-divider" />

        <FormGroup className="mb-4">
          <FormLabel className="profile-form-label">New Password</FormLabel>
          <FormControl
            placeholder="Leave blank to keep current password"
            id="da-password"
            defaultValue=""
            type="password"
            onChange={(e) => setPassword(e.target.value)}
            className="profile-form-control"
          />
        </FormGroup>

        <div className="profile-button-container">
          <Button
            onClick={updateProfile}
            id="da-update-profile-btn"
            variant="contained"
            disableElevation
            className="profile-submit-btn"
          >
            Update Profile
          </Button>
        </div>
      </div>
    </div>
  );
}
