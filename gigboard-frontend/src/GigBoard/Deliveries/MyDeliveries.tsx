import { useState, useEffect, useCallback } from "react";
import {
  FormGroup,
  FormLabel,
  FormControl,
  Modal,
  Row,
  Col,
  Dropdown,
} from "react-bootstrap";
import { Card, CardContent, Typography, Button } from "@mui/material";
import * as client from "./client";
import type { DeliveryFilters } from "./client";
import "../../index.css";
import type { Delivery } from "../types";

export default function MyDeliveries({
  myDeliveries,
  setMyDeliveries,
}: {
  myDeliveries: Delivery[];
  setMyDeliveries: React.Dispatch<React.SetStateAction<Delivery[]>>;
}) {
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
  const [neighborhoods, setNeighborhoods] = useState<string[]>([]);
  const [apps, setApps] = useState<string[]>([]);

  // Reset and error handling
  const [reset, setReset] = useState(false);

  // Delivery to delete
  const [deliveryToDelete, setDeliveryToDelete] = useState(-1);
  const [deliveryToUpdate, setDeliveryToUpdate] = useState<Delivery | null>(
    null,
  );

  // Initial fetch deliveries
  const fetchDeliveries = useCallback(async () => {
    const deliveries = await client.findUserDeliveries();
    setMyDeliveries(deliveries);
  }, [setMyDeliveries]);

  const applyFilters = async () => {
    const filters: DeliveryFilters = {
      totalPay,
      basePay,
      tipPay,
      mileage,
      customerNeighborhood: neighborhood,
      app,
    };

    const deliveries = await client.getFilteredDeliveries(filters);
    setMyDeliveries(deliveries);
    setShowForm(false);
  };

  // Deletes delivery from the database
  const deleteDelivery = async (deliveryId: number) => {
    await client.deleteUserDelivery(deliveryId);
    fetchDeliveries();
    setDeliveryToDelete(-1);
  };

  const updateDelivery = async () => {
    if (!deliveryToUpdate) return;
    await client.updateUserDelivery(deliveryToUpdate);
    fetchDeliveries();
    setDeliveryToUpdate(null);
  };

  // Retrieves list of user neighborhoods for dropdown
  const fetchNeighborhoods = useCallback(async () => {
    const userNeighborhoods = await client.findUserNeighborhoods();
    setNeighborhoods(userNeighborhoods);
  }, []);

  // Retreieves list of user apps for dropdown
  const fetchApps = useCallback(async () => {
    const userApps = await client.findUserApps();
    setApps(userApps);
  }, []);

  // Converts datetime to readable format
  const formatTime = (date: string) => {
    const newDate = new Date(date);
    const options: Intl.DateTimeFormatOptions = {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
      hour12: true,
    };
    return newDate.toLocaleString(undefined, options);
  };

  // Set all filters to null
  const resetFilters = () => {
    setTotalPay(null);
    setBasePay(null);
    setTipPay(null);
    setMileage(null);
    setNeighborhood(null);
    setApp(null);
    setReset(true);
  };

  // Initial fetch
  useEffect(() => {
    fetchDeliveries();
    fetchNeighborhoods();
    fetchApps();
  }, [fetchApps, fetchDeliveries, fetchNeighborhoods]);

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
  }, [
    totalPay,
    basePay,
    tipPay,
    neighborhood,
    app,
    reset,
    mileage,
    fetchDeliveries,
  ]);

  return (
    <div id="da-my-deliveries" className="mt-3 col-sm-8">
      <Button
        onClick={() => setShowForm(true)}
        variant="outlined"
        sx={{
          mr: 1,
          mb: 2,
          color: "#374151",
          borderColor: "#D1D5DB",
          fontWeight: 600,
          "&:hover": { bgcolor: "#F9FAFB", borderColor: "#D1D5DB" },
        }}
      >
        Filter Deliveries
      </Button>
      <Button
        onClick={resetFilters}
        variant="text"
        sx={{
          mr: 1,
          mb: 2,
          color: "#EF4444",
          fontWeight: 600,
          "&:hover": { bgcolor: "#FEF2F2" },
        }}
      >
        Reset Filters
      </Button>

      {/*Modal to apply delivery filters*/}
      <Modal
        show={showForm}
        onHide={() => setShowForm(false)}
        centered
        size="lg"
      >
        <Modal.Header closeButton>
          <Modal.Title>Filter Deliveries</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <div className="filter-deliveries">
            <FormGroup as={Row} className="mb-3">
              <FormLabel column sm={4}>
                Minimum Total Pay
              </FormLabel>
              <Col sm={7}>
                <FormControl
                  type="number"
                  value={totalPay === null ? "" : totalPay}
                  min="1.00"
                  step="0.01"
                  onChange={(e) =>
                    setTotalPay(
                      e.target.value === "" ? null : Number(e.target.value),
                    )
                  }
                />
              </Col>
            </FormGroup>
            <FormGroup as={Row} className="mb-3">
              <FormLabel column sm={4}>
                Minimum Base Pay
              </FormLabel>
              <Col sm={7}>
                <FormControl
                  type="number"
                  value={basePay === null ? "" : basePay}
                  min="1.00"
                  step="0.01"
                  onChange={(e) =>
                    setBasePay(
                      e.target.value === "" ? null : Number(e.target.value),
                    )
                  }
                />
              </Col>
            </FormGroup>
            <FormGroup as={Row} className="mb-3">
              <FormLabel column sm={4}>
                Minimum Tip Pay
              </FormLabel>
              <Col sm={7}>
                <FormControl
                  type="number"
                  value={tipPay === null ? "" : tipPay}
                  min="1.00"
                  step="0.01"
                  onChange={(e) =>
                    setTipPay(
                      e.target.value === "" ? null : Number(e.target.value),
                    )
                  }
                />
              </Col>
            </FormGroup>
            <FormGroup as={Row} className="mb-3">
              <FormLabel column sm={4}>
                Minimum Mileage
              </FormLabel>
              <Col sm={7}>
                <FormControl
                  type="number"
                  value={mileage === null ? "" : mileage}
                  min="1.00"
                  step="0.01"
                  onChange={(e) =>
                    setMileage(
                      e.target.value === "" ? null : Number(e.target.value),
                    )
                  }
                />
              </Col>
            </FormGroup>
            <FormGroup as={Row} className="d-flex align-items-center mb-2">
              <FormLabel column sm={4}>
                Neighborhood
              </FormLabel>
              <Col sm={7}>
                <select
                  onChange={(e) => setNeighborhood(e.target.value)}
                  className="form-control mb-2"
                  id="da-app"
                >
                  <option value=""></option>
                  {neighborhoods.map((neighborhood: string) => (
                    <option value={neighborhood} key={neighborhood}>
                      {neighborhood}
                    </option>
                  ))}
                </select>
              </Col>
            </FormGroup>
            <FormGroup as={Row} className="d-flex align-items-center mb-2">
              <FormLabel column sm={4}>
                Delivery App
              </FormLabel>
              <Col sm={7}>
                <select
                  onChange={(e) => setApp(e.target.value)}
                  className="form-control mb-2"
                  id="da-app"
                >
                  <option value=""></option>
                  {apps.map((app: string) => (
                    <option value={app} key={app}>
                      {app}
                    </option>
                  ))}
                </select>
              </Col>
            </FormGroup>
            <Button
              onClick={applyFilters}
              variant="contained"
              sx={{
                color: "#FFFFFF",
                backgroundColor: "#1E293B",
                fontWeight: 600,
                "&:hover": { bgcolor: "#0F172A" },
              }}
            >
              Apply Filters
            </Button>
          </div>
        </Modal.Body>
      </Modal>

      {/*Render individual delivery details on cards*/}
      <Row>
        {myDeliveries.map((delivery: Delivery) => (
          <Col md={6} xl={4} key={delivery.id}>
            {" "}
            <Card
              sx={{
                mb: 4,
                textAlign: "start",
                borderRadius: 3,
                border: "1px solid #E5E7EB",
                boxShadow: "0 1px 3px 0 rgba(0, 0, 0, 0.1)",
                position: "relative",
                transition: "transform 0.2s, box-shadow 0.2s",
                "&:hover": {
                  boxShadow: "0 10px 15px -3px rgba(0, 0, 0, 0.1)",
                  transform: "translateY(-2px)",
                },
              }}
            >
              <CardContent sx={{ p: 3 }}>
                {/* Top Controls & Header */}
                <div
                  style={{ position: "absolute", top: "1rem", right: "1rem" }}
                >
                  <Dropdown>
                    <Dropdown.Toggle
                      variant="light"
                      size="sm"
                      style={{
                        backgroundColor: "transparent",
                        border: "none",
                        color: "#6B7280",
                      }}
                    >
                      &#x22EE;
                    </Dropdown.Toggle>
                    <Dropdown.Menu>
                      <Dropdown.Item
                        onClick={() => setDeliveryToDelete(Number(delivery.id))}
                        style={{ color: "#EF4444" }}
                      >
                        Delete Delivery
                      </Dropdown.Item>
                      <Dropdown.Item
                        onClick={() => setDeliveryToUpdate(delivery)}
                        style={{ color: "#1E293B" }}
                      >
                        Update Delivery
                      </Dropdown.Item>
                    </Dropdown.Menu>
                  </Dropdown>
                </div>

                <Typography
                  variant="overline"
                  sx={{
                    color: "#6366F1",
                    fontWeight: 700,
                    letterSpacing: "0.05em",
                  }}
                >
                  {delivery.app}
                </Typography>
                <Typography
                  variant="body2"
                  color="text.secondary"
                  sx={{ mb: 2 }}
                >
                  {formatTime(delivery.deliveryTime ?? "")}
                </Typography>

                {/* Total Pay */}
                <Typography
                  variant="h3"
                  fontWeight="bold"
                  sx={{ color: "#111827", mb: 3 }}
                >
                  ${(delivery.totalPay ?? 0).toFixed(2)}
                </Typography>

                {/* Stat Grid */}
                <div
                  style={{
                    display: "grid",
                    gridTemplateColumns: "repeat(3, 1fr)",
                    gap: "1rem",
                    marginBottom: "1.5rem",
                    paddingBottom: "1.5rem",
                    borderBottom: "1px solid #E5E7EB",
                  }}
                >
                  <div>
                    <Typography
                      variant="caption"
                      sx={{
                        color: "#6B7280",
                        fontWeight: 600,
                        textTransform: "uppercase",
                      }}
                    >
                      Base
                    </Typography>
                    <Typography
                      variant="body1"
                      fontWeight="600"
                      sx={{ color: "#111827" }}
                    >
                      ${(delivery.basePay ?? 0).toFixed(2)}
                    </Typography>
                  </div>
                  <div>
                    <Typography
                      variant="caption"
                      sx={{
                        color: "#6B7280",
                        fontWeight: 600,
                        textTransform: "uppercase",
                      }}
                    >
                      Tip
                    </Typography>
                    <Typography
                      variant="body1"
                      fontWeight="600"
                      sx={{ color: "#10B981" }}
                    >
                      {" "}
                      ${(delivery.tipPay ?? 0).toFixed(2)}
                    </Typography>
                  </div>
                  <div>
                    <Typography
                      variant="caption"
                      sx={{
                        color: "#6B7280",
                        fontWeight: 600,
                        textTransform: "uppercase",
                      }}
                    >
                      Dist
                    </Typography>
                    <Typography
                      variant="body1"
                      fontWeight="600"
                      sx={{ color: "#111827" }}
                    >
                      {(delivery.mileage ?? 0).toFixed(1)} mi
                    </Typography>
                  </div>
                </div>

                {/* Location Details */}
                <div
                  style={{
                    display: "flex",
                    flexDirection: "column",
                    gap: "0.5rem",
                  }}
                >
                  <div style={{ display: "flex", gap: "0.5rem" }}>
                    <Typography variant="body2" sx={{ color: "#374151" }}>
                      <strong style={{ color: "#111827" }}>From:</strong>{" "}
                      {delivery.restaurant}
                    </Typography>
                  </div>
                  <div style={{ display: "flex", gap: "0.5rem" }}>
                    <Typography variant="body2" sx={{ color: "#374151" }}>
                      <strong style={{ color: "#111827" }}>To:</strong>{" "}
                      {delivery.customerNeighborhood}
                    </Typography>
                  </div>
                </div>

                {/* Notes */}
                {delivery.notes && (
                  <div
                    style={{
                      marginTop: "1.5rem",
                      padding: "0.75rem 1rem",
                      backgroundColor: "#F9FAFB",
                      borderRadius: "8px",
                      borderLeft: "3px solid #D1D5DB",
                    }}
                  >
                    <Typography
                      variant="body2"
                      sx={{ color: "#4B5563", fontStyle: "italic" }}
                    >
                      "{delivery.notes}"
                    </Typography>
                  </div>
                )}
              </CardContent>
            </Card>
          </Col>
        ))}
      </Row>

      {/*Modal to confirm update delivery*/}
      <Modal
        show={deliveryToDelete !== -1}
        onHide={() => setDeliveryToDelete(-1)}
        centered
        size="lg"
      >
        <Modal.Header closeButton>
          <Modal.Title>Delete Delivery</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          Are you sure you want to delete this delivery? This action cannot be
          undone.
        </Modal.Body>
        <Modal.Footer>
          <Button
            variant="contained"
            color="secondary"
            sx={{ mr: 2, backgroundColor: "#6B7280" }}
            onClick={() => setDeliveryToDelete(-1)}
            disableElevation
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            color="error"
            onClick={() => deleteDelivery(deliveryToDelete)}
            disableElevation
          >
            Delete
          </Button>
        </Modal.Footer>
      </Modal>

      {/* --- Global Update Modal --- */}
      <Modal
        show={deliveryToUpdate !== null}
        onHide={() => setDeliveryToUpdate(null)}
        centered
        size="lg"
      >
        <Modal.Header closeButton>
          <Modal.Title>Update Delivery</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {deliveryToUpdate && (
            <div className="update-delivery-details">
              <FormGroup as={Row} className="d-flex align-items-center mb-3">
                <FormLabel
                  column
                  sm={4}
                  className="me-3"
                  style={{ fontWeight: 500 }}
                >
                  App
                </FormLabel>
                <Col sm={7}>
                  <select
                    onChange={(e) =>
                      setDeliveryToUpdate({
                        ...deliveryToUpdate,
                        app: e.target.value,
                      })
                    }
                    className="form-control"
                    id="da-app"
                    defaultValue={deliveryToUpdate.app}
                  >
                    <option value=""></option>
                    <option value="Doordash">Doordash</option>
                    <option value="UberEats">Uber Eats</option>
                    <option value="Grubhub">Grubhub</option>
                    <option value="Instacart">Instacart</option>
                  </select>
                </Col>
              </FormGroup>

              <FormGroup as={Row} className="d-flex align-items-center mb-3">
                <FormLabel
                  column
                  sm={4}
                  className="me-3"
                  style={{ fontWeight: 500 }}
                >
                  Time
                </FormLabel>
                <Col sm={7}>
                  <FormControl
                    type="datetime-local"
                    defaultValue={deliveryToUpdate.deliveryTime}
                    onChange={(e) =>
                      setDeliveryToUpdate({
                        ...deliveryToUpdate,
                        deliveryTime: e.target.value,
                      })
                    }
                  />
                </Col>
              </FormGroup>

              <FormGroup as={Row} className="d-flex align-items-center mb-3">
                <FormLabel
                  column
                  sm={4}
                  className="me-3"
                  style={{ fontWeight: 500 }}
                >
                  Base Pay
                </FormLabel>
                <Col sm={7}>
                  <FormControl
                    type="number"
                    step="0.01"
                    min="1.00"
                    placeholder="Base Pay"
                    defaultValue={deliveryToUpdate.basePay}
                    onChange={(e) =>
                      setDeliveryToUpdate({
                        ...deliveryToUpdate,
                        basePay: parseFloat(e.target.value),
                      })
                    }
                  />
                </Col>
              </FormGroup>

              <FormGroup as={Row} className="d-flex align-items-center mb-3">
                <FormLabel
                  column
                  sm={4}
                  className="me-3"
                  style={{ fontWeight: 500 }}
                >
                  Tip Pay
                </FormLabel>
                <Col sm={7}>
                  <FormControl
                    type="number"
                    step="0.01"
                    min="1.00"
                    placeholder="Tip Pay"
                    defaultValue={deliveryToUpdate.tipPay}
                    onChange={(e) =>
                      setDeliveryToUpdate({
                        ...deliveryToUpdate,
                        tipPay: parseFloat(e.target.value),
                      })
                    }
                  />
                </Col>
              </FormGroup>

              <FormGroup as={Row} className="d-flex align-items-center mb-3">
                <FormLabel
                  column
                  sm={4}
                  className="me-3"
                  style={{ fontWeight: 500 }}
                >
                  Mileage
                </FormLabel>
                <Col sm={7}>
                  <FormControl
                    type="number"
                    step="0.01"
                    min="0.01"
                    placeholder="Mileage"
                    defaultValue={deliveryToUpdate.mileage}
                    onChange={(e) =>
                      setDeliveryToUpdate({
                        ...deliveryToUpdate,
                        mileage: parseFloat(e.target.value),
                      })
                    }
                  />
                </Col>
              </FormGroup>

              <FormGroup as={Row} className="d-flex align-items-center mb-3">
                <FormLabel
                  column
                  sm={4}
                  className="me-3"
                  style={{ fontWeight: 500 }}
                >
                  Restaurant
                </FormLabel>
                <Col sm={7}>
                  <FormControl
                    type="text"
                    placeholder="Restaurant"
                    defaultValue={deliveryToUpdate.restaurant}
                    onChange={(e) =>
                      setDeliveryToUpdate({
                        ...deliveryToUpdate,
                        restaurant: e.target.value,
                      })
                    }
                  />
                </Col>
              </FormGroup>

              <FormGroup as={Row} className="d-flex align-items-center mb-3">
                <FormLabel
                  column
                  sm={4}
                  className="me-3"
                  style={{ fontWeight: 500 }}
                >
                  Customer Neighborhood
                </FormLabel>
                <Col sm={7}>
                  <FormControl
                    type="text"
                    placeholder="Customer Neighborhood"
                    defaultValue={deliveryToUpdate.customerNeighborhood}
                    onChange={(e) =>
                      setDeliveryToUpdate({
                        ...deliveryToUpdate,
                        customerNeighborhood: e.target.value,
                      })
                    }
                  />
                </Col>
              </FormGroup>

              <FormGroup as={Row} className="d-flex align-items-center mb-4">
                <FormLabel
                  column
                  sm={4}
                  className="me-3"
                  style={{ fontWeight: 500 }}
                >
                  Notes
                </FormLabel>
                <Col sm={7}>
                  <FormControl
                    as="textarea"
                    rows={3}
                    placeholder="Notes"
                    defaultValue={deliveryToUpdate.notes}
                    onChange={(e) =>
                      setDeliveryToUpdate({
                        ...deliveryToUpdate,
                        notes: e.target.value,
                      })
                    }
                  />
                </Col>
              </FormGroup>

              <div style={{ display: "flex", justifyContent: "flex-end" }}>
                <Button
                  onClick={updateDelivery}
                  variant="contained"
                  disableElevation
                  style={{
                    backgroundColor: "#1E293B",
                    color: "white",
                    textTransform: "none",
                    fontWeight: 600,
                  }}
                >
                  Update Delivery
                </Button>
              </div>
            </div>
          )}
        </Modal.Body>
      </Modal>
    </div>
  );
}
