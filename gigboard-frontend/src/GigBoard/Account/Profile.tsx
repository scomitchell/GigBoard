import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { setCurrentUser } from "./reducer";
import { Row, Col, FormLabel, FormGroup, FormControl, Button } from "react-bootstrap";
import * as client from "./client";
import type { User } from "../types";
import type { RootState } from "../store";

export default function Profile() {
    const [user, setUser] = useState<User>();
    const [password, setPassword] = useState("");
    const { currentUser } = useSelector((state: RootState) => state.accountReducer);
    const [loading, setLoading] = useState(true);

    const dispatch = useDispatch();

    const fetchProfile = async () => {
        if (!currentUser) return;
        if (!currentUser.username) return;
        const user = await client.getUserByUsername(currentUser.username);
        setUser(user);
        setLoading(false);
    }

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
    }, [currentUser]);

    if (loading) {
        return (
            <p>Loading...</p>
        );
    }

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
            <div id="da-profile" style={{ width: "75%", height: "100%" }}>
                <h1 style={{ textAlign: "center" }}>Your Profile</h1>
                <FormGroup as={Row} className="mb-3 mt-4 align-items-center d-flex">
                    <FormLabel column sm={3} className="text-sm-end">First Name</FormLabel>
                    <Col sm={9}>
                        <FormControl
                            placeholder="First Name"
                            id="da-firstname"
                            defaultValue={user && user.firstName}
                            onChange={(e) => setUser({ ...user, firstName: e.target.value })}
                        />
                    </Col>
                </FormGroup>

                <FormGroup as={Row} className="mb-3 align-items-center d-flex">
                    <FormLabel column sm={3} className="text-sm-end">Last Name</FormLabel>
                    <Col sm={9}>
                        <FormControl
                            placeholder="Last Name"
                            id="da-lastname"
                            defaultValue={user && user.lastName}
                            onChange={(e) => setUser({ ...user, lastName: e.target.value })}
                        />
                    </Col>
                </FormGroup>

                <FormGroup as={Row} className="mb-3 align-items-center d-flex">
                    <FormLabel column sm={3} className="text-sm-end">Email</FormLabel>
                    <Col sm={9}>
                        <FormControl
                            placeholder="Email"
                            type="email"
                            id="da-lastname"
                            defaultValue={user && user.email}
                            onChange={(e) => setUser({ ...user, email: e.target.value })}
                        />
                    </Col>
                </FormGroup>

                <FormGroup as={Row} className="mb-3 align-items-center d-flex">
                    <FormLabel column sm={3} className="text-sm-end">Username</FormLabel>
                    <Col sm={9}>
                        <FormControl
                            placeholder="Username"
                            id="da-username"
                            defaultValue={user && user.username}
                            onChange={(e) => setUser({ ...user, username: e.target.value })}
                        />
                    </Col>
                </FormGroup>

                <FormGroup as={Row} className="mb-3 align-items-center d-flex">
                    <FormLabel column sm={3} className="text-sm-end">Password</FormLabel>
                    <Col sm={9}>
                        <FormControl
                            placeholder="Password"
                            id="da-password"
                            defaultValue=""
                            type="password"
                            className="mb-3"
                            onChange={(e) => setPassword(e.target.value)}
                        />
                    </Col>
                </FormGroup>

                <FormGroup as={Row} className="mb-3 d-flex" style={{alignItems: "end"}}>
                    <Col sm={2} />
                    <Col sm={10}>
                        <Button onClick={updateProfile} id="da-update-profile-btn" className="btn btn-primary w-100">
                            Update Profile
                        </Button>
                    </Col>
                </FormGroup>
            </div>
        </div>
    );
}