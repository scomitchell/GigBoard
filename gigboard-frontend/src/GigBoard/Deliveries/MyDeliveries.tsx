import { useState, useEffect } from "react";
import { FormGroup, FormLabel, FormControl, Modal, Row, Col, Dropdown } from "react-bootstrap";
import { Card, CardContent, Typography, Button } from "@mui/material";
import * as client from "./client";
import type { DeliveryFilters } from "./client";
import '../../index.css';

export default function MyDeliveries({ myDeliveries, setMyDeliveries }: {
    myDeliveries: any[],
    setMyDeliveries: React.Dispatch<React.SetStateAction<any[]>>}) {
    // Control Modal
    const [showForm, setShowForm] = useState(false);

    // User entered filters
    const [totalPay, setTotalPay] = useState<number | null>(null);
    const [basePay, setBasePay] = useState<number | null>(null);
    const [tipPay, setTipPay] = useState<number | null>(null);
    const [mileage, setMileage] = useState<number | null>(null);
    const [neighborhood, setNeighborhood] = useState<string | null>(null);
    const [app, setApp] = useState<string | null>(null);


    // Items for dropdown filters
    const [neighborhoods, setNeighborhoods] = useState<any>([]);
    const [apps, setApps] = useState<any>([]);

    // Reset and error handling
    const [reset, setReset] = useState(false);

    // Delivery to delete
    const [deliveryToDelete, setDeliveryToDelete] = useState(-1);
    const [deliveryToUpdate, setDeliveryToUpdate] = useState<any | null>(null);

    // Initial fetch deliveries
    const fetchDeliveries = async () => {
        // If filters applied, call getFilteredDeliveries, sort by date
        if (totalPay || basePay || tipPay || neighborhood || app || mileage) {
            const filters: DeliveryFilters = {
                totalPay: totalPay,
                basePay: basePay,
                tipPay: tipPay,
                mileage: mileage,
                customerNeighborhood: neighborhood,
                app: app
            }

            const deliveries = await client.getFilteredDeliveries(filters);
            setMyDeliveries(deliveries);
            setShowForm(false);
            return;
        }

        // If no filters retrieve all deliveries, sort by date
        const deliveries = await client.findUserDeliveries();
        setMyDeliveries(deliveries);
    }

    // Deletes delivery from the database
    const deleteDelivery = async (deliveryId: number) => {
        await client.deleteUserDelivery(deliveryId);
        fetchDeliveries();
        setDeliveryToDelete(-1);
    }

    const updateDelivery = async () => {
        await client.updateUserDelivery(deliveryToUpdate);
        fetchDeliveries();
        setDeliveryToUpdate(null);
    }

    // Retrieves list of user neighborhoods for dropdown
    const fetchNeighborhoods = async () => {
        const userNeighborhoods = await client.findUserNeighborhoods();
        setNeighborhoods(userNeighborhoods);
    }


    // Retreieves list of user apps for dropdown
    const fetchApps = async () => {
        const userApps = await client.findUserApps();
        setApps(userApps);
    }

    // Converts datetime to readable format
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

    // Set all filters to null
    const resetFilters = () => {
        setTotalPay(null);
        setBasePay(null);
        setTipPay(null);
        setMileage(null);
        setNeighborhood(null);
        setApp(null);
        setReset(true);
    }

    // Initial fetch
    useEffect(() => {
        fetchDeliveries();
        fetchNeighborhoods();
        fetchApps();
    }, [])


    // If reset intitiated and all cleared, re-fetch deliveries
    useEffect(() => {
        const allCleared =
            totalPay === null &&
            basePay === null &&
            tipPay === null &&
            mileage === null &&
            neighborhood === null &&
            app === null;

        if (reset && allCleared) {
            fetchDeliveries();
            setReset(false); // Reset the flag
        }
    }, [totalPay, basePay, tipPay, neighborhood, app, reset]);

    return (
        <div id="da-my-deliveries" className="mt-3 col-sm-8">
            <Button onClick={() => setShowForm(true)} 
                variant="contained" 
                color="secondary"
                sx={{mr: 1, mb: 2}}>
                Filter Deliveries
            </Button>
            <Button onClick={resetFilters} 
                variant="outlined" 
                color="error"
                className="mui-button"
                sx={{mr: 1, mb: 2}}>
                Reset Filters
            </Button>

            {/*Modal to apply delivery filters*/}
            <Modal show={showForm} onHide={() => setShowForm(false)} centered size="lg">
                <Modal.Header closeButton>
                    <Modal.Title>Filter Deliveries</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <div className="filter-deliveries">
                        <FormGroup as={Row} className="mb-3">
                            <FormLabel column sm={4}>Minimum Total Pay</FormLabel>
                            <Col sm={7}>
                                <FormControl
                                    type="number"
                                    value={totalPay === null ? "" : totalPay}
                                    min="1.00"
                                    step="0.01"
                                    onChange={e => setTotalPay(e.target.value === "" ? null : Number(e.target.value))}
                                />
                            </Col>
                        </FormGroup>
                        <FormGroup as={Row} className="mb-3">
                            <FormLabel column sm={4}>Minimum Base Pay</FormLabel>
                            <Col sm={7}>
                                <FormControl
                                    type="number"
                                    value={basePay === null ? "" : basePay}
                                    min="1.00"
                                    step="0.01"
                                    onChange={e => setBasePay(e.target.value === "" ? null : Number(e.target.value))}
                                />
                            </Col>
                        </FormGroup>
                        <FormGroup as={Row} className="mb-3">
                            <FormLabel column sm={4}>Minimum Tip Pay</FormLabel>
                            <Col sm={7}>
                                <FormControl
                                    type="number"
                                    value={tipPay === null ? "" : tipPay}
                                    min="1.00"
                                    step="0.01"
                                    onChange={e => setTipPay(e.target.value === "" ? null : Number(e.target.value))}
                                />
                            </Col>
                        </FormGroup>
                        <FormGroup as={Row} className="mb-3">
                            <FormLabel column sm={4}>Minimum Mileage</FormLabel>
                            <Col sm={7}>
                                <FormControl
                                    type="number"
                                    value={mileage === null ? "" : mileage}
                                    min="1.00"
                                    step="0.01"
                                    onChange={e => setMileage(e.target.value === "" ? null : Number(e.target.value))}
                                />
                            </Col>
                        </FormGroup>
                        <FormGroup as={Row} className="d-flex align-items-center mb-2">
                            <FormLabel column sm={4}>Neighborhood</FormLabel>
                            <Col sm={7}>
                                <select onChange={(e) => setNeighborhood(e.target.value)}
                                    className="form-control mb-2" id="da-app">
                                    <option value=""></option>
                                    {neighborhoods.map((neighborhood: any) => 
                                        <option value={neighborhood}>{neighborhood}</option>
                                    )}
                                </select>
                            </Col>
                        </FormGroup>
                        <FormGroup as={Row} className="d-flex align-items-center mb-2">
                            <FormLabel column sm={4}>Delivery App</FormLabel>
                            <Col sm={7}>
                                <select onChange={(e) => setApp(e.target.value)}
                                    className="form-control mb-2" id="da-app">
                                    <option value=""></option>
                                    {apps.map((app: any) =>
                                        <option value={app}>{app}</option>
                                    )}
                                </select>
                            </Col>
                        </FormGroup>
                        <Button onClick={fetchDeliveries} variant="contained" color="primary">
                            Apply Filters
                        </Button>
                    </div>
                </Modal.Body>
            </Modal>

            {/*Render individual delivery details on cards*/}
            <Row>
                {myDeliveries.map((delivery: any) => 
                    <Col sm={6} key={delivery.id}>
                        <Card sx={{
                                mb: 3,
                                textAlign: "start",
                                borderRadius: 3,
                                boxShadow: 3,
                                position: "relative",
                                transition: "0.3s",
                                "&:hover": { boxShadow: 6 },
                        }}>
                            <CardContent sx={{ p: 2 }}>
                                {/*Fix dropdown to top right corner of card*/}
                                <div style={{ position: 'absolute', top: '0.5rem', right: '0.5rem' }}>
                                    {/*Dropdown menu for delete delivery*/}
                                    <Dropdown>
                                        <Dropdown.Toggle variant="secondary" size="sm">
                                            &#x22EE;
                                        </Dropdown.Toggle>

                                        <Dropdown.Menu>
                                            <Dropdown.Item onClick={() => setDeliveryToDelete(delivery.id)} 
                                                className="text-danger">
                                                Delete Delivery
                                            </Dropdown.Item>
                                            <Dropdown.Item onClick={() => setDeliveryToUpdate(delivery)}
                                                className="text-warning">
                                                Update Delivery
                                            </Dropdown.Item>
                                        </Dropdown.Menu>
                                    </Dropdown>
                                </div>

                                <Typography variant="h6" fontWeight="bold">
                                    {formatTime(delivery.deliveryTime)} {" "} <br />
                                    Total Pay: ${delivery.totalPay.toFixed(2)}
                                </Typography>
                                <Typography variant="body1" sx={{ mt: 1 }}>
                                    <strong>Base Pay:</strong> ${delivery.basePay.toFixed(2)} {" "} <br />
                                    <strong>Tip Pay:</strong> ${delivery.tipPay.toFixed(2)} {" "} <br />
                                    <strong>Mileage:</strong> {delivery.mileage.toFixed(2)} {" miles"} <br />
                                    <strong>App:</strong> {delivery.app} {" "} <br />
                                    <strong>Restaurant:</strong> {delivery.restaurant} {" "} <br />
                                    <strong>Customer Neighborhood:</strong> {delivery.customerNeighborhood} {" "} <br />
                                    <strong>Notes:</strong> {delivery.notes} {" "}
                                </Typography>

                                {/*Modal to delete delivery*/}
                                <>
                                    <Modal show={deliveryToDelete !== -1} 
                                        onHide={() => setDeliveryToDelete(-1)} centered size="lg">
                                        <Modal.Header closeButton>
                                            <Modal.Title>Delete Delivery</Modal.Title>
                                        </Modal.Header>
                                        <Modal.Body>Are you sure you want to delete this delivery?</Modal.Body>
                                        <Modal.Footer>
                                            <Button variant="contained" 
                                                color="secondary" 
                                                sx={{ mr: 2}}
                                                onClick={() => setDeliveryToDelete(-1)}>
                                                Cancel
                                            </Button>
                                            <Button
                                                variant="outlined"
                                                color="error"
                                                onClick={() => {
                                                    deleteDelivery(deliveryToDelete);
                                                }}
                                            >
                                                Delete
                                            </Button>
                                        </Modal.Footer>
                                    </Modal>
                                </>
                            </CardContent>
                        </Card>
                    </Col>
                )}

                {/*Modal to confirm update delivery*/}
                <>
                    <Modal show={deliveryToUpdate !== null} 
                        onHide={() => setDeliveryToUpdate(null)} centered size="lg">
                        <Modal.Header closeButton>
                            <Modal.Title>Update Delivery</Modal.Title>
                        </Modal.Header>
                        <Modal.Body>
                            {deliveryToUpdate &&
                            <div className="update-delivery-details">
                                <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                    <FormLabel column sm={4} className="me-3">App</FormLabel>
                                    <Col sm={7}>
                                        <select onChange={(e) => setDeliveryToUpdate({...deliveryToUpdate, app: e.target.value})}
                                            className="form-control mb-2" 
                                            id="da-app"
                                            defaultValue={deliveryToUpdate.app}>
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
                                                defaultValue={deliveryToUpdate.deliveryTime}
                                                onChange={(e) => setDeliveryToUpdate({ ...deliveryToUpdate, deliveryTime: e.target.value })}
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
                                                defaultValue={deliveryToUpdate.basePay}
                                                onChange={(e) => setDeliveryToUpdate({...deliveryToUpdate, basePay: e.target.value})}
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
                                                defaultValue={deliveryToUpdate.tipPay}
                                                onChange={(e) => setDeliveryToUpdate({...deliveryToUpdate, tipPay: e.target.value})}
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
                                                defaultValue={deliveryToUpdate.mileage}
                                                onChange={(e) => setDeliveryToUpdate({ ...deliveryToUpdate, mileage: e.target.value })}
                                            />
                                        </Col>
                                    </FormGroup>
                                    <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                        <FormLabel column sm={4} className="me-3">Restaurant</FormLabel>
                                        <Col sm={7}>
                                            <FormControl
                                                type="text"
                                                placeholder="Restaurant"
                                                defaultValue={deliveryToUpdate.restaurant}
                                                onChange={(e) => setDeliveryToUpdate({...deliveryToUpdate, restaurant: e.target.value})}
                                            />
                                        </Col>
                                    </FormGroup>
                                    <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                        <FormLabel column sm={4} className="me-3">Customer Neighborhood</FormLabel>
                                        <Col sm={7}>
                                            <FormControl
                                                type="text"
                                                placeholder="Customer Neighborhood"
                                                defaultValue={deliveryToUpdate.customerNeighborhood}
                                                onChange={(e) => setDeliveryToUpdate({...deliveryToUpdate, customerNeighborhood: e.target.value})}
                                            />
                                        </Col>
                                    </FormGroup>
                                    <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                        <FormLabel column sm={4} className="me-3">Notes</FormLabel>
                                        <Col sm={7}>
                                            <FormControl
                                                type="text"
                                                placeholder="Notes"
                                                defaultValue={deliveryToUpdate.notes}
                                                onChange={(e) => setDeliveryToUpdate({...deliveryToUpdate, notes: e.target.value})}
                                            />
                                        </Col>
                                    </FormGroup>
                                    <Button onClick={updateDelivery} variant="contained" color="primary">
                                        Update Delivery
                                    </Button>
                                </div>
                                }
                        </Modal.Body>
                    </Modal>
                </>
            </Row>
        </div>
    );
}