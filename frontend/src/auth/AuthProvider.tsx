// src/auth/AuthProvider.tsx
import React, { useEffect, useState, useCallback } from "react";
import { AuthContext } from "./AuthContext";
import type { AuthContextType } from "./types";
import type { JwtPayload } from "./jwtTypes";
import { OpenAPI } from "../api/generated/core/OpenAPI";
import { registerOnUnauthorized } from "../api/apiClient";
import { AuthService } from "../api/generated/services/AuthService";

const TOKEN_KEY = "myapp_token";
const REFRESH_TOKEN_KEY = "myapp_refresh_token";
const USERNAME_KEY = "myapp_username";
const ROLE_KEY = "myapp_role";

function decodeRoleFromToken(token: string | null): string | null {
    if (!token) return null;
    try {
        const parts = token.split(".");
        if (parts.length !== 3) return null;

        const payloadJson = atob(parts[1].replace(/-/g, "+").replace(/_/g, "/"));
        const payload: JwtPayload = JSON.parse(payloadJson);

        const roleClaim = payload["role"];
        return roleClaim ?? null;
    } catch {
        return null;
    }
}

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [token, setToken] = useState<string | null>(() => localStorage.getItem(TOKEN_KEY));
    const [refreshToken, setRefreshToken] = useState<string | null>(() =>
        localStorage.getItem(REFRESH_TOKEN_KEY)
    );
    const [userName, setUserName] = useState<string | null>(() =>
        localStorage.getItem(USERNAME_KEY)
    );
    const [role, setRole] = useState<string | null>(() => localStorage.getItem(ROLE_KEY));

    const applyTokenToOpenApi = (t: string | null) => {
        if (t) {
            OpenAPI.TOKEN = t;
            OpenAPI.HEADERS = {
                ...(OpenAPI.HEADERS ?? {}),
                Authorization: `Bearer ${t}`,
            };
        } else {
            OpenAPI.TOKEN = undefined;
            OpenAPI.HEADERS = undefined;
        }
    };

    useEffect(() => {
        applyTokenToOpenApi(token);
    }, [token]);

    const login = useCallback((
        accessToken: string,
        name: string,
        refresh: string,
        roleFromServer: string
    ) => {
        const decodedRole = decodeRoleFromToken(accessToken) ?? roleFromServer;

        setToken(accessToken);
        setRefreshToken(refresh);
        setUserName(name);
        setRole(decodedRole);

        localStorage.setItem(TOKEN_KEY, accessToken);
        localStorage.setItem(REFRESH_TOKEN_KEY, refresh);
        localStorage.setItem(USERNAME_KEY, name);

        if (decodedRole) {
            localStorage.setItem(ROLE_KEY, decodedRole);
        } else {
            localStorage.removeItem(ROLE_KEY);
        }

        applyTokenToOpenApi(accessToken);
    }, []);


    const logout = useCallback(() => {
        setToken(null);
        setRefreshToken(null);
        setUserName(null);
        setRole(null);

        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(REFRESH_TOKEN_KEY);
        localStorage.removeItem(USERNAME_KEY);
        localStorage.removeItem(ROLE_KEY);
    }, []);

    // 🔥 401 全域處理：先嘗試 refresh，失敗才真正登出
    useEffect(() => {
        registerOnUnauthorized(async () => {
            if (!refreshToken) {
                logout();
                return;
            }

            try {
                const res = await AuthService.postApiAuthRefresh({
                    // 名稱依照你 Swagger OperationId 產生的 method 調整
                    refreshToken,
                });

                const loginResultDto = res.data;

                if (!loginResultDto || !loginResultDto.accessToken || !loginResultDto.refreshToken) {
                    logout();
                    return;
                }

                login(
                    loginResultDto.accessToken,
                    loginResultDto.userName ?? "",
                    loginResultDto.refreshToken,
                    loginResultDto.role ?? ""
                );
            } catch {
                logout();
            }
        });
    }, [refreshToken, login, logout]);

    const value: AuthContextType = {
        token,
        refreshToken,
        userName,
        role,
        isAuthenticated: !!token,
        login,
        logout,
    };

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
