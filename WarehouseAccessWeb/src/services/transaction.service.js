import { apiClient, fetchBinaryApi } from './api/api-client'
import { showLoading, hideLoading } from '../stores/loading.store'

export function lookupTransactionByCard(cardNumber) {
  return apiClient.post('/WarehouseAccess/Transactions/LookupByCard', { cardNumber })
}

export function createCheckInTransaction(payload) {
  return apiClient.post('/WarehouseAccess/Transactions/CreateCheckIn', payload)
}

export function getLiveTransactions(params = {}) {
  const query = new URLSearchParams()
  if (params.keyword) query.set('keyword', params.keyword)
  if (params.take) query.set('take', String(params.take))
  const qs = query.toString()
  const path = qs
    ? `/WarehouseAccess/Transactions/GetLiveMonitor?${qs}`
    : '/WarehouseAccess/Transactions/GetLiveMonitor'
  return apiClient.get(path)
}

export function getTransactionHistory(params = {}) {
  const query = new URLSearchParams()
  if (params.keyword) query.set('keyword', params.keyword)
  if (params.fromDate) query.set('fromDate', params.fromDate)
  if (params.toDate) query.set('toDate', params.toDate)
  if (params.page) query.set('page', String(params.page))
  if (params.pageSize) query.set('pageSize', String(params.pageSize))
  const qs = query.toString()
  const path = qs
    ? `/WarehouseAccess/Transactions/GetHistory?${qs}`
    : '/WarehouseAccess/Transactions/GetHistory'
  return apiClient.get(path)
}

export function confirmTransactionCheckOut(payload) {
  return apiClient.post('/WarehouseAccess/Transactions/ConfirmCheckOut', payload)
}

export function getTransactionDashboardStats() {
  return apiClient.get('/WarehouseAccess/Transactions/GetDashboardStats')
}

export async function exportTransactionExcel(params = {}) {
  showLoading()
  try {
    const query = new URLSearchParams()
    if (params.keyword) query.set('keyword', params.keyword)
    if (params.fromDate) query.set('fromDate', params.fromDate)
    if (params.toDate) query.set('toDate', params.toDate)

    const qs = query.toString()
    const path = qs
      ? `/WarehouseAccess/Transactions/ExportHistoryExcel?${qs}`
      : '/WarehouseAccess/Transactions/ExportHistoryExcel'

    const response = await fetchBinaryApi(path, { method: 'GET' })
    if (!response.success) return response
    const blob = response.data.blob
    const fileName = 'warehouse-access.xlsx'
    return { success: true, data: { blob, fileName } }
  } catch (error) {
    return { success: false, message: error?.message || 'Export history failed' }
  } finally {
    hideLoading()
  }
}
