export type RegisterPayload = {
  nombre: string;
  rut: string;
  correo: string;
  password: string;
  rol?: string;
};

export type AuthUser = {
  id: number;
  nombre: string;
  correo: string;
  edad: number;
  rol: string;
};

export type AuthResponse = {
  message: string;
  user: AuthUser;
  token: string;
};
