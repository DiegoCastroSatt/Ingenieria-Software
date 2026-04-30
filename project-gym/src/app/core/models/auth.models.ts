export type RegisterPayload = {
  nombre: string;
  rut: string;
  correo: string;
  password: string;
};

export type AuthUser = {
  id: number;
  nombre: string;
  correo: string;
};

export type AuthResponse = {
  message: string;
  user: AuthUser;
};

export type HealthResponse = {
  status: string;
  message: string;
};
