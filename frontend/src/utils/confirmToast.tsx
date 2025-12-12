// src/utils/confirmToast.tsx
import toast, { type Toast } from "react-hot-toast";

export function confirmToast(message: string, onConfirm: () => void) {
    toast.custom(
        (t: Toast) => (
            <div
                style={{
                    padding: "16px",
                    background: "white",
                    borderRadius: 6,
                    boxShadow: "0 2px 10px rgba(0,0,0,0.15)",
                    display: "flex",
                    flexDirection: "column",
                    gap: "12px",
                    minWidth: "220px",

                    // 用 t.visible 控制顯示/隱藏動畫
                    opacity: t.visible ? 1 : 0,
                    transform: t.visible ? "translateY(0)" : "translateY(-8px)",
                    transition: "all 100ms ease-out", // 縮短到 150ms
                }}
            >
                <div>{message}</div>

                <div
                    style={{
                        display: "flex",
                        justifyContent: "flex-end",
                        gap: 8,
                    }}
                >
                    <button
                        onClick={() => toast.dismiss(t.id)}
                        style={{ padding: "4px 10px" }}
                    >
                        取消
                    </button>

                    <button
                        onClick={() => {
                            toast.dismiss(t.id);
                            onConfirm();
                        }}
                        style={{
                            padding: "4px 10px",
                            background: "#d9534f",
                            color: "white",
                            borderRadius: 4,
                        }}
                    >
                        確定
                    </button>
                </div>
            </div>
        ),
        { duration: 999999 } // 不自動消失，直到人按確定或取消
    );
}
