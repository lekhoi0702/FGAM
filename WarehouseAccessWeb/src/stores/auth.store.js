import { reactive } from 'vue'
import { cardLogin as cardLoginApi, login as loginApi } from '../services/auth.service'

const AUTH_STORAGE_KEY = 'wa_auth_session'

const state = reactive({
  isAuthenticated: false,
  currentUser: null
})

export function useAuthState() {
  return state
}

function persistAuthSession() {
  sessionStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify({
    isAuthenticated: state.isAuthenticated,
    currentUser: state.currentUser
  }))
}
export function restoreSession() {
  try {
    const raw = sessionStorage.getItem(AUTH_STORAGE_KEY)
    if (!raw) return
    const parsed = JSON.parse(raw)
    state.isAuthenticated = !!parsed?.isAuthenticated
    state.currentUser = parsed?.currentUser || null
  } catch {
    state.isAuthenticated = false
    state.currentUser = null
  }
}

export async function login(payload, mode = 'password') {
  const response = mode === 'card'
    ? await cardLoginApi(payload)
    : await loginApi(payload)
  if (response?.success && response.data) {
    state.isAuthenticated = true
    state.currentUser = response.data
    persistAuthSession()
  }
  return response
}

export function logout() {
  state.isAuthenticated = false
  state.currentUser = null
  sessionStorage.removeItem(AUTH_STORAGE_KEY)
}
