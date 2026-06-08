import { apiClient, fetchBinaryApi } from './api/api-client'
import { showLoading, hideLoading } from '../stores/loading.store'

export function getFactories() {
  return apiClient.get('/WarehouseAccess/Factories/GetFactories')
}

export function getDepartmentsCrud() {
  return apiClient.get('/WarehouseAccess/Departments/GetDepartments')
}
export function createDepartment(payload) {
  return apiClient.post('/WarehouseAccess/Departments/CreateDepartment', payload)
}
export function updateDepartment(payload) {
  return apiClient.put('/WarehouseAccess/Departments/UpdateDepartment', payload)
}
export function deleteDepartment(departmentCode) {
  return apiClient.delete(`/WarehouseAccess/Departments/DeleteDepartment?departmentCode=${encodeURIComponent(departmentCode)}`)
}
export function importDepartmentsExcel(file, loginUserId, companyId) {
  const formData = new FormData()
  formData.append('file', file)
  if (loginUserId) formData.append('loginUserId', loginUserId)
  if (companyId != null && String(companyId).trim()) formData.append('companyId', String(companyId).trim())
  return apiClient.post('/WarehouseAccess/Departments/ImportDepartments', formData)
}
export async function exportDepartmentsTemplateExcel() {
  showLoading()
  try {
    const response = await fetchBinaryApi('/WarehouseAccess/Departments/ExportDepartmentsTemplate', { method: 'GET' })
    if (!response.success) return response
    const blob = response.data.blob
    const fileName = 'departments-template.xlsx'
    return { success: true, data: { blob, fileName } }
  } catch (error) {
    return { success: false, message: error?.message || 'Export template failed' }
  } finally {
    hideLoading()
  }
}

export function getPurposesCrud() {
  return apiClient.get('/WarehouseAccess/Purposes/GetPurposes')
}
export function createPurpose(payload) {
  return apiClient.post('/WarehouseAccess/Purposes/CreatePurpose', payload)
}
export function updatePurpose(payload) {
  return apiClient.put('/WarehouseAccess/Purposes/UpdatePurpose', payload)
}
export function deletePurpose(purposeId) {
  return apiClient.delete(`/WarehouseAccess/Purposes/DeletePurpose?purposeId=${encodeURIComponent(purposeId)}`)
}

export function getUserTypesCrud() {
  return apiClient.get('/WarehouseAccess/UserTypes/GetUserTypes')
}
export function createUserType(payload) {
  return apiClient.post('/WarehouseAccess/UserTypes/CreateUserType', payload)
}
export function updateUserType(payload) {
  return apiClient.put('/WarehouseAccess/UserTypes/UpdateUserType', payload)
}
export function deleteUserType(userTypeId) {
  return apiClient.delete(`/WarehouseAccess/UserTypes/DeleteUserType?userTypeId=${encodeURIComponent(userTypeId)}`)
}

// Factories CRUD
export function getFactoriesCrud() {
  return apiClient.get('/WarehouseAccess/Factories/GetFactories')
}
export function createFactory(payload) {
  return apiClient.post('/WarehouseAccess/Factories/CreateFactory', payload)
}
export function updateFactory(payload) {
  return apiClient.put('/WarehouseAccess/Factories/UpdateFactory', payload)
}
export function deleteFactory(factoryId) {
  return apiClient.delete(`/WarehouseAccess/Factories/DeleteFactory?factoryId=${encodeURIComponent(factoryId)}`)
}
