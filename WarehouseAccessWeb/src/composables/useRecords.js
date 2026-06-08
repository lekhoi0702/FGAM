import { ref, computed } from 'vue'
import { useAuthState } from '../stores/auth.store'
import {
  confirmTransactionCheckOut,
  createCheckInTransaction,
  exportTransactionExcel,
  getTransactionDashboardStats,
  getTransactionHistory,
  getLiveTransactions,
  lookupTransactionByCard
} from '../services/transaction.service'
import {
  createUser,
  deleteUser,
  exportUsersTemplateExcel,
  getDepartments,
  getUsers,
  importUsersExcel,
  updateUser,
  addWhitelistUser,
  getWhitelistUsers,
  importWhitelistExcel,
  exportWhitelistTemplateExcel,
  exportWhitelistExcel,
  deleteWhitelistUser
} from '../services/employee-management.service'
import {
  createDepartment,
  createPurpose,
  createUserType,
  deleteDepartment,
  deletePurpose,
  deleteUserType,
  getDepartmentsCrud,
  getPurposesCrud,
  getUserTypesCrud,
  updateDepartment,
  updatePurpose,
  updateUserType,
  getFactoriesCrud,
  createFactory,
  updateFactory,
  deleteFactory,
  importDepartmentsExcel,
  exportDepartmentsTemplateExcel
} from '../services/master-data.service'

const monitorItems = ref([])
const monitorLoading = ref(false)
const monitorErrorMessage = ref('')

const historyItems = ref([])
const historyLoading = ref(false)
const historyErrorMessage = ref('')
const historyPage = ref(1)
const historyPageSize = ref(10)
const historyTotal = ref(0)
const historyTotalPages = ref(1)
const todayUniqueVisitorsCount = ref(0)
const totalRecordsCount = ref(0)

const userListItems = ref([])
const userDepartmentOptions = ref([])
const usersLoading = ref(false)
const usersErrorMessage = ref('')
const usersTotal = ref(0)

const whitelistUsers = ref([])
const whitelistUsersLoading = ref(false)

const departmentItems = ref([])
const purposeItems = ref([])
const userTypeItems = ref([])
const factoryItems = ref([])
const masterDataErrorMessage = ref('')

