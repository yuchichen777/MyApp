// src/api/client.ts
import axios from "axios";

const api = axios.create({
    baseURL: import.meta.env.DEV
        ? "http://localhost:5166/api" // 本機 dotnet run
        : "/api",                    // Docker / Nginx / 正式環境
    withCredentials: true,         // 未來若用 cookie / refresh token 會用到
});

export default api;