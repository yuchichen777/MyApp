export interface JwtPayload {
    name?: string;     // ClaimTypes.Name
    role?: string;     // ClaimTypes.Role
    sub?: string;
    exp?: number;
    iss?: string;
    aud?: string;
    [key: string]: unknown; // 允許其他 Claim，但避免 any
}
