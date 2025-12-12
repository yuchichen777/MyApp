// src/layout/MainLayout.tsx
import React from "react";
import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/useAuth";

const MainLayout: React.FC = () => {
    const { userName, role, logout } = useAuth();
    const isAdmin = role === "Admin";

    return (
        <div style={{ minHeight: "100vh", display: "flex", flexDirection: "column" }}>
            {/* 頂部列 */}
            <header
                style={{
                    padding: 16,
                    borderBottom: "1px solid #ddd",
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                }}
            >
                <h1 style={{ margin: 0 }}>MyApp 前端</h1>
                <div>
                    <span style={{ marginRight: 12 }}>
                        使用者：{userName}（角色：{role ?? "未設定"}）
                    </span>
                    <button onClick={logout}>登出</button>
                </div>
            </header>

            {/* 內容 + 導覽 */}
            <div style={{ display: "flex", flex: 1 }}>
                {/* 左側導航 */}
                <nav
                    style={{
                        width: 180,
                        borderRight: "1px solid #ddd",
                        padding: 16,
                    }}
                >
                    <div style={{ marginBottom: 8 }}>
                        <NavLink
                            to="/products"
                            style={({ isActive }) => ({
                                fontWeight: isActive ? "bold" : "normal",
                                textDecoration: "none",
                            })}
                        >
                            產品管理
                        </NavLink>
                    </div>

                    <div>
                        <NavLink
                            to="/customers"
                            style={({ isActive }) => ({
                                fontWeight: isActive ? "bold" : "normal",
                                textDecoration: "none",
                            })}
                        >
                            客戶管理
                        </NavLink>
                    </div>

                    {isAdmin && (
                        <div>
                            <NavLink
                                to="/users"
                                style={({ isActive }) => ({
                                    fontWeight: isActive ? "bold" : "normal",
                                    textDecoration: "none",
                                })}
                            >
                                使用者管理
                            </NavLink>
                        </div>
                    )}
                </nav>

                {/* 右側內容區 */}
                <main style={{ flex: 1, padding: 16 }}>
                    <Outlet />
                </main>
            </div>
        </div>
    );
};

export default MainLayout;
