import { apiClient } from './api/api-client'

export function login(payload) {
  return apiClient.post('/WarehouseAccess/Auth/Login', payload)
}

export function cardLogin(payload) {
  return apiClient.post('/WarehouseAccess/Auth/CardLogin', payload)
}
