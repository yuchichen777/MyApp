// src/features/customers/CustomerList.tsx
import { useEffect, useState } from "react";
import { CustomersService } from "../../api/generated/services/CustomersService";
import type { CustomerDto } from "../../api/generated/models/CustomerDto";
import CustomerForm from "./CustomerForm";
import { unwrap, handleApiError } from "../../api/apiClient";
import { useAuth } from "../../auth/useAuth";
import { notifySuccess } from "../../utils/notify";
import DeleteButton from "../../components/DeleteButton";

type Customer = CustomerDto;

const CustomerList = () => {
    const { role, token } = useAuth();
    const [items, setItems] = useState<Customer[]>([]);
    const [editing, setEditing] = useState<Customer | null>(null);

    const [page, setPage] = useState(1);
    const [pageSize] = useState(5);
    const [total, setTotal] = useState(0);
    const [keyword, setKeyword] = useState("");

    const isAdmin = role === "Admin";

    const load = async () => {
        const paged = await unwrap(
            CustomersService.getApiCustomersPaged(
                page,
                pageSize,
                keyword || undefined,
                undefined,
                false
            )
        );

        setItems(paged?.items ?? []);
        setTotal(paged?.totalCount ?? 0);
    };

    useEffect(() => {
        load();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [page, pageSize, keyword, token]);

    const handleSave = async (c: Customer) => {
        // 這裡不 catch，讓錯誤丟回 CustomerForm 去顯示欄位錯誤 / Toast
        if (c.id && c.id > 0) {
            await unwrap(
                CustomersService.putApiCustomers(c.id, {
                    code: c.code,
                    name: c.name,
                    contact: c.contact,
                    phone: c.phone,
                })
            );
            notifySuccess("更新成功");
        } else {
            await unwrap(
                CustomersService.postApiCustomers({
                    code: c.code,
                    name: c.name,
                    contact: c.contact,
                    phone: c.phone,
                })
            );
            notifySuccess("新增成功");
        }

        setEditing(null);
        await load();
    };

    const handleDelete = async (id: number) => {
        try {
            await unwrap(CustomersService.deleteApiCustomers(id));
            notifySuccess("刪除成功");
            await load();
        } catch (error) {
            console.error(error);
            handleApiError(error, "刪除失敗，請稍後再試");
        }
    };

    const handleNew = () => {
        const empty: Customer = {
            id: 0,
            code: "",
            name: "",
            contact: "",
            phone: "",
        };
        setEditing(empty);
    };

    const handleEdit = (c: Customer) => {
        setEditing(c);
    };

    const totalPages = Math.max(1, Math.ceil(total / pageSize));

    return (
        <div style={{ padding: 16 }}>
            <h2>客戶管理</h2>

            <div style={{ marginBottom: 12 }}>
                <input
                    placeholder="輸入關鍵字（Code / Name）"
                    value={keyword}
                    onChange={(e) => {
                        setPage(1);
                        setKeyword(e.target.value);
                    }}
                    style={{ marginRight: 8 }}
                />
                <button onClick={() => setPage(1)}>查詢</button>
                <button
                    onClick={handleNew}
                    style={{ marginLeft: 12 }}
                    disabled={!isAdmin}
                >
                    新增客戶
                </button>
                {!isAdmin && (
                    <span style={{ marginLeft: 8, fontSize: 12, color: "#666" }}>
                        只有 Admin 可以新增 / 刪除客戶
                    </span>
                )}
            </div>

            {editing && (
                <CustomerForm
                    initial={editing}
                    onCancel={() => setEditing(null)}
                    onSave={handleSave}
                />
            )}

            <table
                style={{
                    marginTop: 16,
                    width: "100%",
                    borderCollapse: "collapse",
                }}
            >
                <thead>
                    <tr>
                        <th style={{ borderBottom: "1px solid #ccc" }}>ID</th>
                        <th style={{ borderBottom: "1px solid #ccc" }}>Code</th>
                        <th style={{ borderBottom: "1px solid #ccc" }}>Name</th>
                        <th style={{ borderBottom: "1px solid #ccc" }}>Contact</th>
                        <th style={{ borderBottom: "1px solid #ccc" }}>Phone</th>
                        <th style={{ borderBottom: "1px solid #ccc" }}>操作</th>
                    </tr>
                </thead>
                <tbody>
                    {items.map((c) => (
                        <tr key={c.id}>
                            <td style={{ borderBottom: "1px solid #eee" }}>{c.id}</td>
                            <td style={{ borderBottom: "1px solid #eee" }}>{c.code}</td>
                            <td style={{ borderBottom: "1px solid #eee" }}>{c.name}</td>
                            <td style={{ borderBottom: "1px solid #eee" }}>{c.contact}</td>
                            <td style={{ borderBottom: "1px solid #eee" }}>{c.phone}</td>
                            <td style={{ borderBottom: "1px solid #eee" }}>
                                <button onClick={() => handleEdit(c)}>編輯</button>
                                {isAdmin && c.id != null && (
                                    <DeleteButton
                                        label="刪除"
                                        confirmMessage={`確定刪除客戶「${c.name}」？`}
                                        onConfirm={() => handleDelete(c.id!)}
                                        disabled={!isAdmin}
                                    />
                                )}
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

            <div style={{ marginTop: 12 }}>
                <button
                    disabled={page <= 1}
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                >
                    上一頁
                </button>
                <span style={{ margin: "0 8px" }}>
                    第 {page} / {totalPages} 頁（共 {total} 筆）
                </span>
                <button
                    disabled={page >= totalPages}
                    onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                >
                    下一頁
                </button>
            </div>
        </div>
    );
};

export default CustomerList;
