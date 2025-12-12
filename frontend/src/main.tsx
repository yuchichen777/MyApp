// src/main.tsx
import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import { BrowserRouter } from "react-router-dom";
import { AuthProvider } from "./auth/AuthProvider";
import { OpenAPI } from "./api/generated/core/OpenAPI";
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { Toaster } from "react-hot-toast";

// 🔥 讓所有 generated API 都從 localStorage 拿 token
OpenAPI.TOKEN = async (): Promise<string> => {
    return localStorage.getItem("myapp_token") ?? "";
};

OpenAPI.BASE = "http://localhost:5166";

ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(
    <React.StrictMode>
        <BrowserRouter>
            <AuthProvider>
                <App />
                {/* 全域 Toast 容器，一次掛在最外層就好 */}
                <ToastContainer position="top-right" autoClose={3000} />
                <Toaster position="top-right" />
            </AuthProvider>
        </BrowserRouter>
    </React.StrictMode>
);
