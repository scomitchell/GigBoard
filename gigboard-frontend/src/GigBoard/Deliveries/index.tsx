import { Modal, FormGroup, FormLabel, FormControl, Row, Col } from "react-bootstrap";
import { Button } from "@mui/material";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import MyDeliveries from "./MyDeliveries";
import * as client from "./client";
import type { Delivery } from "../types";

export default function Deliveries() {
    //Controls modal
    const [showForm, setShowForm] = useState(false);

    /*Delivery state variable for newly created delivery*/
    const [delivery, setDelivery] = useState<Delivery>({ 
        app: "", 
        customerNeighborhood: "", 
        notes: "", 
        restaurant: "",
        deliveryTime: "",
        totalPay: 0,
        basePay: 0,
        tipPay: 0,
        mileage: 0
    });
    const [myDeliveries, setMyDeliveries] = useState<Delivery[]>([]);

    /*Error handling*/
    const [error, setError] = useState("");

    const navigate = useNavigate();

    // Call to client to add delivery to db
    const addDelivery = async () => {
        try {
            // If any field is left blank, prompt user to fill in required information
            if (!delivery.app || !delivery.deliveryTime || !delivery.basePay || !delivery.tipPay || !delivery.mileage
                || !delivery.restaurant || !delivery.customerNeighborhood) {
                alert("Please complete all fields before submitting");
                return;
            }

            if (new Date(delivery.deliveryTime) > new Date()) {
                alert("Delivery time cannot be in the future");
                return;
            }

            // parse delivery so pay gets passed as numerical value
            const parsedDelivery = {
                ...delivery,
                basePay: delivery.basePay,
                tipPay: delivery.tipPay
            };

            // Call client, close modal, and go to delivery page.
            const newDelivery = await client.addUserDelivery(parsedDelivery);
            setMyDeliveries(prev => [newDelivery, ...prev]);
            setShowForm(false);
            navigate("/GigBoard/MyDeliveries");
        } catch {
            setError("Add delivery failed");
        }
    }

    if (error.length > 0) {
        return (
            <p>{error}</p>
        );
    }

    return (
        <div id="da-my-deliveries">
            <div id="deliveries-header" className="d-flex align-items-center">
                <h1 className="me-3">Your Deliveries</h1>
                <Button onClick={() => setShowForm(true)} variant="contained" color="primary">
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
                                        <option value=""></option>
                                        <option value="Doordash">Doordash</option>
                                        <option value="UberEats">Uber Eats</option>
                                        <option value="Grubhub">Grubhub</option>
                                        <option value="Instacart">Instacart</option>
                                    </select>
                                </Col>
                            </FormGroup>
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">Time</FormLabel>
                                <Col sm={7}>
                                    <FormControl type="datetime-local"
                                        onChange={(e) => setDelivery({ ...delivery, deliveryTime: e.target.value })}
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
                                        onChange={(e) => setDelivery({...delivery, basePay: parseFloat(e.target.value)})}
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
                                        onChange={(e) => setDelivery({...delivery, tipPay: parseFloat(e.target.value)})}
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
                                        onChange={(e) => setDelivery({ ...delivery, mileage: parseFloat(e.target.value)})}
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
                            <Button onClick={addDelivery} variant="contained" color="primary">
                                Add Delivery
                            </Button>
                        </div>
                    </Modal.Body>
                </Modal>
            </div>
            <MyDeliveries myDeliveries={myDeliveries} setMyDeliveries={setMyDeliveries} />
        </div>
    );
}