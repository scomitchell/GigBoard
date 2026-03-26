import {
  Modal,
  FormGroup,
  FormControl,
  FormLabel,
  Row,
  Col,
  Dropdown,
} from "react-bootstrap";
import { Card, CardContent, Typography, Button } from "@mui/material";
import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import * as client from "./client";
import type { ShiftFilters } from "./client";
import type { FullShift } from "../types";
import './shift.css'

export default function MyShifts({
  myShifts,
  setMyShifts,
}: {
  myShifts: FullShift[];
  setMyShifts: React.Dispatch<React.SetStateAction<FullShift[]>>;
}) {
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
    const shifts = await client.findUserShifts();
    setMyShifts(shifts);
  }, [setMyShifts]);

  const applyFilters = async () => {
    const filters: ShiftFilters = {
      startTime: startTime,
      endTime: endTime,
      app: app,
    };

    const shifts = await client.getFilteredShifts(filters);
    setMyShifts(shifts);
    setShowForm(false);
  };

  // Delete shift from db
  const deleteShift = async (shiftId: number) => {
    await client.deleteUserShift(shiftId);
    fetchShifts();
    setShiftToDelete(-1);
  };

  // Update shift in db
  const updateShift = async () => {
    if (!shiftToUpdate) return;

    await client.updateUserShift(shiftToUpdate);
    fetchShifts();
    setShiftToUpdate(null);
  };

  // Fetch list of user used apps
  const fetchApps = useCallback(async () => {
    const apps = await client.getUserApps();
    setUserApps(apps);
  }, []);

  const formatShiftDetails = (start: string, end: string) => {
    const startDate = new Date(start);
    const endDate = new Date(end);

    const fullDateOpts: Intl.DateTimeFormatOptions = {
      month: "short",
      day: "numeric",
      year: "numeric",
    };
    const shortDateOpts: Intl.DateTimeFormatOptions = {
      month: "short",
      day: "numeric",
    };
    const timeOpts: Intl.DateTimeFormatOptions = {
      hour: "numeric",
      minute: "2-digit",
    };

    const diffMs = Math.max(0, endDate.getTime() - startDate.getTime()); // Prevent negative time
    const diffHrs = Math.floor(diffMs / (1000 * 60 * 60));
    const diffMins = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));

    let durationStr = "";
    if (diffHrs > 0) durationStr += `${diffHrs}h `;
    if (diffMins > 0 || diffHrs === 0) durationStr += `${diffMins}m`;

    let dateHeader = "";
    const timeRange = `${startDate.toLocaleTimeString(undefined, timeOpts)} - ${endDate.toLocaleTimeString(undefined, timeOpts)}`;

    if (startDate.toDateString() === endDate.toDateString()) {
      dateHeader = startDate.toLocaleDateString(undefined, fullDateOpts);
    } else {
      const startDay = startDate.toLocaleDateString(undefined, shortDateOpts);
      const endDay = endDate.toLocaleDateString(undefined, shortDateOpts);
      dateHeader = `${startDay} - ${endDay}, ${endDate.getFullYear()}`;
    }

    return {
      dateHeader,
      timeRange,
      duration: durationStr.trim(),
    };
  };

  // Clear all filters
  const resetFilters = () => {
    setStartTime(null);
    setEndTime(null);
    setApp(null);
    setReset(true);
  };

  useEffect(() => {
    fetchShifts();
    fetchApps();
  }, [fetchApps, fetchShifts]);

  // useEffect for reset filters
  useEffect(() => {
    const allCleared = startTime === null && endTime === null && app === null;

    if (reset && allCleared) {
      fetchShifts();
      setReset(false);
    }
  }, [startTime, endTime, app, reset, fetchShifts]);

  return (
    <div id="da-my-shifts" className="mt-3 col-sm-8">
      <Button
        onClick={() => setShowForm(true)}
        variant="outlined"
        disableElevation
        className="shift-filter-btn"
      >
        Filter Shifts
      </Button>
      <Button
        onClick={resetFilters}
        variant="text"
        className="shift-reset-btn"
      >
        Reset filters
      </Button>

      <Modal
        show={showForm}
        onHide={() => setShowForm(false)}
        centered
        size="lg"
      >
        <Modal.Header closeButton>
          <Modal.Title>Filter Shifts</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <div className="filter-shifts">
            <FormGroup as={Row} className="mb-3">
              <FormLabel column sm={4}>
                Starts after
              </FormLabel>
              <Col sm={7}>
                <FormControl
                  type="datetime-local"
                  value={startTime === null ? "" : startTime}
                  onChange={(e) =>
                    setStartTime(e.target.value === "" ? null : e.target.value)
                  }
                />
              </Col>
            </FormGroup>
            <FormGroup as={Row} className="mb-3">
              <FormLabel column sm={4}>
                Ends before
              </FormLabel>
              <Col sm={7}>
                <FormControl
                  type="datetime-local"
                  value={endTime === null ? "" : endTime}
                  onChange={(e) =>
                    setEndTime(e.target.value === "" ? null : e.target.value)
                  }
                />
              </Col>
            </FormGroup>
            <FormGroup as={Row} className="d-flex align-items-center mb-2">
              <FormLabel column sm={4}>
                App
              </FormLabel>
              <Col sm={7}>
                <select
                  onChange={(e) => setApp(e.target.value)}
                  className="form-control mb-2"
                  id="da-app"
                >
                  <option value=""></option>
                  {userApps.map((app: string) => (
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
              disableElevation
              className="shift-submit-btn"
            >
              Apply Filters
            </Button>
          </div>
        </Modal.Body>
      </Modal>

      <Row>
        {myShifts.map((shift: FullShift) => {
          const { dateHeader, timeRange, duration } = formatShiftDetails(
            shift.startTime,
            shift.endTime,
          );
          return (
            <Col md={6} xl={6} key={shift.id} className="mb-4">
              <Card className="shift-card">
                <CardContent sx={{ p: 3 }}>
                  {/* Card Controls */}
                  <div className="shift-card-controls">
                    <Dropdown>
                      <Dropdown.Toggle
                        variant="light"
                        size="sm"
                        className="shift-dropdown-toggle"
                      >
                        &#x22EE;
                      </Dropdown.Toggle>

                      <Dropdown.Menu>
                        <Dropdown.Item
                          onClick={() => setShiftToDelete(Number(shift.id))}
                          style={{ color: "#EF4444" }}
                        >
                          Delete Shift
                        </Dropdown.Item>
                        <Dropdown.Item
                          onClick={() => setShiftToUpdate(shift)}
                          style={{ color: "#1E293B" }}
                        >
                          Update Shift
                        </Dropdown.Item>
                      </Dropdown.Menu>
                    </Dropdown>
                  </div>

                  {/* Clickable Card Body */}
                  <Link
                    to={`/GigBoard/Shifts/${shift.id}`}
                    className="text-decoration-none text-dark"
                    style={{ display: "block" }}
                  >
                    <Typography
                      variant="overline"
                      className="shift-app-badge"
                    >
                      {shift.app}
                    </Typography>

                    <Typography
                      variant="body2"
                      className="shift-date-header"
                    >
                      {dateHeader}
                    </Typography>

                    <div className="shift-time-duration">
                      <Typography
                        variant="h5"
                        fontWeight="bold"
                        sx={{ color: "#111827" }}
                      >
                        {timeRange}
                      </Typography>

                      <span className="shift-duration-badge">
                        {duration}
                      </span>
                    </div>
                  </Link>
                </CardContent>
              </Card>
            </Col>
          );
        })}
      </Row>

      <Modal
        show={shiftToDelete !== -1}
        onHide={() => setShiftToDelete(-1)}
        centered
        size="lg"
      >
        <Modal.Header closeButton>
          <Modal.Title>Confirm Deletion</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          Are you sure you want to delete this shift? This action cannot be
          undone.
        </Modal.Body>
        <Modal.Footer>
          <Button
            variant="contained"
            color="secondary"
            className="shift-cancel-btn"
            onClick={() => setShiftToDelete(-1)}
            disableElevation
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            color="error"
            onClick={() => {
              deleteShift(shiftToDelete);
            }}
            disableElevation
          >
            Delete
          </Button>
        </Modal.Footer>
      </Modal>

      <Modal
        show={shiftToUpdate !== null}
        onHide={() => setShiftToUpdate(null)}
        centered
        size="lg"
      >
        <Modal.Header closeButton>
          <Modal.Title>Update Shift</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {shiftToUpdate && (
            <div id="add-shift-details">
              <FormGroup as={Row} className="d-flex align-items-center mb-3">
                <FormLabel
                  column
                  sm={4}
                  className="me-3 shift-form-label"
                >
                  Start Time
                </FormLabel>
                <Col sm={7}>
                  <FormControl
                    type="datetime-local"
                    defaultValue={shiftToUpdate.startTime}
                    onChange={(e) =>
                      setShiftToUpdate({
                        ...shiftToUpdate,
                        startTime: e.target.value,
                      })
                    }
                  />
                </Col>
              </FormGroup>

              <FormGroup as={Row} className="d-flex align-items-center mb-3">
                <FormLabel
                  column
                  sm={4}
                  className="me-3 shift-form-label"
                >
                  End Time
                </FormLabel>
                <Col sm={7}>
                  <FormControl
                    type="datetime-local"
                    defaultValue={shiftToUpdate.endTime}
                    onChange={(e) =>
                      setShiftToUpdate({
                        ...shiftToUpdate,
                        endTime: e.target.value,
                      })
                    }
                  />
                </Col>
              </FormGroup>

              <FormGroup as={Row} className="d-flex align-items-center mb-4">
                <FormLabel
                  column
                  sm={4}
                  className="me-3 shift-form-label"
                >
                  App
                </FormLabel>
                <Col sm={7}>
                  <select
                    onChange={(e) =>
                      setShiftToUpdate({
                        ...shiftToUpdate,
                        app: e.target.value,
                      })
                    }
                    className="form-control"
                    defaultValue={shiftToUpdate.app}
                    id="da-app"
                  >
                    <option value=""></option>
                    <option value="Doordash">Doordash</option>
                    <option value="UberEats">Uber Eats</option>
                    <option value="Grubhub">Grubhub</option>
                    <option value="InstaCart">Instacart</option>
                  </select>
                </Col>
              </FormGroup>

              <div style={{ display: "flex", justifyContent: "flex-end" }}>
                <Button
                  onClick={updateShift}
                  variant="contained"
                  disableElevation
                  className="shift-submit-btn"
                >
                  Update Shift
                </Button>
              </div>
            </div>
          )}
        </Modal.Body>
      </Modal>
    </div>
  );
}
