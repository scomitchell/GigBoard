import { Modal, FormGroup, FormLabel, FormControl, Row, Col } from "react-bootstrap";
import { Button } from "@mui/material";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import MyExpenses from "./MyExpenses";
import * as client from "./client";
import type { Expense, FullExpense } from "../types";

export default function Expenses() {
    // Modal control
    const [showForm, setShowForm] = useState(false);

    // State variable for new expense
    const [expense, setExpense] = useState<Expense>({});
    const [myExpenses, setMyExpenses] = useState<FullExpense[]>([]);

    // Error handling
    const [error, setError] = useState("");

    const navigate = useNavigate();

    const addExpense = async () => {
        try {
            if (!expense.amount || !expense.date || !expense.type) {
                alert("Please fill in all fields before submitting");
                return;
            }

            const parsedExpense = {
                ...expense,
                amount: expense.amount
            }

            const newExpense = await client.addExpense(parsedExpense);
            setMyExpenses(prev => [newExpense, ...prev]);
            setShowForm(false);
            navigate("/GigBoard/Expenses");
        } catch (err) {
            setError(err instanceof Error ? err.message : "An error occurred");
        }
    }

    if (error.length > 0) {
        return (
            <p>{error}</p>
        )
    }

    return (
        <div id="da-expenses">
            <div id="da-expenses-header" className="d-flex align-items-center">
                <h1 className="me-2">Track Your Expenses</h1>
                <Button onClick={() => setShowForm(true)} variant="contained" color="primary">
                    Add Expense
                </Button>

                <Modal show={showForm} onHide={() => setShowForm(false)} centered size="lg">
                    <Modal.Header closeButton>
                        <Modal.Title>Add Expense</Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        <div id="add-expense-details">
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">Expense Amount</FormLabel>
                                <Col sm={7}>
                                    <FormControl 
                                        type="number"
                                        min="0.01"
                                        step="0.01"
                                        placeholder="Expense Amount"
                                        onChange={(e) => setExpense({...expense, amount: parseFloat(e.target.value)})}
                                    />
                                </Col>
                            </FormGroup>
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">Expense Date</FormLabel>
                                <Col sm={7}>
                                    <FormControl 
                                        type="date"
                                        onChange={(e) => setExpense({...expense, date: e.target.value})}
                                    />
                                </Col>
                            </FormGroup>
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">Expense Type</FormLabel>
                                <Col sm={7}>
                                    <FormControl 
                                        type="text"
                                        placeholder="Expense Type"
                                        onChange={(e) => setExpense({...expense, type: e.target.value})}
                                    />
                                </Col>
                            </FormGroup>
                            <FormGroup as={Row} className="d-flex align-items-center mb-2">
                                <FormLabel column sm={4} className="me-3">Expense Notes</FormLabel>
                                <Col sm={7}>
                                    <FormControl 
                                        type="text"
                                        placeholder="Expense Notes"
                                        onChange={(e) => setExpense({...expense, notes: e.target.value})}
                                    />
                                </Col>
                            </FormGroup>
                            <Button onClick={addExpense} variant="contained" color="primary">
                                Add Expense
                            </Button>
                        </div>
                    </Modal.Body>
                </Modal>
            </div>
            <MyExpenses myExpenses={myExpenses} setMyExpenses={setMyExpenses} />
        </div>
    );
}