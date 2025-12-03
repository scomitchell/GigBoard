import { Modal, FormGroup, FormLabel, FormControl, Row, Col } from "react-bootstrap";
import { Button } from "@mui/material";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import MyShifts from "./MyShifts";
import * as client from "./client";

export default function Shifts() {
    // Controls Modal
    const [showForm, setShowForm] = useState(false);

    // Shift state variable for creating new shifts
    const [shift, setShift] = useState<any>({});
    const [myShifts, setMyShifts] = useState<any[]>([]);

    // Error handling
    const [error, setError] = useState("");

    const navigate = useNavigate();

    // Call to client to add shift to db
    const addShift = async () => {
        try {
            if (!shift.startTime || !shift.endTime || !shift.app) {
                alert("Please fill out all fields before submitting");
                return;
            }

            if (shift.startTime >= shift.endTime) {
                alert("Shift start time must come after end time");
                return;
            }

            if (new Date(shift.startTime) > new Date()) {
                alert("Shift start time cannot be in the future");
                return;
            }

            const newShift = await client.addShift(shift);
            setMyShifts(prev => [newShift, ...prev]);
            setShowForm(false);
            navigate("/GigBoard/Shifts");
        } catch (err: any) {
            setError("Add shift failed");
        }
    }

    if (error.length > 0) {
        return (
            <p>{error}</p>
        );
    }

    return (
        <div id="da-shifts">
            <div id="da-shifts-header" className="d-flex align-items-center">
                <h1 className="me-3">Log Shifts</h1>
                <Button onClick={() => setShowForm(true)} variant="contained" color="primary">
                    Add Shift
                </Button>

                {/*Modal form for adding a new shift*/}
                <Modal show={showForm} onHide={() => setShowForm(false)} centered size="lg">
                    <Modal.Header closeButton>
                        <Modal.Title>Add New Shift</Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        <div id="add-shift-details">
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">Start Time</FormLabel>
                                <Col sm={7}>
                                    <FormControl
                                        type="datetime-local"
                                        onChange={(e) => setShift({ ...shift, startTime: e.target.value })}
                                    />
                                </Col>
                            </FormGroup>
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">End Time</FormLabel>
                                <Col sm={7}>
                                    <FormControl
                                        type="datetime-local"
                                        onChange={(e) => setShift({ ...shift, endTime: e.target.value })}
                                    />
                                </Col>
                            </FormGroup>
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">App</FormLabel>
                                <Col sm={7}>
                                    <select onChange={(e) => setShift({ ...shift, app: e.target.value })}
                                        className="form-control mb-2" id="da-app">
                                        <option value=""></option>
                                        <option value="Doordash">Doordash</option>
                                        <option value="UberEats">Uber Eats</option>
                                        <option value="Grubhub">Grubhub</option>
                                        <option value="Instacart">Instacart</option>
                                    </select>
                                </Col>
                            </FormGroup>
                            <Button onClick={addShift} variant="contained" color="primary">
                                Add Shift
                            </Button>
                        </div>
                    </Modal.Body>
                </Modal>
            </div>
            <MyShifts myShifts={myShifts} setMyShifts={setMyShifts} />
        </div>
    );
}