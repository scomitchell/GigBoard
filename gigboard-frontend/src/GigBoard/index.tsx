import { Col } from "react-bootstrap";
import { Link } from "react-router-dom";
import { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { Card, CardContent, Typography, Button } from "@mui/material";
import { FaHome } from "react-icons/fa";
import Statistics from "./Statistics";
import * as client from "./Account/client";
import type { RootState } from "./store";
export default function GigBoard() {
    const [hasData, setHasData] = useState(false);
    const [loading, setLoading] = useState(true);
    const currentUser = useSelector((state: RootState) => state.accountReducer.currentUser);

    const fetchHasData = async () => {
        try {
            const userHasData = await client.getUserHasData();
            setHasData(userHasData);
        } catch {
            setHasData(false);
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        const token = localStorage.getItem("token");

        if (currentUser && token) {
            setLoading(true);
            fetchHasData();
        } else {
            setHasData(false);
            setLoading(false);
        }
    }, [currentUser]);

    if (loading) {
        return (
            <div>Loading...</div>
        );
    }

    if (!hasData) {
        return (
            <div id="home-header" className="ms-1">
                <h1>GigBoard</h1>
                <h2>Your insight dashboard</h2>
                <Col sm={9}>
                    <Card
                        sx={{
                            mb: 3,
                            borderRadius: 3,
                            boxShadow: 3,
                            p: 2,
                            minHeight: 300,
                        }}
                    >
                        <CardContent>
                            <Typography variant="h4" fontWeight="bold" sx={{ mb: 3 }}>
                                Welcome to GigBoard!
                            </Typography>
                            <Typography variant="h5" sx={{ mb: 3 }}>
                                Track your gig work shifts, deliveries, and expenses to get insights
                                and maximize your earnings.
                            </Typography>
                            {currentUser && (
                                <div className="mb-3">
                                    <Typography variant="body1" sx={{ mt: 2 }}>
                                        You can add deliveries in whichever way works best for you:
                                    </Typography>
                                    <ul>
                                        <li>Add a shift first, then log deliveries directly within that shift.</li>
                                        <li>Add a shift first, then add deliveries separately. The app will automatically link matching deliveries to the shift.</li>
                                        <li>Or add deliveries on their own without creating a shift.</li>
                                    </ul>
                                    <Typography variant="body1" fontWeight="bold" sx={{ mt: 1, mb: 1 }}>
                                        Once you've added your first delivery, return here by clicking <strong>Home</strong> (<FaHome fontSize="medium" />) in the navbar to see your personal statistics dashboard.
                                    </Typography>
                                    <Button variant="contained"
                                        color="primary"
                                        component={Link}
                                        to="/GigBoard/Shifts"
                                        sx={{
                                            mr: 3,
                                            textDecoration: 'none',
                                            '&:hover': {
                                                textDecoration: 'none',
                                                color: 'white',
                                            },
                                        }}>
                                        Add First Shift
                                    </Button>
                                    <Button variant="contained"
                                        color="success"
                                        component={Link}
                                        to="/GigBoard/MyDeliveries"
                                        sx={{
                                            mr: 3,
                                            textDecoration: 'none',
                                            '&:hover': {
                                                textDecoration: 'none',
                                                color: 'white',
                                            },
                                        }}>
                                        Add First Delivery
                                    </Button>
                                </div>
                            )}
                            {!currentUser && (
                                <Typography variant="body1" fontWeight="bold">
                                    Note: Backend is currently deployed with free tier of Azure,
                                    please allow up to 50s spinup time on Signup.
                                </Typography>
                            )}
                        </CardContent>
                    </Card>
                </Col>
            </div>
        );
    } else {
        return <Statistics />
    }
}