import { useEffect, useState } from "react";
import { ProductsService } from "../../api/generated/services/ProductsService";

import type { ProductDto } from "../../api/generated/models/ProductDto";
import ProductForm from "./ProductForm";
import { unwrap, handleApiError } from "../../api/apiClient";
import { useAuth } from "../../auth/useAuth";
import { notifySuccess } from "../../utils/notify";
import DeleteButton from "../../components/DeleteButton";

// 把 ProductDto 當前端的 Product 型別
type Product = ProductDto;

const ProductList = () => {
    const { role, token } = useAuth();
    const [items, setItems] = useState<Product[]>([]);
    const [editing, setEditing] = useState<Product | null>(null);

    // 分頁查詢參數
    const [page, setPage] = useState(1);
    const [pageSize] = useState(5);
    const [total, setTotal] = useState(0);
    const [keyword, setKeyword] = useState("");

    const isAdmin = role === "Admin";

    const load = async () => {
        // 呼叫後端 paged API → 回傳 ApiResponse<PagedResult<ProductDto>>
        const paged = await unwrap(
            ProductsService.getApiProductsPaged(
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

    const handleSave = async (p: Product): Promise<void> => {
        // 建立 or 更新
        if (typeof p.id === "number" && p.id > 0) {
            await ProductsService.putApiProducts(p.id, {
                code: p.code ?? undefined,
                name: p.name ?? undefined,
                // 只在 price 為 number 時傳入（避免 null 被傳給只允許 number 的欄位）
                price: typeof p.price === "number" ? p.price : undefined,
            });
            notifySuccess("更新成功");
        } else {
            await ProductsService.postApiProducts({
                code: p.code ?? undefined,
                name: p.name ?? undefined,
                price: typeof p.price === "number" ? p.price : undefined,
            });
            notifySuccess("新增成功");
        }

        setEditing(null);
        await load();
    };

    const handleDelete = async (id: number) => {
        try {
            await unwrap(ProductsService.deleteApiProducts(id));
            notifySuccess("刪除成功");
            await load();
        } catch (error) {
            console.error(error);
            handleApiError(error, "刪除失敗，請稍後再試");
        }
    };

    const handleNew = () => {
        const empty: Product = {
            id: 0,
            code: "",
            name: "",
            price: 0,
        };
        setEditing(empty);
    };

    const totalPages = Math.max(1, Math.ceil(total / pageSize));

    return (
        <div style={{ padding: "16px" }}>
            <h2>產品管理（查詢 + 分頁 + 統一格式）</h2>

            {/* 查詢區 */}
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
                <button onClick={handleNew} style={{ marginLeft: 12 }}>
                    新增產品
                </button>
            </div>

            {editing && (
                <ProductForm
                    initial={editing}
                    onCancel={() => setEditing(null)}
                    onSave={handleSave}
                />
            )}

            {/* 列表 */}
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
                        <th style={{ borderBottom: "1px solid #ccc" }}>Price</th>
                        <th style={{ borderBottom: "1px solid #ccc" }}>操作</th>
                    </tr>
                </thead>
                <tbody>
                    {items.map((p) => (
                        <tr key={p.id}>
                            <td style={{ borderBottom: "1px solid #eee" }}>{p.id}</td>
                            <td style={{ borderBottom: "1px solid #eee" }}>{p.code}</td>
                            <td style={{ borderBottom: "1px solid #eee" }}>{p.name}</td>
                            <td style={{ borderBottom: "1px solid #eee" }}>{p.price}</td>
                            <td style={{ borderBottom: "1px solid #eee" }}>
                                <button onClick={() => setEditing(p)}>編輯</button>
                                {isAdmin && p.id != null && (
                                    <DeleteButton
                                        label="刪除"
                                        confirmMessage={`確定刪除產品「${p.name}」？`}
                                        onConfirm={() => handleDelete(p.id!)}
                                        disabled={!isAdmin}
                                    />
                                )}
                            </td>
                        </tr>
                    ))}
                    {items.length === 0 && (
                        <tr>
                            <td colSpan={5} style={{ padding: 8 }}>
                                尚無資料
                            </td>
                        </tr>
                    )}
                </tbody>
            </table>

            {/* 分頁器 */}
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

export default ProductList;
