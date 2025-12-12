// src/components/DeleteButton.tsx
import { useState } from "react";
import { confirmToast } from "../utils/confirmToast";

interface DeleteButtonProps {
    /** 按鈕文字，預設 "刪除" */
    label?: string;
    /** 確認視窗顯示的訊息 */
    confirmMessage?: string;
    /** 確認後要執行的刪除邏輯（可回傳 Promise） */
    onConfirm: () => Promise<void> | void;
    /** 是否禁用按鈕（例如非 Admin） */
    disabled?: boolean;
}

const DeleteButton: React.FC<DeleteButtonProps> = ({
    label = "刪除",
    confirmMessage = "確定刪除？",
    onConfirm,
    disabled = false,
}) => {
    const [pending, setPending] = useState(false);

    const handleClick = () => {
        if (disabled || pending) return;

        confirmToast(confirmMessage, async () => {
            try {
                setPending(true);
                await onConfirm();
            } finally {
                setPending(false);
            }
        });
    };

    return (
        <button onClick={handleClick} disabled={disabled || pending}>
            {pending ? "處理中..." : label}
        </button>
    );
};

export default DeleteButton;
