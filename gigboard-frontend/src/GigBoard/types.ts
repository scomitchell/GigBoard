export type User = {
    email?: string | undefined,
    firstName?: string | undefined,
    lastName?: string | undefined,
    username?: string | undefined,
    password?: string | undefined
};

export interface Shift {
    app: string,
    startTime: string,
    endTime: string
}

export type FullShift = Shift & {
    id: number
}

export type Delivery = {
    id?: string,
    deliveryTime?: string,
    app?: string,
    customerNeighborhood?: string,
    restaurant?: string,
    notes?: string,
    totalPay?: number,
    tipPay?: number,
    basePay?: number,
    mileage?: number
}

export type Expense = {
    amount?: number,
    date?: string,
    type?: string,
    notes?: string
}

export type FullExpense = Expense & {
    id: number
}