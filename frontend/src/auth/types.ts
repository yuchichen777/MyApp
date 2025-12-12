// src/auth/types.ts
export type AuthContextType = {
    token: string | null;
    refreshToken: string | null;
    userName: string | null;
    role: string | null;
    isAuthenticated: boolean;
    login: (accessToken: string, userName: string, refreshToken: string, role: string) => void;
    logout: () => void;
};
