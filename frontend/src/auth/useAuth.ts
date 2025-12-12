// auth/useAuth.ts
import { useContext } from "react";
import type { AuthContextType } from "./types";
import { AuthContext } from "./AuthContext";

export const useAuth = (): AuthContextType => {
    const ctx = useContext(AuthContext);
    if (!ctx) {
        throw new Error("useAuth must be used within AuthProvider");
    }
    return ctx;
};
