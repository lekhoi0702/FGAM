import { APP_CONFIG } from '../../constants/app-config'
import { showLoading, hideLoading } from '../../stores/loading.store'

let serverTimeOffset = 0

export function getAuthHeaders() {
  let authHeaders = {}
  try {
    const authSessionRaw = sessionStorage.getItem('wa_auth_session')
    if (authSessionRaw) {
      const authSession = JSON.parse(authSessionRaw)
      if (authSession?.currentUser?.userId) {
        authHeaders['X-User-Id'] = authSession.currentUser.userId
      }
      if (authSession?.currentUser?.factoryId) {
        authHeaders['X-Factory-Id'] = authSession.currentUser.factoryId
      }
    }
  } catch (error) {
    console.error('Failed to parse auth session in api-client', error)
  }
  return authHeaders
}

export function fetchApi(path, options = {}) {
  return fetch(`${APP_CONFIG.apiBaseUrl}${path}`, {
    ...options,
    headers: {
      ...(options.headers || {}),
      ...getAuthHeaders()
    }
  })
}

export async function fetchBinaryApi(path, options = {}, expectedContentTypes = ['application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'application/octet-stream']) {
  const response = await fetchApi(path, options)
  const contentType = response.headers.get('content-type') || ''

  if (!response.ok) {
    if (contentType.includes('application/json')) {
      try {
        const payload = await response.json()
        return {
          success: false,
          data: null,
          message: payload?.message || `HTTP ${response.status}`
        }
      } catch {
        return {
          success: false,
          data: null,
          message: `HTTP ${response.status}`
        }
      }
    }

    const fallbackText = (await response.text().catch(() => '')).trim()
    return {
      success: false,
      data: null,
      message: fallbackText || `HTTP ${response.status}`
    }
  }

  if (expectedContentTypes.length > 0 && !expectedContentTypes.some((expectedType) => contentType.includes(expectedType))) {
    const preview = (await response.text().catch(() => '')).trim()
    return {
      success: false,
      data: null,
      message: `Expected Excel file but received ${contentType || 'unknown content type'}${preview ? `: ${preview.slice(0, 120)}` : ''}`
    }
  }

  const blob = await response.blob()
  return { success: true, data: { blob } }
}

async function request(path, options = {}) {
  const isNativeAndroidContext =
    typeof window !== 'undefined' &&
    !!window.Capacitor &&
    typeof window.Capacitor.getPlatform === 'function' &&
    window.Capacitor.getPlatform() === 'android'

  if (isNativeAndroidContext && !APP_CONFIG.apiBaseUrl) {
    return {
      success: false,
      data: null,
      message: 'Android API base URL is missing. Set VITE_API_BASE_URL and rebuild/sync Android.'
    }
  }

  const isMutative = ['POST', 'PUT', 'DELETE'].includes(options.method?.toUpperCase())
  if (isMutative && !options.skipGlobalLoading) {
    showLoading()
  }

  const controller = new AbortController()
  const timeout = setTimeout(() => controller.abort(), APP_CONFIG.apiTimeoutMs)

  try {
    const isFormData = options.body instanceof FormData
    const baseHeaders = isFormData ? {} : { 'Content-Type': 'application/json' }

    const response = await fetch(`${APP_CONFIG.apiBaseUrl}${path}`, {
      ...options,
      headers: {
        ...baseHeaders,
        ...getAuthHeaders(),
        ...(options.headers || {})
      },
      signal: controller.signal
    })

    // Parse the server Date header to calculate clock offset (handling out-of-sync TV/client clocks)
    const serverDateHeader = response.headers.get('date')
    if (serverDateHeader) {
      const serverTime = Date.parse(serverDateHeader)
      if (!isNaN(serverTime)) {
        serverTimeOffset = serverTime - Date.now()
      }
    }

    const contentType = response.headers.get('content-type') || ''
    const isJson = contentType.includes('application/json')
    const payload = isJson ? await response.json() : null

    if (!response.ok) {
      return {
        success: false,
        data: null,
        message: payload?.message || `HTTP ${response.status}`
      }
    }

    if (!isJson) {
      return {
        success: false,
        data: null,
        message: 'Expected JSON response but received non-JSON content.'
      }
    }

    return payload
  } catch (error) {
    return {
      success: false,
      data: null,
      message: error?.message || 'Network error'
    }
  } finally {
    clearTimeout(timeout)
    if (isMutative && !options.skipGlobalLoading) {
      hideLoading()
    }
  }
}

export const apiClient = {
  get(path) {
    return request(path, { method: 'GET' })
  },
  post(path, body) {
    const payload = body instanceof FormData ? body : JSON.stringify(body)
    return request(path, { method: 'POST', body: payload })
  },
  put(path, body) {
    return request(path, { method: 'PUT', body: JSON.stringify(body) })
  },
  delete(path) {
    return request(path, { method: 'DELETE' })
  }
}

export function getServerNow() {
  return Date.now() + serverTimeOffset
}
