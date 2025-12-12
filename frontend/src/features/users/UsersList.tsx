// src/features/users/UsersList.tsx
import { useEffect, useState } from "react";
import { UsersService } from "../../api/generated/services/UsersService";
import type { UserDto } from "../../api/generated/models/UserDto";
import type { UserUpdateDto } from "../../api/generated/models/UserUpdateDto";
import { unwrap, handleApiError } from "../../api/apiClient";
import { useAuth } from "../../auth/useAuth";
import { notifySuccess } from "../../utils/notify";
import DeleteButton from "../../components/DeleteButton";

type User = UserDto;

const UsersList = () => {
    const { role: currentRole, userName: currentUserName, token } = useAuth();
    const [items, setItems] = useState<User[]>([]);
    const [page, setPage] = useState(1);
    const [pageSize] = useState(10);
    const [total, setTotal] = useState(0);
    const [keyword, setKeyword] = useState("");
    const [roleFilter, setRoleFilter] = useState<string>("");

    // 編輯用狀態
    const [editingUser, setEditingUser] = useState<User | null>(null);
    const [editRole, setEditRole] = useState("");
    const [editIsActive, setEditIsActive] = useState(false);
    const [editPassword, setEditPassword] = useState("");
    const [loading, setLoading] = useState(false);

    const totalPages = Math.max(1, Math.ceil(total / pageSize));

    const load = async () => {
        setLoading(true);
        try {
            const res = await unwrap(
                UsersService.getUsersPaged(
                    page,
                    pageSize,
                    keyword || undefined,
                    undefined,
                    false,
                    roleFilter || undefined
                )
            );

            setItems(res?.items ?? []);
            setTotal(res?.totalCount ?? 0);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        load();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [page, pageSize, keyword, roleFilter, token]);

    // 點「編輯」按鈕
    const handleEditClick = (u: User) => {
        setEditingUser(u);
        setEditRole(u.role ?? "User");
        setEditIsActive(u.isActive ?? true);
        setEditPassword(""); // 每次打開編輯，密碼欄清空
    };

    // 取消編輯
    const handleCancelEdit = () => {
        setEditingUser(null);
    };

    // 確認儲存
    const handleSaveEdit = async () => {
        if (!editingUser) return;

        // 依照後端 UserUpdateDto 結構組 payload
        const payload: UserUpdateDto = {
            id: editingUser.id,
            role: editRole,
            isActive: editIsActive,
            password: editPassword.trim() === "" ? undefined : editPassword.trim(),
        };

        try {
            setLoading(true);
            await unwrap(
                // 這裡要對應你的 OpenAPI 產生的名稱
                // 一般會是 putApiUsers(id, body) 或 updateUser 等
                UsersService.putApiUsers(editingUser.id!, payload)
            );
            notifySuccess("更新成功");
            setEditingUser(null);
            await load();
        } catch (e) {
            console.error(e);
            handleApiError(e, "更新失敗，請稍後再試");
        } finally {
            setLoading(false);
        }
    };

    // 點「刪除」按鈕
    const handleDeleteClick = async (u: User) => {
        // 防呆 1：不能刪掉 Admin 帳號
        if (u.role === "Admin") {
            notifySuccess("無法刪除 Admin 帳號");
            return;
        }

        // 防呆 2：不能刪掉自己（依你實際 userName 判斷）
        if (u.userName === currentUserName) {
            notifySuccess("不能刪除目前登入的帳號");
            return;
        }

        try {
            setLoading(true);
            await unwrap(
                // 一般會是 deleteApiUsers(id)
                UsersService.deleteApiUsers(u.id!)
            );
            notifySuccess("刪除成功");
            // 如果刪到最後一頁只剩一筆，可以視情況調整 page
            if (items.length === 1 && page > 1) {
                setPage(p => p - 1);
            } else {
                await load();
            }
        } catch (e) {
            console.error(e);
            handleApiError(e, "刪除失敗，請稍後再試");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div>
            <h2>使用者管理（僅 Admin）</h2>
            <p>目前角色：{currentRole}</p>

            {/* 編輯區塊（簡單 inline 表單） */}
            {editingUser && (
                <div
                    style={{
                        border: "1px solid #ccc",
                        padding: 12,
                        marginBottom: 12,
                        borderRadius: 4,
                        background: "#f9f9f9",
                    }}
                >
                    <h3>編輯使用者（ID: {editingUser.id}）</h3>
                    <div style={{ marginBottom: 8 }}>
                        <label style={{ marginRight: 8 }}>UserName：</label>
                        <span>{editingUser.userName}</span>
                    </div>

                    <div style={{ marginBottom: 8 }}>
                        <label style={{ marginRight: 8 }}>Role：</label>
                        <select
                            value={editRole}
                            onChange={(e) => setEditRole(e.target.value)}
                        >
                            <option value="Admin">Admin</option>
                            <option value="User">User</option>
                        </select>
                    </div>

                    <div style={{ marginBottom: 8 }}>
                        <label style={{ marginRight: 8 }}>狀態：</label>
                        <label>
                            <input
                                type="checkbox"
                                checked={editIsActive}
                                onChange={(e) => setEditIsActive(e.target.checked)}
                            />{" "}
                            啟用
                        </label>
                    </div>

                    <div style={{ marginBottom: 8 }}>
                        <label style={{ marginRight: 8 }}>新密碼：</label>
                        <input
                            type="password"
                            placeholder="留空則不修改密碼"
                            value={editPassword}
                            onChange={(e) => setEditPassword(e.target.value)}
                        />
                    </div>

                    <div>
                        <button onClick={handleSaveEdit} disabled={loading}>
                            儲存
                        </button>
                        <button
                            onClick={handleCancelEdit}
                            style={{ marginLeft: 8 }}
                            disabled={loading}
                        >
                            取消
                        </button>
                    </div>
                </div>
            )}

            {/* 篩選條件 */}
            <div style={{ marginBottom: 12 }}>
                <input
                    placeholder="輸入帳號關鍵字"
                    value={keyword}
                    onChange={(e) => {
                        setPage(1);
                        setKeyword(e.target.value);
                    }}
                    style={{ marginRight: 8 }}
                />
                <select
                    value={roleFilter}
                    onChange={(e) => {
                        setPage(1);
                        setRoleFilter(e.target.value);
                    }}
                    style={{ marginRight: 8 }}
                >
                    <option value="">全部角色</option>
                    <option value="Admin">Admin</option>
                    <option value="User">User</option>
                </select>
                <button onClick={() => setPage(1)}>查詢</button>
            </div>

            {/* 列表 */}
            <table
                style={{
                    width: "100%",
                    borderCollapse: "collapse",
                }}
            >
                <thead>
                    <tr>
                        <th style={{ borderBottom: "1px solid #ccc" }}>ID</th>
                        <th style={{ borderBottom: "1px solid #ccc" }}>UserName</th>
                        <th style={{ borderBottom: "1px solid #ccc" }}>Role</th>
                        <th style={{ borderBottom: "1px solid #ccc" }}>IsActive</th>
                        <th style={{ borderBottom: "1px solid #ccc" }}>CreatedAt</th>
                        <th style={{ borderBottom: "1px solid #ccc" }}>操作</th>
                    </tr>
                </thead>
                <tbody>
                    {items.map((u) => (
                        <tr key={u.id}>
                            <td style={{ borderBottom: "1px solid #eee" }}>{u.id}</td>
                            <td style={{ borderBottom: "1px solid #eee" }}>{u.userName}</td>
                            <td style={{ borderBottom: "1px solid #eee" }}>{u.role}</td>
                            <td style={{ borderBottom: "1px solid #eee" }}>
                                {u.isActive ? "啟用" : "停用"}
                            </td>
                            <td style={{ borderBottom: "1px solid #eee" }}>
                                {u.createdAt
                                    ? new Date(u.createdAt).toLocaleString()
                                    : ""}
                            </td>
                            <td style={{ borderBottom: "1px solid #eee" }}>
                                <button
                                    onClick={() => handleEditClick(u)}
                                    style={{ marginRight: 8 }}
                                >
                                    編輯
                                </button>
                                <DeleteButton
                                    label="刪除"
                                    confirmMessage={`確定要刪除使用者「${u.userName}」嗎？`}
                                    onConfirm={() => handleDeleteClick(u)}
                                    disabled={loading}
                                />
                            </td>
                        </tr>
                    ))}
                    {items.length === 0 && (
                        <tr>
                            <td colSpan={6} style={{ padding: 8 }}>
                                尚無資料
                            </td>
                        </tr>
                    )}
                </tbody>
            </table>

            {/* 分頁器 */}
            <div style={{ marginTop: 12 }}>
                <button
                    disabled={page <= 1 || loading}
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                >
                    上一頁
                </button>
                <span style={{ margin: "0 8px" }}>
                    第 {page} / {totalPages} 頁（共 {total} 筆）
                </span>
                <button
                    disabled={page >= totalPages || loading}
                    onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                >
                    下一頁
                </button>
                {loading && <span style={{ marginLeft: 8 }}>載入中...</span>}
            </div>
        </div>
    );
};

export default UsersList;
