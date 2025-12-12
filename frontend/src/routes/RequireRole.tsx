//src/routes/RequireRole.tsx
import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../auth/useAuth";

interface RequireRoleProps {
    role: string;
    redirectTo?: string;
}

const RequireRole: React.FC<RequireRoleProps> = ({ role, redirectTo = "/" }) => {
    const { isAuthenticated, role: currentRole } = useAuth();

    // 未登入：請先登入
    if (!isAuthenticated) {
        return <Navigate to="/login" replace />;
    }

    // 已登入但角色不符：導回首頁或自訂頁
    if (currentRole !== role) {
        return <Navigate to={redirectTo} replace />;
    }

    // 角色 OK，放行
    return <Outlet />;
};

export default RequireRole;
