const defaultApiBaseUrl = import.meta.env.DEV ? '' : 'http://localhost:5034/WarehouseAccessAPi'

export const APP_CONFIG = {
  apiBaseUrl: (import.meta.env.VITE_API_BASE_URL ?? defaultApiBaseUrl).trim(),
  apiTimeoutMs: 15000
}
