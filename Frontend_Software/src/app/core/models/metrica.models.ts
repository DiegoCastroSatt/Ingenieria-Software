export type Metrica = { idMetrica: number; idUsuario: number; ejercicio: string; pesoKg: number; fecha: string; notas?: string | null; fechaCreacion: string };
export type CrearMetricaPayload = { idUsuario: number; ejercicio: string; pesoKg: number; fecha: string; notas?: string | null };
