import React, { useState } from "react";
import { AuthService } from "../../api/generated/services/AuthService";
import { useAuth } from "../../auth/useAuth";
import { parseValidationErrors } from "../../api/apiClient";
import { useNavigate, useLocation } from "react-router-dom";
import type { LoginResultDto } from "../../api/generated";

type Tab = "login" | "register";

export const LoginPage: React.FC = () => {
    const { login } = useAuth();
    const [tab, setTab] = useState<Tab>("login");
    const navigate = useNavigate();
    const location = useLocation();

    const from =
        (location.state as { from?: Location })?.from?.pathname ?? "/products";

    const [loginForm, setLoginForm] = useState({
        userName: "",
        password: "",
    });

    const [registerForm, setRegisterForm] = useState({
        userName: "",
        password: "",
        confirmPassword: "",
    });

    const [errors, setErrors] = useState<Record<string, string[]>>({});
    const [submitting, setSubmitting] = useState(false);

    const handleLoginChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setLoginForm((prev) => ({ ...prev, [name]: value }));
    };

    const handleRegisterChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setRegisterForm((prev) => ({ ...prev, [name]: value }));
    };

    const handleLoginSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitting(true);
        setErrors({});

        try {
            // 直接呼叫 AuthService.login 對應的 method
            const res = await AuthService.postApiAuthLogin(loginForm);
            // 不用 unwrap：這裡可能沒有 data.errors 結構，直接看 res.data
            const loginResultDto = res.data;

            if (loginResultDto) {
                login(loginResultDto.accessToken as string, loginForm.userName, loginResultDto.refreshToken as string, loginResultDto.role as string);
                // 🔥 登入後導回原本想去的頁面（通常是 /products）
                navigate(from, { replace: true });
            } else {
                alert("登入失敗");
            }
            // TODO: 這裡可以導到首頁或 Product 頁
        } catch (error: unknown) {
            const errs = parseValidationErrors(error);
            if (errs) {
                setErrors(errs);
            } else {
                alert("登入失敗");
                console.error(error);
            }
        } finally {
            setSubmitting(false);
        }
    };

    const handleRegisterSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitting(true);
        setErrors({});

        try {
            const res = await AuthService.postApiAuthRegister(registerForm);
            const loginResultDto = res.data as LoginResultDto;
            if (!loginResultDto.accessToken) {
                alert("註冊失敗：未取得 token");
                return;
            }

            login(loginResultDto.accessToken as string, loginForm.userName, loginResultDto.refreshToken as string, loginResultDto.role as string);
            navigate(from, { replace: true });
            alert("註冊成功並已登入");
            // TODO: 這裡也可以導去首頁/產品頁
        } catch (error: unknown) {
            const errs = parseValidationErrors(error);
            if (errs) {
                setErrors(errs);
            } else {
                alert("註冊失敗");
                console.error(error);
            }
        } finally {
            setSubmitting(false);
        }
    };

    const renderErrors = (field: string) => {
        const msgs = errors[field];
        if (!msgs || msgs.length === 0) return null;
        return (
            <div style={{ color: "red", fontSize: 12 }}>
                {msgs.map((m, idx) => (
                    <div key={idx}>{m}</div>
                ))}
            </div>
        );
    };

    return (
        <div
            style={{
                maxWidth: 400,
                margin: "40px auto",
                padding: 16,
                border: "1px solid #ddd",
                borderRadius: 8,
            }}
        >
            <div style={{ marginBottom: 16, textAlign: "center" }}>
                <button
                    type="button"
                    onClick={() => {
                        setTab("login");
                        setErrors({});
                    }}
                    style={{
                        marginRight: 8,
                        padding: "4px 12px",
                        background: tab === "login" ? "#007bff" : "#eee",
                        color: tab === "login" ? "#fff" : "#000",
                        border: "none",
                        borderRadius: 4,
                    }}
                >
                    登入
                </button>
                <button
                    type="button"
                    onClick={() => {
                        setTab("register");
                        setErrors({});
                    }}
                    style={{
                        padding: "4px 12px",
                        background: tab === "register" ? "#007bff" : "#eee",
                        color: tab === "register" ? "#fff" : "#000",
                        border: "none",
                        borderRadius: 4,
                    }}
                >
                    註冊
                </button>
            </div>

            {tab === "login" && (
                <form onSubmit={handleLoginSubmit}>
                    <div style={{ marginBottom: 8 }}>
                        <label style={{ display: "block" }}>帳號：</label>
                        <input
                            name="userName"
                            value={loginForm.userName}
                            onChange={handleLoginChange}
                            required
                        />
                        {renderErrors("UserName")}
                    </div>

                    <div style={{ marginBottom: 8 }}>
                        <label style={{ display: "block" }}>密碼：</label>
                        <input
                            name="password"
                            type="password"
                            value={loginForm.password}
                            onChange={handleLoginChange}
                            required
                        />
                        {renderErrors("Password")}
                    </div>

                    <button type="submit" disabled={submitting}>
                        {submitting ? "登入中..." : "登入"}
                    </button>
                </form>
            )}

            {tab === "register" && (
                <form onSubmit={handleRegisterSubmit}>
                    <div style={{ marginBottom: 8 }}>
                        <label style={{ display: "block" }}>帳號：</label>
                        <input
                            name="userName"
                            value={registerForm.userName}
                            onChange={handleRegisterChange}
                            required
                        />
                        {renderErrors("UserName")}
                    </div>

                    <div style={{ marginBottom: 8 }}>
                        <label style={{ display: "block" }}>密碼：</label>
                        <input
                            name="password"
                            type="password"
                            value={registerForm.password}
                            onChange={handleRegisterChange}
                            required
                        />
                        {renderErrors("Password")}
                    </div>

                    <div style={{ marginBottom: 8 }}>
                        <label style={{ display: "block" }}>確認密碼：</label>
                        <input
                            name="confirmPassword"
                            type="password"
                            value={registerForm.confirmPassword}
                            onChange={handleRegisterChange}
                            required
                        />
                        {renderErrors("ConfirmPassword")}
                    </div>

                    <button type="submit" disabled={submitting}>
                        {submitting ? "註冊中..." : "註冊"}
                    </button>
                </form>
            )}
        </div>
    );
};

export default LoginPage;
