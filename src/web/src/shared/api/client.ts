import { config } from "../../app/config";

export class ApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const url = `${config.apiUrl}${path.startsWith("/") ? path : `/${path}`}`;
  const response = await fetch(url, {
    headers: { "Content-Type": "application/json", ...(init?.headers ?? {}) },
    ...init,
  });

  if (!response.ok) {
    throw new ApiError(response.status, `${response.status} ${response.statusText}`);
  }

  return (await response.json()) as T;
}
