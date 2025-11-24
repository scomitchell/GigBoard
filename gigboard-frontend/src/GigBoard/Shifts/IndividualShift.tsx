import { useEffect, useState} from "react";
import {Row, Col, Modal, FormGroup, FormControl, FormLabel} from "react-bootstrap";
import { Card, CardContent, Typography, Button } from "@mui/material";
import { useParams } from "react-router-dom";
import * as client from "./client";
import * as deliveryClient from "../Deliveries/client";

export default function IndividualShift() {
    const { shiftId } = useParams();

    const [shiftDeliveries, setShiftDeliveries] = useState<any[]>([]);
    const [shift, setShift] = useState<any>({});

    const [delivery, setDelivery] = useState<any | null>(null);
    const [showForm, setShowForm] = useState(false);

    const fetchShift = async () => {
        const userShift = await client.findShiftById(Number(shiftId));
        setShift(userShift);
    }

    const fetchDeliveriesForShift = async () => {
        const userShiftDeliveries = await client.findDeliveriesForShift(Number(shiftId));
        setShiftDeliveries(userShiftDeliveries);
    }

    const addDeliveryToShift = async () => {
        if (delivery.deliveryTime < shift.startTime 
            || delivery.deliveryTime > shift.endTime
            || delivery.app != shift.app) {
            alert("Date and app must match shift");
            return;
        }

        await deliveryClient.addUserDelivery(delivery);
        fetchDeliveriesForShift();
        setShowForm(false);
    }

    // Display time as date, time
    const formatTime = (date: string) => {
        const newDate = new Date(date);
        const options: Intl.DateTimeFormatOptions = {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
            hour12: true
        };
        return newDate.toLocaleString(undefined, options);
    }

    useEffect(() => {
        fetchShift();
        fetchDeliveriesForShift();
    }, [])

    return (
        <div id="individiual-shifts">
            <h1>Details for shift: {formatTime(shift.startTime)} - {formatTime(shift.endTime)}</h1>
            <h2 className="mb-3">App: {shift.app}</h2>

            <div id="add-deliveries" className="d-flex align-items-center mb-3">
                <h3 className="me-2">Deliveries:</h3>
                <Button
                    onClick={() => {
                        setDelivery({
                            app: shift.app, // this ensures app is pre-populated
                            deliveryTime: "",
                            basePay: "",
                            tipPay: "",
                            mileage: "",
                            restaurant: "",
                            customerNeighborhood: "",
                            notes: "",
                        });
                        setShowForm(true);
                    }}
                    variant="contained"
                    color="primary"
                >
                    Add Delivery
                </Button>

                {/*Modal form for creating new deliveries*/}
                <Modal show={showForm} onHide={() => setShowForm(false)} centered size="lg">
                    <Modal.Header closeButton>
                        <Modal.Title>Add New Delivery</Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        <div className="add-delivery-details">
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">App</FormLabel>
                                <Col sm={7}>
                                    <select onChange={(e) => setDelivery({...delivery, app: e.target.value})}
                                        className="form-control mb-2" id="da-app">
                                        <option value={shift.app}>{shift.app}</option>
                                    </select>
                                </Col>
                            </FormGroup>
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">Time</FormLabel>
                                <Col sm={7}>
                                    <FormControl type="datetime-local"
                                        onChange={(e) => setDelivery({ ...delivery, deliveryTime: e.target.value })}
                                        defaultValue={shift.startTime}
                                    />
                                </Col>
                            </FormGroup>
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">Base Pay</FormLabel>
                                <Col sm={7}>
                                    <FormControl
                                        type="number"
                                        step="0.01"
                                        min="1.00"
                                        placeholder="Base Pay"
                                        onChange={(e) => setDelivery({...delivery, basePay: e.target.value})}
                                    />
                                </Col>
                            </FormGroup>
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">Tip Pay</FormLabel>
                                <Col sm={7}>
                                    <FormControl
                                        type="number"
                                        step="0.01"
                                        min="1.00"
                                        placeholder="Tip Pay"
                                        onChange={(e) => setDelivery({...delivery, tipPay: e.target.value})}
                                    />
                                </Col>
                            </FormGroup>
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">Mileage</FormLabel>
                                <Col sm={7}>
                                    <FormControl
                                        type="number"
                                        step="0.01"
                                        min="0.01"
                                        placeholder="Mileage"
                                        onChange={(e) => setDelivery({ ...delivery, mileage: e.target.value })}
                                    />
                                </Col>
                            </FormGroup>
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">Restaurant</FormLabel>
                                <Col sm={7}>
                                    <FormControl
                                        type="text"
                                        placeholder="Restaurant"
                                        onChange={(e) => setDelivery({...delivery, restaurant: e.target.value})}
                                    />
                                </Col>
                            </FormGroup>
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">Customer Neighborhood</FormLabel>
                                <Col sm={7}>
                                    <FormControl
                                        type="text"
                                        placeholder="Customer Neighborhood"
                                        onChange={(e) => setDelivery({...delivery, customerNeighborhood: e.target.value})}
                                    />
                                </Col>
                            </FormGroup>
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">Notes</FormLabel>
                                <Col sm={7}>
                                    <FormControl
                                        type="text"
                                        placeholder="Notes"
                                        onChange={(e) => setDelivery({...delivery, notes: e.target.value})}
                                    />
                                </Col>
                            </FormGroup>
                            <Button variant="contained" color="primary" onClick={addDeliveryToShift}>
                                Add Delivery
                            </Button>
                        </div>
                    </Modal.Body>
                </Modal>
            </div>

            {shiftDeliveries.map((delivery: any) => 
                <Col sm={6} key={delivery.id}>
                    <Card sx={{
                        mb: 3,
                        textAlign: "start",
                        borderRadius: 3,
                        boxShadow: 3,
                        position: "relative",
                        transition: "0.3s",
                    }}>
                        <CardContent sx={{ p: 2 }}>
                            <Typography variant="h6" fontWeight="bold">Total Pay: ${delivery.totalPay.toFixed(2)}</Typography>
                            <Typography sx={{ mt: 1 }}>
                                <strong>Date Completed:</strong> {formatTime(delivery.deliveryTime)} {" "} <br />
                                <strong>Base Pay:</strong> ${delivery.basePay.toFixed(2)} {" "} <br />
                                <strong>Tip Pay:</strong> ${delivery.tipPay.toFixed(2)} {" "} <br />
                                <strong>Mileage:</strong> {delivery.mileage.toFixed(2)} {" miles"} <br />
                                <strong>App:</strong> {delivery.app} {" "} <br />
                                <strong>Restaurant:</strong> {delivery.restaurant} {" "} <br />
                                <strong>Customer Neighborhood:</strong> {delivery.customerNeighborhood} {" "} <br />
                                <strong>Notes:</strong> {delivery.notes} {" "}
                            </Typography>
                        </CardContent>
                    </Card> 
                </Col>
            )}
        </div>
    );
}