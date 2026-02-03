# GigBoard

**View Live App:** https://gigboard-app.netlify.app/

GigBoard is a Full-Stack web application that empowers gig delivery drivers (DoorDash, UberEats) to track earnings, shifts, and expenses, and gain key performance insights.

## Backend Features Include
- **JWT Authentication** - Secure login and signup using JSON Web Tokens
- **CRUD operations** - Create, read, update, and delete Deliveries, Shifts, and Expenses
- **Shift association** - Deliveries can be associated with specific shifts
- **User profile management** - Users can update their profiles securely
- **Analytics Endpoint** - Backend computes average total, base, and tip pays, dollar-per-mile, most used and highest paying apps, highest paying restaurant and neighborhood, and average monthly expenses both total and by type.
- **Real-Time Statistics** - Backend automatically recalculates earnings, shift, and expense statistics on data changes and pushes to the frontend with SignalR.
- **Charts and ML** -  Python predicts earnings for a particular shift based on input time, neighborhood, and app. Separately, the C# backend prepares data for Plotly chart generation on the frontend.

## Frontend Features Include
- **Responsive Navigation** - Role aware navigation bar that updates on login/logout
- **Delivery Logger** - Easy-to-use forms to add, update, and remove delivery records
- **Shift Tracker** - View, manage, and analyze delivery shifts
- **Expense Tracker** - Log and categorize work-related expenses
- **Dashboard** - Summary of performance statistics including charts and an earnings predictor.
- **Search and Filters** - Quickly find deliveries, shifts, and expenses based on app, type, date, dollar amount, etc.
- **Real-Time UI Updates** - React dynamically updates statistics dashboard as SignalR pushes new data, eliminating manual refreshes.
- **Plotly Charts** - Interactive visualizations of delivery stats such as average base pay by app, tips by neighborhood, and other key metrics.

## Tech Stack
- **Frontend** - TypeScript, React, HTML, CSS
- **Backend** - C#, ASP.NET, Python, SQL Server

![Home Screen - Logged Out](./docs/images/GigBoard%20Landing%20Signed%20Out.png)
![Home Screen - New User/No Deliveries](./docs/images/GigBoard%20Landing%20New%20User%20No%20Data.png)
![Home Screen - Statistics Dash 1](./docs/images/GigBoard%20Statistics%20Dash%201.png)
![Home Screen - Statistics Dash 2](./docs/images/GigBoard%20Statistics%20Dash%202.png)
![Home Screen - Statistics Dash 3](./docs/images/GigBoard%20Statistics%20Dash%203.png)
![Home Screen - Statistics Dash 4](./docs/images/GigBoard%20Statistics%20Dash%204.png)
![Predict Earnings Screenshot](./docs/images/GigBoard%20Predict%20Earnings.png)
![Deliveries Screen](./docs/images/GigBoard%20Deliveries.png)
![Shifts Screen](./docs/images/GigBoard%20Shifts.png)
![Expenses Screen](./docs/images/GigBoard%20Expenses.png)
![Profile Screen](./docs/images/GigBoard%20Profile%20Screen.png)