import { Modal, FormGroup, FormControl, FormLabel, Row, Col, Dropdown } from "react-bootstrap";
import { Card, CardContent, Typography, Button } from "@mui/material";
import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import * as client from "./client";
import type { ShiftFilters } from "./client";
import type { FullShift } from "../types";

export default function MyShifts({ myShifts, setMyShifts }: {
    myShifts: FullShift[],
    setMyShifts: React.Dispatch<React.SetStateAction<FullShift[]>>}) {
    // Modal control state
    const [showForm, setShowForm] = useState(false);

    // User entered Filters
    const [startTime, setStartTime] = useState<string | null>(null);
    const [endTime, setEndTime] = useState<string | null>(null);
    const [app, setApp] = useState<string | null>(null);
    
    // Select menu options
    const [userApps, setUserApps] = useState<string[]>([]);

    // Control reset
    const [reset, setReset] = useState(false);

    // Delete
    const [shiftToDelete, setShiftToDelete] = useState(-1);
    const [shiftToUpdate, setShiftToUpdate] = useState<FullShift | null>(null);
    
    // Fetch all or fitered shifts
    const fetchShifts = useCallback(async () => {
        // If any filters applied, call filteredShifts
        if (startTime || endTime || app) {
            const filters: ShiftFilters = {
                startTime: startTime,
                endTime: endTime,
                app: app
            }

            const shifts = await client.getFilteredShifts(filters);

            setMyShifts(shifts);
            setShowForm(false);
            return;
        }

        const shifts = await client.findUserShifts();
        setMyShifts(shifts);
        setShowForm(false);
        return;
    }, [app, endTime, setMyShifts, startTime])

    // Delete shift from db
    const deleteShift = async (shiftId: number) => {
        await client.deleteUserShift(shiftId);
        fetchShifts();
        setShiftToDelete(-1);
    }

    // Update shift in db
    const updateShift = async () => {
        if (!shiftToUpdate) return;
        
        await client.updateUserShift(shiftToUpdate);
        fetchShifts();
        setShiftToUpdate(null);
    }

    // Fetch list of user used apps
    const fetchApps = useCallback(async () => {
        const apps = await client.getUserApps();
        setUserApps(apps);
    }, []);

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

    // Clear all filters
    const resetFilters = () => {
        setStartTime(null);
        setEndTime(null);
        setApp(null);
        setReset(true);
    }

    useEffect(() => {
        fetchShifts();
        fetchApps();
    }, [fetchApps, fetchShifts])

    // useEffect for reset filters
    useEffect(() => {
        const allCleared =
            startTime === null &&
            endTime === null &&
            app === null;

        if (reset && allCleared) {
            fetchShifts();
            setReset(false);
        }
    }, [startTime, endTime, app, reset, fetchShifts])

    return (
        <div id="da-my-shifts" className="mt-3 col-sm-8">
            <Button onClick={() => setShowForm(true)}
                variant="contained"
                color="secondary"
                sx={{mr: 1, mb: 2}}>
                Filter Shifts
            </Button>
            <Button onClick={resetFilters}
                variant="outlined"
                color="error"
                sx={{mr: 1, mb: 2}}>
                Reset filters
            </Button>
            
            {/*Modal to filter shifts*/}
            <Modal show={showForm} onHide={() => setShowForm(false)} centered size="lg">
                <Modal.Header closeButton>
                    <Modal.Title>Filter Shifts</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <div className="filter-shifts">
                        <FormGroup as={Row} className="mb-3">
                            <FormLabel column sm={4}>Starts after</FormLabel>
                                <Col sm={7}>
                                    <FormControl
                                        type="datetime-local"
                                        value={startTime === null ? "" : startTime}
                                        onChange={(e) => setStartTime(e.target.value === "" ? null : e.target.value)}
                                     />
                                </Col>
                        </FormGroup>
                        <FormGroup as={Row} className="mb-3">
                            <FormLabel column sm={4}>Ends before</FormLabel>
                            <Col sm={7}>
                                <FormControl
                                    type="datetime-local"
                                    value={endTime === null ? "" : endTime}
                                    onChange={(e) => setEndTime(e.target.value === "" ? null : e.target.value)}
                                />
                            </Col>
                        </FormGroup>
                        <FormGroup as={Row} className="d-flex align-items-center mb-2">
                            <FormLabel column sm={4}>App</FormLabel>
                            <Col sm={7}>
                                <select onChange={(e) => setApp(e.target.value)}
                                    className="form-control mb-2" id="da-app">
                                    <option value=""></option>
                                    {userApps.map((app: string) =>
                                        <option value={app} key={app}>{app}</option>
                                    )}
                                </select>
                            </Col>
                        </FormGroup>
                        <Button onClick={fetchShifts} variant="contained" color="primary">
                            Apply Filters
                        </Button>
                    </div>
                </Modal.Body>
            </Modal>
            
            {/*Display Shift details on cards*/}
            <Row>
                {myShifts.map((shift: FullShift) =>
                    <Col sm={6} key={shift.id}>
                        <Card sx={{
                            mb: 3,
                            textAlign: "start",
                            borderRadius: 3,
                            boxShadow: 3,
                            position: "relative",
                            transition: "0.3s",
                            minHeight: 150,
                            "&:hover": { boxShadow: 6, transform: "translateY(-3px)" },
                        }}>
                            <CardContent sx={{ p: 2 }}>
                                {/*Fix dropdown menu to top right corner of card*/}
                                <div style={{ position: 'absolute', top: '0.5rem', right: '0.5rem'}}>
                                    {/*Dropdown menu for delete Shift*/}
                                    <Dropdown>
                                        <Dropdown.Toggle variant="secondary" size="sm">
                                            &#x22EE;
                                        </Dropdown.Toggle>

                                        <Dropdown.Menu>
                                            <Dropdown.Item onClick={(e) => {
                                                e.preventDefault();
                                                setShiftToDelete(shift.id);
                                                }} 
                                                className="text-danger">
                                                Delete Shift
                                            </Dropdown.Item>
                                            <Dropdown.Item onClick={(e) => {
                                                e.preventDefault();
                                                setShiftToUpdate(shift);
                                            }}
                                                className="text-warning">
                                                Update Shift
                                            </Dropdown.Item>
                                        </Dropdown.Menu>
                                    </Dropdown>
                                </div>

                                {/* Only this part is clickable */}
                                <Link to={`/GigBoard/Shifts/${shift.id}`} className="text-decoration-none text-dark">
                                    <Typography variant="h6" fontWeight="bold">
                                        {formatTime(shift.startTime)} {"  -"}
                                    </Typography>
                                    <Typography variant="h6" fontWeight="bold">
                                        {formatTime(shift.endTime)}
                                    </Typography>
                                    <Typography variant="body1" sx={{ mt: 1}}>
                                        <strong>App:</strong> {shift.app} <br />
                                    </Typography>
                                </Link>

                                {/*Modal to confirm delete shift*/}
                                <>
                                    <Modal show={shiftToDelete !== -1} onHide={() => setShiftToDelete(-1)} centered size="lg">
                                        <Modal.Header closeButton>
                                            <Modal.Title>Confirm Deletion</Modal.Title>
                                        </Modal.Header>
                                        <Modal.Body>Are you sure you want to delete this shift?</Modal.Body>
                                        <Modal.Footer>
                                            <Button variant="contained" 
                                                color="secondary" 
                                                sx={{mr: 2}}
                                                onClick={() => setShiftToDelete(-1)}>
                                                Cancel
                                            </Button>
                                            <Button variant="outlined"
                                                color="error"
                                                onClick={() => {
                                                    deleteShift(shiftToDelete);
                                                }}
                                            >
                                                Delete
                                            </Button>
                                        </Modal.Footer>
                                    </Modal>

                                    <Modal show={shiftToUpdate !== null} onHide={() => setShiftToUpdate(null)} centered size="lg">
                                        <Modal.Header closeButton>
                                            <Modal.Title>Update Shift</Modal.Title>
                                        </Modal.Header>
                                        <Modal.Body>
                                            {shiftToUpdate &&
                                            <div id="add-shift-details">
                                                <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                                    <FormLabel column sm={4} className="me-3">Start Time</FormLabel>
                                                    <Col sm={7}>
                                                        <FormControl
                                                            type="datetime-local"
                                                            defaultValue={shiftToUpdate.startTime}
                                                            onChange={(e) => setShiftToUpdate({ ...shiftToUpdate, startTime: e.target.value })}
                                                        />
                                                    </Col>
                                                </FormGroup>
                                                <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                                    <FormLabel column sm={4} className="me-3">End Time</FormLabel>
                                                    <Col sm={7}>
                                                        <FormControl
                                                            type="datetime-local"
                                                            defaultValue={shiftToUpdate.endTime}
                                                            onChange={(e) => setShiftToUpdate({ ...shiftToUpdate, endTime: e.target.value })}
                                                        />
                                                    </Col>
                                                </FormGroup>
                                                <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                                    <FormLabel column sm={4} className="me-3">App</FormLabel>
                                                    <Col sm={7}>
                                                        <select onChange={(e) => setShiftToUpdate({ ...shiftToUpdate, app: e.target.value })}
                                                            className="form-control mb-2" 
                                                            defaultValue={shiftToUpdate.app}
                                                            id="da-app">
                                                            <option value=""></option>
                                                            <option value="Doordash">Doordash</option>
                                                            <option value="UberEats">Uber Eats</option>
                                                            <option value="Grubhub">Grubhub</option>
                                                            <option value="Instacart">Instacart</option>
                                                        </select>
                                                    </Col>
                                                </FormGroup>
                                                <Button onClick={updateShift} variant="contained" color="primary">
                                                    Update Shift
                                                </Button>
                                            </div>
                                            }
                                        </Modal.Body>
                                    </Modal>
                                </>
                            </CardContent>
                        </Card>
                    </Col>
                )}
            </Row>
        </div>
    );
}