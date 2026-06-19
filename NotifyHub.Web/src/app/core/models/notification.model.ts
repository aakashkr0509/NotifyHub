export interface Notification {
    id: string;
    tenantId: string;
    userId: string | null;
    title: string;
    body: string;
    status: 'Unread' | 'Read';
    createdAt: string;
}

export interface LoginRequest{
    email: string;
    password: string;
    subdomain: string;
}

export interface AuthResponse{
    accessToken: string;
    refreshToken: string;
    email: string;
    role: string;
    tenantId: string;
}