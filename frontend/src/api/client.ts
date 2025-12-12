import axios from "axios";

const api = axios.create({
    baseURL: "http://localhost:5166/api", // 對應後端 MyApp.Api
});

export default api;
