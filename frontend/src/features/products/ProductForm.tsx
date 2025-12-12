import { useEffect, useState } from "react";
import type { ProductDto } from "../../api/generated/models/ProductDto";
import { parseValidationErrors } from "../../api/apiClient";
import { notifyError } from "../../utils/notify";

// 用後端產生的 ProductDto 當前端的 view model 型別
type Product = ProductDto;

// 後端 ProblemDetails 裡 errors 的型別
type ValidationErrors = Record<string, string[]>;

interface Props {
    initial: Product;
    // onSave 回傳 Promise，方便這裡 await + catch
    onSave: (p: Product) => Promise<void>;
    onCancel: () => void;
}

const ProductForm = ({ initial, onSave, onCancel }: Props) => {
    const [form, setForm] = useState<Product>(initial);
    const [errors, setErrors] = useState<ValidationErrors>({});
    const [submitting, setSubmitting] = useState(false);

    // 當切換編輯的那一筆時，重設表單 + 清錯誤
    useEffect(() => {
        setForm(initial);
        setErrors({});
    }, [initial]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;

        setForm((prev: Product) => ({
            ...prev,
            [name]: name === "price" ? Number(value) : value,
        }));

        // 使用者修改時順便清掉該欄位錯誤
        setErrors((prev: ValidationErrors) => {
            if (!prev[name]) return prev;
            const clone = { ...prev };
            delete clone[name];
            return clone;
        });
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitting(true);
        setErrors({});

        try {
            await onSave(form);
        } catch (error: unknown) {
            const errs = parseValidationErrors(error);

            if (errs) {
                setErrors(errs);
            } else {
                notifyError("儲存時發生錯誤，請稍後再試");
                console.error(error);
            }
        } finally {
            setSubmitting(false);
        }
    };

    const renderFieldError = (field: string) => {
        if (!errors[field]) return null;
        return (
            <div style={{ color: "red", marginTop: 4 }}>
                {errors[field].join("，")}
            </div>
        );
    };

    return (
        <form
            onSubmit={handleSubmit}
            style={{
                marginTop: 16,
                marginBottom: 16,
                padding: 12,
                border: "1px solid #ddd",
            }}
        >
            <div style={{ marginBottom: 8 }}>
                <label style={{ display: "inline-block", width: 80 }}>Code：</label>
                <input
                    name="code"
                    value={form.code ?? ""}
                    onChange={handleChange}
                    required
                />
                {renderFieldError("Code")}
            </div>

            <div style={{ marginBottom: 8 }}>
                <label style={{ display: "inline-block", width: 80 }}>Name：</label>
                <input
                    name="name"
                    value={form.name ?? ""}
                    onChange={handleChange}
                    required
                />
                {renderFieldError("Name")}
            </div>

            <div style={{ marginBottom: 8 }}>
                <label style={{ display: "inline-block", width: 80 }}>Price：</label>
                <input
                    name="price"
                    type="number"
                    value={form.price ?? 0}
                    onChange={handleChange}
                    required
                />
                {renderFieldError("Price")}
            </div>

            <div style={{ marginTop: 8 }}>
                <button type="submit" disabled={submitting}>
                    {submitting ? "儲存中..." : "儲存"}
                </button>
                <button
                    type="button"
                    onClick={onCancel}
                    style={{ marginLeft: 8 }}
                    disabled={submitting}
                >
                    取消
                </button>
            </div>
        </form>
    );
};

export default ProductForm;