export function useRecords() {
  const authState = useAuthState()
  const currentUserId = computed(() => authState.currentUser?.userId || '')

  async function refreshDashboardStats() {
    try {
      const response = await getTransactionDashboardStats()
      if (response?.success && response.data) {
        todayUniqueVisitorsCount.value = response.data.todayUniqueVisitors || 0
        totalRecordsCount.value = response.data.totalRecords || 0
      } else {
        todayUniqueVisitorsCount.value = 0
        totalRecordsCount.value = 0
      }
    } catch {
      todayUniqueVisitorsCount.value = 0
      totalRecordsCount.value = 0
    }
  }

  async function loadLiveMonitor(keyword = '') {
    monitorLoading.value = true
    monitorErrorMessage.value = ''
    try {
      const response = await getLiveTransactions({ keyword: keyword || undefined, take: 200 })
      if (response?.success) {
        monitorItems.value = response.data || []
        await refreshDashboardStats()
      } else {
        monitorErrorMessage.value = response?.message || 'Cannot load live monitor records.'
        monitorItems.value = []
      }
    } catch (e) {
      monitorErrorMessage.value = e.message || 'Error fetching live monitor'
    } finally {
      monitorLoading.value = false
    }
  }

  async function loadHistoryRecords(keyword = '', fromDate = '', toDate = '', page = 1, pageSize = 15) {
    historyLoading.value = true
    historyErrorMessage.value = ''
    try {
      const response = await getTransactionHistory({ 
        keyword: keyword || undefined, 
        fromDate: fromDate || undefined, 
        toDate: toDate || undefined, 
        page, 
        pageSize 
      })
      if (response?.success) {
        historyItems.value = response.data?.items || []
        historyPage.value = Number(response.data?.page) || 1
        historyPageSize.value = Number(response.data?.pageSize) || 10
        historyTotal.value = Number(response.data?.total) || 0
        historyTotalPages.value = Number(response.data?.totalPages) || 1
        await refreshDashboardStats()
      } else {
        historyErrorMessage.value = response?.message || 'Cannot load history records.'
        historyItems.value = []
        historyTotal.value = 0
        historyTotalPages.value = 1
      }
    } catch (e) {
      historyErrorMessage.value = e.message || 'Error fetching history'
    } finally {
      historyLoading.value = false
    }
  }

  async function lookupCard(cardNumber) {
    return lookupTransactionByCard(cardNumber)
  }

  async function submitCheckIn(payload) {
    const res = await createCheckInTransaction({ ...payload, loginUserId: currentUserId.value || undefined })
    if (res?.success) {
      await loadLiveMonitor()
      await loadHistoryRecords()
    }
    return res
  }

  async function submitCheckOut(transactionId) {
    const res = await confirmTransactionCheckOut({ transactionId: transactionId == null ? '' : String(transactionId) })
    if (res?.success) {
      await loadLiveMonitor()
      await loadHistoryRecords()
    }
    return res
  }

  async function exportHistory(keyword = '', fromDate = '', toDate = '') {
    return exportTransactionExcel({ keyword, fromDate, toDate })
  }

  async function loadUserDepartments() {
    const response = await getDepartments()
    userDepartmentOptions.value = response?.success ? (response.data || []) : []
  }

  async function loadUsersList() {
    usersLoading.value = true
    usersErrorMessage.value = ''
    try {
      const response = await getUsers()
      if (response?.success) {
        userListItems.value = (response.data || []).map(e => ({
          userId: e.employeeId,
          fullName: e.employeeName,
          cardNumber: e.cardNumber,
          companyName: e.factoryName, // map factory name to companyName
          factoryId: e.factoryId,
          departmentCode: e.departmentId,
          departmentName: e.departmentName,
          isWhiteList: e.isWhiteList,
          recordStatus: e.recordStatus || e.RecordStatus,
          updatedAt: e.updateDate || e.updatedAt || null,
          userTypeId: e.userTypeId || '1'
        }))
        usersTotal.value = userListItems.value.length
      } else {
        usersErrorMessage.value = response?.message || 'Cannot load users.'
        userListItems.value = []
        usersTotal.value = 0
      }
    } catch (e) {
      usersErrorMessage.value = e.message || 'Error loading users'
    } finally {
      usersLoading.value = false
    }
  }

  async function saveUser(payload, isEdit = false) {
    const normalizedPayload = {
      loginUserId: currentUserId.value,
      userId: payload.userId,
      fullName: payload.fullName,
      departmentCode: payload.departmentCode,
      userTypeId: payload.userTypeId != null ? Number(payload.userTypeId) : null,
      cardNumber: payload.cardNumber,
      password: payload.password,
      isLoginUser: payload.isLoginUser
    }
    const res = isEdit ? await updateUser(normalizedPayload) : await createUser(normalizedPayload)
    if (res?.success) await loadUsersList()
    return res
  }

  async function removeUser(userId) {
    const res = await deleteUser(userId)
    if (res?.success) await loadUsersList()
    return res
  }

  async function registerUserToWhitelist(payload) {
    const res = await addWhitelistUser({
      userId: payload.userId,
      avatar: payload.avatar,
      loginUserId: currentUserId.value || ''
    })
    if (res?.success) {
      await loadUsersList()
      await loadWhitelistUsers()
    }
    return res
  }

  async function loadWhitelistUsers() {
    whitelistUsersLoading.value = true
    try {
      const response = await getWhitelistUsers()
      if (response?.success) {
        whitelistUsers.value = response.data || []
      } else {
        whitelistUsers.value = []
      }
    } catch {
      whitelistUsers.value = []
    } finally {
      whitelistUsersLoading.value = false
    }
  }

  async function importUsers(file) {
    const res = await importUsersExcel(file, currentUserId.value || '')
    if (res?.success) await loadUsersList()
    return res
  }

  async function downloadUsersTemplate() {
    const response = await exportUsersTemplateExcel()
    if (response?.success && response?.data?.blob) {
      const downloadUrl = URL.createObjectURL(response.data.blob)
      const anchorElement = document.createElement('a')
      anchorElement.href = downloadUrl
      anchorElement.download = response.data.fileName || 'users-template.xlsx'
      document.body.appendChild(anchorElement)
      anchorElement.click()
      anchorElement.remove()
      URL.revokeObjectURL(downloadUrl)
      return { success: true }
    }
    return response || { success: false, message: 'Export template failed' }
  }

  async function importWhitelist(file) {
    const res = await importWhitelistExcel(file, currentUserId.value || '')
    if (res?.success) {
      await loadUsersList()
      await loadWhitelistUsers()
    }
    return res
  }

  async function downloadWhitelistTemplate() {
    const response = await exportWhitelistTemplateExcel()
    if (response?.success && response?.data?.blob) {
      const downloadUrl = URL.createObjectURL(response.data.blob)
      const anchorElement = document.createElement('a')
      anchorElement.href = downloadUrl
      anchorElement.download = response.data.fileName || 'whitelist-template.xlsx'
      document.body.appendChild(anchorElement)
      anchorElement.click()
      anchorElement.remove()
      URL.revokeObjectURL(downloadUrl)
      return { success: true }
    }
    return response || { success: false, message: 'Export template failed' }
  }

  async function downloadWhitelistExcel() {
    const response = await exportWhitelistExcel()
    if (response?.success && response?.data?.blob) {
      const downloadUrl = URL.createObjectURL(response.data.blob)
      const anchorElement = document.createElement('a')
      anchorElement.href = downloadUrl
      anchorElement.download = response.data.fileName || 'whitelist.xlsx'
      document.body.appendChild(anchorElement)
      anchorElement.click()
      anchorElement.remove()
      URL.revokeObjectURL(downloadUrl)
      return { success: true }
    }
    return response || { success: false, message: 'Export whitelist failed' }
  }

  async function removeWhitelist(userId) {
    const res = await deleteWhitelistUser({
      userId,
      loginUserId: currentUserId.value || ''
    })
    if (res?.success) {
      await loadUsersList()
      await loadWhitelistUsers()
    }
    return res
  }

  async function loadDepartmentsCrud() {
    masterDataErrorMessage.value = ''
    const response = await getDepartmentsCrud()
    if (response?.success) departmentItems.value = response.data || []
    else {
      masterDataErrorMessage.value = response?.message || 'Cannot load departments.'
      departmentItems.value = []
    }
  }

  async function saveDepartment(payload, isEdit = false) {
    const normalizedPayload = {
      loginUserId: currentUserId.value,
      companyId: payload.companyId == null ? '' : String(payload.companyId).trim(),
      departmentCode: payload.departmentCode,
      departmentName: payload.departmentName
    }
    const res = isEdit ? await updateDepartment(normalizedPayload) : await createDepartment(normalizedPayload)
    if (res?.success) {
      await loadDepartmentsCrud()
      await loadUserDepartments()
    }
    return res
  }

  async function removeDepartment(departmentCode) {
    const res = await deleteDepartment(departmentCode)
    if (res?.success) {
      await loadDepartmentsCrud()
      await loadUserDepartments()
    }
    return res
  }

  async function importDepartments(file, companyId) {
    const res = await importDepartmentsExcel(file, currentUserId.value || '', companyId == null ? '' : String(companyId).trim())
    if (res?.success) {
      await loadDepartmentsCrud()
      await loadUserDepartments()
    }
    return res
  }

  async function downloadDepartmentsTemplate() {
    const response = await exportDepartmentsTemplateExcel()
    if (response?.success && response?.data?.blob) {
      const downloadUrl = URL.createObjectURL(response.data.blob)
      const anchorElement = document.createElement('a')
      anchorElement.href = downloadUrl
      anchorElement.download = response.data.fileName || 'departments-template.xlsx'
      document.body.appendChild(anchorElement)
      anchorElement.click()
      anchorElement.remove()
      URL.revokeObjectURL(downloadUrl)
      return { success: true }
    }
    return response || { success: false, message: 'Export template failed' }
  }

  async function loadPurposesCrud() {
    masterDataErrorMessage.value = ''
    purposeItems.value = [
      { purposeId: '1', purposeName: 'Làm việc' },
      { purposeId: '2', purposeName: 'Giao nhận hàng hóa' },
      { purposeId: '3', purposeName: 'Đánh giá / Kiểm toán' },
      { purposeId: '4', purposeName: 'Hội họp' },
      { purposeId: '5', purposeName: 'Phỏng vấn xin việc' }
    ]
  }

  async function savePurpose(payload, isEdit = false) {
    return { success: true }
  }

  async function removePurpose(purposeId) {
    return { success: true }
  }

  async function loadUserTypesCrud() {
    masterDataErrorMessage.value = ''
    userTypeItems.value = [
      { userTypeId: '1', userTypeName: 'Nhân Viên Nội Bộ' },
      { userTypeId: '2', userTypeName: 'Nhà Cung Cấp' },
      { userTypeId: '3', userTypeName: 'Khách Hàng' },
      { userTypeId: '4', userTypeName: 'Kiểm Toán Bên Thứ 3' }
    ]
  }

  async function saveUserType(payload, isEdit = false) {
    return { success: true }
  }

  async function removeUserType(userTypeId) {
    return { success: true }
  }

  async function loadFactoriesCrud() {
    masterDataErrorMessage.value = ''
    const response = await getFactoriesCrud()
    if (response?.success) factoryItems.value = response.data || []
    else {
      masterDataErrorMessage.value = response?.message || 'Cannot load factories.'
      factoryItems.value = []
    }
  }

  async function saveFactory(payload, isEdit = false) {
    const normalizedPayload = {
      loginUserId: currentUserId.value,
      factoryId: payload.factoryId != null ? String(payload.factoryId) : undefined,
      factoryName: payload.factoryName
    }
    const res = isEdit ? await updateFactory(normalizedPayload) : await createFactory(normalizedPayload)
    if (res?.success) await loadFactoriesCrud()
    return res
  }

  async function removeFactory(factoryId) {
    const res = await deleteFactory(factoryId == null ? '' : String(factoryId))
    if (res?.success) await loadFactoriesCrud()
    return res
  }

  const stats = computed(() => ({
    onSite: monitorItems.value.length,
    today: todayUniqueVisitorsCount.value,
    totalRecords: totalRecordsCount.value
  }))

  return {
    monitorItems,
    monitorLoading,
    monitorErrorMessage,
    historyItems,
    historyLoading,
    historyErrorMessage,
    historyPage,
    historyPageSize,
    historyTotal,
    historyTotalPages,
    userListItems,
    userDepartmentOptions,
    usersLoading,
    usersErrorMessage,
    usersTotal,
    departmentItems,
    purposeItems,
    userTypeItems,
    factoryItems,
    masterDataErrorMessage,
    loadLiveMonitor,
    loadHistoryRecords,
    lookupCard,
    submitCheckIn,
    submitCheckOut,
    exportHistory,
    loadUserDepartments,
    loadUsersList,
    saveUser,
    removeUser,
    registerUserToWhitelist,
    whitelistUsers,
    whitelistUsersLoading,
    loadWhitelistUsers,
    importUsers,
    downloadUsersTemplate,
    importWhitelist,
    downloadWhitelistTemplate,
    downloadWhitelistExcel,
    removeWhitelist,
    loadDepartmentsCrud,
    saveDepartment,
    removeDepartment,
    importDepartments,
    downloadDepartmentsTemplate,
    loadPurposesCrud,
    savePurpose,
    removePurpose,
    loadUserTypesCrud,
    saveUserType,
    removeUserType,
    loadFactoriesCrud,
    saveFactory,
    removeFactory,
    stats
  }
}
