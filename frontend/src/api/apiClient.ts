// src/api/apiClient.ts
import { notifyError } from "../utils/notify";
import type { CancelablePromise } from "../api/generated/core/CancelablePromise";

// 統一對應後端 ApiResponse<T> 的基本形狀
export interface ApiResponseShape<T = unknown> {
    success: boolean;
    statusCode: number;
    message?: string | null;
    traceId?: string | null;
    data?: T;
    errors?: Record<string, string[]>;
}

// 後端錯誤 body 的型別（ApiErrorResponse）
type ApiErrorResponseBody = {
    statusCode?: number;
    message?: string | null;
    errors?: Record<string, string[]>;
};

// openapi-typescript-codegen 丟出來的錯誤「大概」長這樣
// 我們自己定一個安全的結構來 parse，就不用管 generated 的 ApiError 是什麼型別
interface ApiErrorLike {
    status?: number;
    statusCode?: number;
    body?: ApiErrorResponseBody;
    response?: {
        data?: ApiErrorResponseBody;
    };
}

/**
 * 401 全域處理註冊點
 * 在 AuthProvider 裡呼叫 registerOnUnauthorized(() => { ... })
 */
let onUnauthorized: (() => void) | null = null;

export function registerOnUnauthorized(handler: () => void) {
    onUnauthorized = handler;
}

/**
 * 將 ApiResponse<T> 解包成 T（就是 data）
 * 用法：
 *   const paged = await unwrap(ProductsService.getApiProductsPaged(...));
 *
 * 同時在這裡統一處理 401（Token 過期 / 未登入）
 */
export async function unwrap<TData>(
    promise: CancelablePromise<TData>
): Promise<NonNullable<TData>> {
    try {
        const res = (await promise) as unknown;

        // 1) 如果是我們定義的 ApiResponse<T>（有 data 欄位）
        if (res && typeof res === "object" && "data" in res) {
            const api = res as ApiResponseShape<TData>;
            return api.data as NonNullable<TData>;
        }

        // 2) 否則視為 T 本身（Controller 直接回傳 T）
        return res as NonNullable<TData>;
    } catch (error: unknown) {
        const apiErr = error as ApiErrorLike;

        const status =
            apiErr.status ??
            apiErr.statusCode ??
            apiErr.body?.statusCode ??
            apiErr.response?.data?.statusCode;

        if (status === 401 && onUnauthorized) {
            onUnauthorized();
        }

        // 丟回去給呼叫端（例如 Form 處理驗證錯誤 / handleApiError）
        throw error;
    }
}

/**
 * 從 Error 裡解析出後端回傳的 errors（驗證錯誤）
 * 如果不是驗證錯誤，回傳 null
 */
export function parseValidationErrors(
    error: unknown
): Record<string, string[]> | null {
    const apiErr = error as ApiErrorLike;

    const body = apiErr.body ?? apiErr.response?.data;

    if (!body || !body.errors) {
        return null;
    }

    return body.errors;
}

/**
 * 統一處理非驗證類錯誤 → 根據 status / message 顯示 Toast
 */
export function handleApiError(
    error: unknown,
    fallbackMessage = "發生錯誤，請稍後再試"
): void {
    const apiErr = error as ApiErrorLike;

    const status =
        apiErr.status ??
        apiErr.statusCode ??
        apiErr.body?.statusCode ??
        apiErr.response?.data?.statusCode;

    const msgFromServer =
        apiErr.body?.message ??
        apiErr.response?.data?.message ??
        null;

    const msg = msgFromServer ?? fallbackMessage;

    // 401 建議交給全域登入邏輯處理（onUnauthorized），這裡不再 Toast
    if (status === 401) {
        return;
    }

    if (status === 403) {
        notifyError(msgFromServer ?? "沒有權限執行此操作");
        return;
    }

    if (status === 404) {
        notifyError(msgFromServer ?? "找不到資料");
        return;
    }

    if (status === 409) {
        notifyError(msgFromServer ?? "資料已被使用，無法完成此操作");
        return;
    }

    if (status !== undefined && status >= 500) {
        notifyError(msgFromServer ?? "系統發生未預期錯誤，請聯絡系統管理員");
        return;
    }

    // 其他狀況（含 400 非驗證錯誤）
    notifyError(msg);
}
