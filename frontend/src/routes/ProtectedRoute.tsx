// src/routes/ProtectedRoute.tsx
import React from "react";
import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../auth/useAuth";

const ProtectedRoute: React.FC = () => {
    const { token } = useAuth();
    const location = useLocation();

    if (!token) {
        // 沒登入 → 導到 /login，並把來源路徑記起來
        return <Navigate to="/login" replace state={{ from: location }} />;
    }

    // 已登入 → 繼續往下 render 對應的子路由
    return <Outlet />;
};

export default ProtectedRoute;
