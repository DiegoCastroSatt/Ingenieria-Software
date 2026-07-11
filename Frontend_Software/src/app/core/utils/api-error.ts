import { HttpErrorResponse } from '@angular/common/http';

const technicalPatterns = [
  /mysql/i,
  /sql/i,
  /stack/i,
  /exception/i,
  /at\s+\S+\.\S+/i,
  /\/src\//i,
  /\\src\\/i,
  /reserva_cancelaciones/i,
  /doesn't exist/i
];

export function formatApiError(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  const status = error.status || 0;
  const detail = sanitizeMessage(extractServerMessage(error));
  const prefix = status > 0 ? `${fallback} (${status})` : fallback;

  if (!detail) {
    return status === 0
      ? 'No se pudo conectar con el servidor. Revisa tu conexion o intenta mas tarde.'
      : prefix;
  }

  return `${prefix} ${detail}`;
}

function extractServerMessage(error: HttpErrorResponse): string {
  if (typeof error.error === 'string') {
    return error.error;
  }

  if (error.error?.errors) {
    const validationMessage = extractValidationMessage(error.error.errors);
    if (validationMessage) {
      return validationMessage;
    }
  }

  if (error.error?.detail) {
    return String(error.error.detail);
  }

  if (error.error?.title) {
    return String(error.error.title);
  }

  if (error.message) {
    return error.message;
  }

  return '';
}

function extractValidationMessage(errors: Record<string, unknown>): string {
  const messages = Object.values(errors)
    .flatMap((value) => Array.isArray(value) ? value : [value])
    .filter((value): value is string => typeof value === 'string');

  return messages[0] ?? '';
}

function sanitizeMessage(message: string): string {
  const compactMessage = message.replace(/\s+/g, ' ').trim();
  if (!compactMessage) {
    return '';
  }

  if (technicalPatterns.some((pattern) => pattern.test(compactMessage))) {
    return 'Ocurrio un problema interno. Intenta nuevamente mas tarde.';
  }

  return compactMessage.length > 180 ? `${compactMessage.slice(0, 177)}...` : compactMessage;
}
