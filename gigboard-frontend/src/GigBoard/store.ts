import { configureStore } from "@reduxjs/toolkit"
import accountReducer from "./Account/reducer"

const store = configureStore({
    reducer: {
        accountReducer,
    },
});

export type RootState = ReturnType<typeof store.getState>;
export default store;