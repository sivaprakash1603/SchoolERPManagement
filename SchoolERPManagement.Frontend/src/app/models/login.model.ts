export interface LoginRequest {
    username: string;
    password: string;
}

export interface AuthResponse {
    userId: number;
    username: string;
    email: string;
    roleId: number;
    roleName: string;
    accessToken: string;
}
