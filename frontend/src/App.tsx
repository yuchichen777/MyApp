// src/App.tsx
import React from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import LoginPage from "./features/auth/LoginPage";
import ProductList from "./features/products/ProductList";
import CustomerList from "./features/customers/CustomerList";
import UsersList from "./features/users/UsersList";
import ProtectedRoute from "./routes/ProtectedRoute";
import MainLayout from "./layout/MainLayout";
import RequireRole from "./routes/RequireRole";

const App: React.FC = () => {
    return (
        <Routes>
            {/* 公開頁：登入 / 註冊 */}
            <Route path="/login" element={<LoginPage />} />

            {/* 需登入的區塊 */}
            <Route element={<ProtectedRoute />}>
                <Route element={<MainLayout />}>
                    <Route path="/products" element={<ProductList />} />
                    <Route path="/customers" element={<CustomerList />} />
                    <Route element={<RequireRole role="Admin" />}>
                        <Route path="/users" element={<UsersList />} />
                    </Route>
                    {/* 預設導向 /products */}
                    <Route path="/" element={<Navigate to="/products" replace />} />
                </Route>
            </Route>

            {/* 兜底：不認識的路徑都導回首頁 */}
            <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
    );
};

export default App;
