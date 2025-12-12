// src/features/customers/CustomerForm.tsx
import { useEffect, useState } from "react";
import type { CustomerDto } from "../../api/generated/models/CustomerDto";
import { parseValidationErrors } from "../../api/apiClient";
import { notifyError } from "../../utils/notify";

type Customer = CustomerDto;

type ValidationErrors = Record<string, string[]>;

interface Props {
    initial: CustomerDto;
    onSave: (c: CustomerDto) => Promise<void>; // 確保是 async
    onCancel: () => void;
}

const CustomerForm = ({ initial, onSave, onCancel }: Props) => {
    const [form, setForm] = useState<Customer>(initial);
    const [errors, setErrors] = useState<ValidationErrors>({});
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        setForm(initial);
        setErrors({});
    }, [initial]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;

        setForm((prev: Customer) => ({
            ...prev,
            [name]: value,
        }));

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
                <label style={{ display: "inline-block", width: 80 }}>Contact：</label>
                <input
                    name="contact"
                    value={form.contact ?? ""}
                    onChange={handleChange}
                />
                {renderFieldError("Contact")}
            </div>

            <div style={{ marginBottom: 8 }}>
                <label style={{ display: "inline-block", width: 80 }}>Phone：</label>
                <input
                    name="phone"
                    value={form.phone ?? ""}
                    onChange={handleChange}
                />
                {renderFieldError("Phone")}
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

export default CustomerForm;
