import { apiClient, fetchBinaryApi } from './api/api-client'
import { showLoading, hideLoading } from '../stores/loading.store'

export function getDepartments() {
  return apiClient.get('/WarehouseAccess/Departments/GetDepartments')
}

export function getUsers() {
  return apiClient.get('/WarehouseAccess/Employees/GetUsers')
}

export function createUser(payload) {
  return apiClient.post('/WarehouseAccess/Employees/CreateUser', payload)
}

export function updateUser(payload) {
  return apiClient.put('/WarehouseAccess/Employees/UpdateUser', payload)
}

export function deleteUser(userId) {
  return apiClient.delete(`/WarehouseAccess/Employees/DeleteUser?userId=${encodeURIComponent(userId)}`)
}

export function importUsersExcel(file, loginUserId) {
  const formData = new FormData()
  formData.append('file', file)
  if (loginUserId) formData.append('loginUserId', loginUserId)
  return apiClient.post('/WarehouseAccess/Employees/ImportUsers', formData)
}

export async function exportUsersTemplateExcel() {
  showLoading()
  try {
    const response = await fetchBinaryApi('/WarehouseAccess/Employees/ExportUsersTemplate', { method: 'GET' })
    if (!response.success) return response
    const blob = response.data.blob
    const fileName = 'users-template.xlsx'
    return { success: true, data: { blob, fileName } }
  } catch (error) {
    return { success: false, message: error?.message || 'Export template failed' }
  } finally {
    hideLoading()
  }
}

export function addWhitelistUser(payload) {
  return apiClient.post('/WarehouseAccess/Employees/AddWhitelistUser', payload)
}

export function getWhitelistUsers() {
  return apiClient.get('/WarehouseAccess/Employees/GetWhitelistUsers')
}

export function importWhitelistExcel(file, loginUserId) {
  const formData = new FormData()
  formData.append('file', file)
  if (loginUserId) formData.append('loginUserId', loginUserId)
  return apiClient.post('/WarehouseAccess/Employees/ImportWhitelist', formData)
}

export async function exportWhitelistTemplateExcel() {
  showLoading()
  try {
    const response = await fetchBinaryApi('/WarehouseAccess/Employees/ExportWhitelistTemplate', { method: 'GET' })
    if (!response.success) return response
    const blob = response.data.blob
    const fileName = 'whitelist-template.xlsx'
    return { success: true, data: { blob, fileName } }
  } catch (error) {
    return { success: false, message: error?.message || 'Export template failed' }
  } finally {
    hideLoading()
  }
}

export async function exportWhitelistExcel() {
  showLoading()
  try {
    const response = await fetchBinaryApi('/WarehouseAccess/Employees/ExportWhitelist', { method: 'GET' })
    if (!response.success) return response
    const blob = response.data.blob
    const fileName = `whitelist-${new Date().toISOString().slice(0, 10)}.xlsx`
    return { success: true, data: { blob, fileName } }
  } catch (error) {
    return { success: false, message: error?.message || 'Export whitelist failed' }
  } finally {
    hideLoading()
  }
}

export function deleteWhitelistUser(payload) {
  return apiClient.post('/WarehouseAccess/Employees/RemoveWhitelistUser', payload)
}
