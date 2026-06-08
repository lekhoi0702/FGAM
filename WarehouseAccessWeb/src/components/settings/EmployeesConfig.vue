<script setup>
import { ref, reactive, onMounted, computed, watch } from 'vue';
import { useI18n } from '../../composables/useI18n';
import { useRecords } from '../../composables/useRecords';
import { useToast } from '../../composables/useToast';
import { useSweetAlert } from '../../composables/useSweetAlert';
import { formatDateTime24h } from '../../utils/dateTime';
import AppDropdown from '../common/AppDropdown.vue';
import PaginationFooter from '../common/PaginationFooter.vue';
import ModalCloseButton from '../common/ModalCloseButton.vue';

const { t } = useI18n();
const {
  userListItems,
  userDepartmentOptions,
  userTypeItems,
  usersLoading,
  usersErrorMessage,
  usersTotal,
  whitelistUsers,
  loadUserDepartments,
  loadUserTypesCrud,
  loadUsersList,
  loadWhitelistUsers,
  saveUser,
  removeUser,
  registerUserToWhitelist,
  importUsers,
  downloadUsersTemplate,
  importWhitelist,
  downloadWhitelistTemplate,
  downloadWhitelistExcel,
  removeWhitelist
} = useRecords();

const { showToast } = useToast();
const { showError, showConfirm, showImportResult } = useSweetAlert();

const keyword = ref('');
const isUserModalOpen = ref(false);
const isRegisterLoginModalOpen = ref(false);

const userFormMode = ref('create');
const formState = reactive({ userId: '', cardNumber: '', fullName: '', departmentCode: '', userTypeId: '' });
const formErrors = reactive({ userId: '', fullName: '', departmentCode: '', userTypeId: '' });
const registerLoginForm = reactive({ userId: '', password: '' });

const importingUsers = ref(false);
const importFileInputRef = ref(null);

onMounted(async () => {
  await loadUserDepartments();
  await loadUserTypesCrud();
  await loadUsersList();
});

const realtimeFilteredUsers = computed(() => {
  const searchKeyword = (keyword.value || '').toLowerCase();
  if (!searchKeyword) return userListItems.value;

  return userListItems.value.filter((userRecord) => {
    const userIdText = (userRecord.userId || '').toLowerCase();
    const cardNumberText = (userRecord.cardNumber || '').toLowerCase();
    const fullNameText = (userRecord.fullName || '').toLowerCase();
    const companyText = (userRecord.companyName || '').toLowerCase();
    const userTypeNameText = (userRecord.userTypeName || '').toLowerCase();
    const departmentText = (userRecord.departmentName || userRecord.departmentCode || '').toLowerCase();
    return (
      userIdText.includes(searchKeyword) ||
      cardNumberText.includes(searchKeyword) ||
      fullNameText.includes(searchKeyword) ||
      companyText.includes(searchKeyword) ||
      userTypeNameText.includes(searchKeyword) ||
      departmentText.includes(searchKeyword)
    );
  });
});

// Pagination
const currentPage = ref(1);
const pageSize = ref(10);
const totalItems = computed(() => realtimeFilteredUsers.value.length);
const paginatedUsers = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return realtimeFilteredUsers.value.slice(start, start + pageSize.value);
});
watch([keyword, userListItems], () => {
  currentPage.value = 1;
});

function resetUserForm() {
  userFormMode.value = 'create';
  formState.userId = '';
  formState.cardNumber = '';
  formState.fullName = '';
  formState.departmentCode = '';
  formState.userTypeId = '';
  formErrors.userId = '';
  formErrors.fullName = '';
  formErrors.departmentCode = '';
  formErrors.userTypeId = '';
  isUserWhitelisted.value = false;
}

function openCreateUserModal() {
  resetUserForm();
  userFormMode.value = 'create';
  isUserModalOpen.value = true;
}

function openCreateLoginUserModal() {
  registerLoginForm.userId = '';
  registerLoginForm.password = '';
  isRegisterLoginModalOpen.value = true;
}

// Close user modal and also reset file inputs if needed
function closeUserModal() {
  isUserModalOpen.value = false;
}

function validate() {
  formErrors.userId = formState.userId.trim() ? '' : 'Required';
  formErrors.fullName = formState.fullName.trim() ? '' : 'Required';
  formErrors.departmentCode = formState.departmentCode ? '' : 'Required';
  formErrors.userTypeId = formState.userTypeId ? '' : 'Required';
  return !formErrors.userId && !formErrors.fullName && !formErrors.departmentCode && !formErrors.userTypeId;
}

async function onSubmit() {
  if (!validate()) return;

  const payload = {
    userId: formState.userId.trim(),
    cardNumber: formState.cardNumber ? formState.cardNumber.trim() : null,
    fullName: formState.fullName.trim(),
    departmentCode: formState.departmentCode,
    userTypeId: formState.userTypeId
  };

  try {
    const res = await saveUser(payload, userFormMode.value === 'edit');
    if (res?.success) {
      showToast(userFormMode.value === 'edit' ? 'User updated successfully' : 'User created successfully');
      resetUserForm();
      closeUserModal();
      await loadUsersList();
    } else {
      await showError(res?.message || 'Save user failed');
    }
  } catch {
    await showError('Save failed');
  }
}

// Edit User State Tracker for Whitelist Control inside Modal
const isUserWhitelisted = ref(false);
const removingWhitelist = ref(false);

const currentWhitelistAvatar = computed(() => {
  if (userFormMode.value !== 'edit' || !formState.userId) return '';
  const selectedUserId = formState.userId.trim();
  const whitelistUser = whitelistUsers.value.find((item) => (item.userId || '').trim() === selectedUserId);
  return whitelistUser?.avatar || '';
});

async function editItem(item) {
  userFormMode.value = 'edit';
  formState.userId = item.userId || '';
  formState.cardNumber = item.cardNumber || '';
  formState.fullName = item.fullName || '';
  formState.departmentCode = item.departmentCode || '';
  formState.userTypeId = item.userTypeId || '';
  isUserWhitelisted.value = item.isWhiteList || false; // Store whitelist status
  isUserModalOpen.value = true;
  if (isUserWhitelisted.value) {
    await loadWhitelistUsers();
  }
}

function closeRegisterLoginModal() {
  isRegisterLoginModalOpen.value = false;
}

async function submitRegisterLoginUser() {
  const selectedUserId = (registerLoginForm.userId || '').trim();
  const password = (registerLoginForm.password || '').trim();
  if (!selectedUserId) {
    await showError('User ID is required');
    return;
  }
  if (!password) {
    await showError('Password is required');
    return;
  }

  const targetUser = userListItems.value.find((userItem) => (userItem.userId || '').trim() === selectedUserId);
  if (!targetUser) {
    await showError('Selected user not found');
    return;
  }

  const payload = {
    userId: targetUser.userId,
    cardNumber: targetUser.cardNumber || '',
    fullName: targetUser.fullName || '',
    departmentCode: targetUser.departmentCode || '',
    userTypeId: targetUser.userTypeId,
    isLoginUser: true,
    password
  };

  try {
    const response = await saveUser(payload, true);
    if (response?.success) {
      showToast('Login user registered successfully');
      closeRegisterLoginModal();
      await loadUsersList();
    } else {
      await showError(response?.message || 'Register login user failed');
    }
  } catch {
    await showError('Register login user failed');
  }
}

const userDropdownItems = computed(() => {
  return (userListItems.value || []).map((userItem) => ({
    userId: userItem.userId,
    fullName: userItem.fullName,
    label: userItem.fullName ? `${userItem.userId} - ${userItem.fullName}` : userItem.userId
  }));
});

async function deleteItem(item) {
  const isConfirmed = await showConfirm();
  if (!isConfirmed) return;
  try {
    const res = await removeUser(item.userId);
    if (res?.success) {
      showToast('User deleted successfully');
      await loadUsersList();
    } else {
      await showError(res?.message || 'Delete user failed');
    }
  } catch {
    await showError('Delete failed');
  }
}

async function onFileImport(event) {
  const file = event?.target?.files?.[0];
  if (!file) return;

  importingUsers.value = true;

  try {
    const res = await importUsers(file);
    if (res?.success) {
      showToast('Spreadsheet imported successfully');
      if (Array.isArray(res?.data?.errors) && res.data.errors.length > 0) {
        await showImportResult(res.data, {
          title: 'User Import Result',
          itemKey: 'userId',
          itemLabel: 'User ID'
        });
      }
      closeUserModal(); // Close the user creation modal upon successful import
      await loadUsersList();
    } else {
      await showError(res?.message || 'Import failed');
    }
  } catch {
    await showError('Import failed');
  } finally {
    importingUsers.value = false;
    event.target.value = '';
  }
}

function triggerImportUsersFilePicker() {
  importFileInputRef.value?.click();
}

async function handleDownloadTemplate() {
  try {
    const res = await downloadUsersTemplate();
    if (!res.success) {
      await showError(res.message);
    }
  } catch {
    await showError('Template download failed');
  }
}

function formatDateTime(value) {
  return formatDateTime24h(value);
}

// Whitelist configuration
const isWhitelistModalOpen = ref(false);
const whitelistForm = reactive({ userId: '', avatar: '' });
const whitelistImagePreview = ref('');
const whitelistFileRef = ref(null);
const whitelistLoading = ref(false);

const importingWhitelist = ref(false);
const whitelistExcelFileRef = ref(null);

const selectedWhitelistUser = computed(() => {
  const inputId = (whitelistForm.userId || '').trim().toLowerCase();
  if (!inputId) return null;
  return userListItems.value.find(
    (u) => (u.userId || '').trim().toLowerCase() === inputId
  );
});

function openWhitelistModal() {
  whitelistForm.userId = '';
  whitelistForm.avatar = '';
  whitelistImagePreview.value = '';
  isWhitelistModalOpen.value = true;
}

function closeWhitelistModal() {
  isWhitelistModalOpen.value = false;
}

// Image File Picker for manual creation
function triggerWhitelistFilePicker() {
  whitelistFileRef.value?.click();
}

function onWhitelistFileChange(event) {
  const file = event?.target?.files?.[0];
  if (!file) return;

  if (!file.type.startsWith('image/')) {
    showError('Please select an image file.');
    return;
  }

  const reader = new FileReader();
  reader.onload = (e) => {
    const base64Data = e.target.result;
    whitelistImagePreview.value = base64Data;
    whitelistForm.avatar = base64Data;
  };
  reader.readAsDataURL(file);
}

async function submitWhitelistUser() {
  if (!whitelistForm.userId.trim()) {
    await showError('User ID is required');
    return;
  }
  if (!selectedWhitelistUser.value) {
    await showError('Selected user not found');
    return;
  }
  if (!whitelistForm.avatar) {
    await showError('Avatar image is required');
    return;
  }

  whitelistLoading.value = true;
  try {
    const res = await registerUserToWhitelist({
      userId: selectedWhitelistUser.value.userId,
      avatar: whitelistForm.avatar
    });
    if (res?.success) {
      showToast('User added to whitelist successfully');
      closeWhitelistModal();
      await loadUsersList();
    } else {
      await showError(res?.message || 'Failed to add user to whitelist');
    }
  } catch (error) {
    await showError(error?.message || 'Error saving to whitelist');
  } finally {
    whitelistLoading.value = false;
  }
}

// Excel Import/Export Handlers for Whitelist
function triggerWhitelistExcelFilePicker() {
  whitelistExcelFileRef.value?.click();
}

async function onWhitelistExcelImport(event) {
  const file = event?.target?.files?.[0];
  if (!file) return;

  importingWhitelist.value = true;
  try {
    const res = await importWhitelist(file);
    if (res?.success) {
      showToast('Whitelist imported successfully');
      if (Array.isArray(res?.data?.errors) && res.data.errors.length > 0) {
        await showImportResult(res.data, {
          title: 'Whitelist Import Result',
          itemKey: 'userId',
          itemLabel: 'User ID'
        });
      }
      closeWhitelistModal();
    } else {
      await showError(res?.message || 'Import failed');
    }
  } catch {
    await showError('Import failed');
  } finally {
    importingWhitelist.value = false;
    event.target.value = '';
  }
}

async function handleDownloadWhitelistTemplate() {
  try {
    const res = await downloadWhitelistTemplate();
    if (!res.success) {
      await showError(res.message);
    }
  } catch {
    await showError('Template download failed');
  }
}

// Export Whitelist (with pictures) handler
async function handleExportWhitelist() {
  try {
    const res = await downloadWhitelistExcel();
    if (!res.success) {
      await showError(res.message);
    }
  } catch {
    await showError('Export whitelist failed');
  }
}

// Remove User from Whitelist handler
async function handleRemoveFromWhitelist() {
  const isConfirmed = await showConfirm(
    'Are you sure you want to remove this user from the whitelist?',
    'Remove',
    'Cancel'
  );
  if (!isConfirmed) return;

  removingWhitelist.value = true;
  try {
    const res = await removeWhitelist(formState.userId);
    if (res?.success) {
      showToast('User removed from whitelist successfully');
      isUserWhitelisted.value = false;
    } else {
      await showError(res?.message || 'Failed to remove user from whitelist');
    }
  } catch (error) {
    await showError(error?.message || 'Error removing from whitelist');
  } finally {
    removingWhitelist.value = false;
  }
}
</script>

<template>
  <div class="space-y-6 text-left ui-text-sm">
    <div class="bg-white rounded-2xl border border-slate-200/80 shadow-sm overflow-hidden glassmorphism">
      <div class="flex flex-col sm:flex-row gap-3 items-center justify-between p-4 bg-slate-50/70 border-b border-slate-100">
        <div class="flex flex-wrap items-center gap-3 w-full">
          <input
            type="text"
            v-model="keyword"
            placeholder="Search users..."
            class="bg-white border border-slate-200 rounded-lg px-4 py-2.5 ui-input outline-none focus:border-primary/50 min-w-[320px] sm:min-w-[420px] lg:min-w-[560px]"
          />
          <button class="bg-emerald-600 text-white ui-btn px-4 py-2 rounded-lg hover:bg-emerald-700 transition active:scale-95" @click="openCreateUserModal">Add New</button>
          <button class="bg-primary text-white ui-btn px-4 py-2 rounded-lg hover:bg-primary-dark transition active:scale-95" @click="openCreateLoginUserModal">Register Login User</button>
          
          <button class="bg-indigo-600 text-white ui-btn px-4 py-2 rounded-lg hover:bg-indigo-700 transition active:scale-95" @click="openWhitelistModal">Add to WhiteList</button>
          <button class="bg-white hover:bg-slate-50 border border-indigo-200 text-indigo-700 ui-btn px-4 py-2 rounded-lg transition active:scale-95 flex items-center gap-1.5 shadow-sm font-semibold" @click="handleExportWhitelist">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
            </svg>
            Export Whitelist Excel
          </button>

          <!-- Hidden Input for User Excel Import -->
          <input ref="importFileInputRef" type="file" accept=".xlsx" @change="onFileImport" :disabled="importingUsers" class="hidden" />

          <button class="bg-white hover:bg-slate-100 border border-slate-200 text-slate-700 ui-btn px-4 py-2 rounded-lg active:scale-95 transition" @click="loadUsersList()">Refresh</button>
        </div>
      </div>

      <p v-if="usersErrorMessage" class="p-4 ui-text-sm text-red-500 font-semibold">{{ usersErrorMessage }}</p>

      <div class="overflow-x-auto w-full">
        <table class="w-full text-left border-collapse ui-table">
          <thead>
            <tr class="border-b border-slate-200 font-bold">
              <th class="p-4">User ID</th>
              <th class="p-4">Full Name</th>
              <th class="p-4">Card Number</th>
              <th class="p-4">Department</th>
              <th class="p-4">Updated At</th>
              <th class="p-4">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr v-if="usersLoading && userListItems.length === 0">
              <td colspan="6" class="p-8 text-center text-slate-400 font-semibold">
                <div class="w-5 h-5 border-2 border-slate-200 border-t-primary rounded-full animate-spin mx-auto mb-2"></div>
                Loading users directory...
              </td>
            </tr>
            <tr v-else-if="paginatedUsers.length === 0">
              <td colspan="6" class="p-8 text-center text-slate-400 font-semibold">No users found.</td>
            </tr>
            <tr v-else  v-for="user in paginatedUsers" :key="user.userId">
              <td class="p-4">
                <div class="flex items-center gap-2">
                  <span class="text-slate-700 font-semibold">{{ user.userId }}</span>
                  <span v-if="user.isWhiteList" class="bg-indigo-50 text-indigo-700 border border-indigo-200/50 text-[10px] font-bold px-2.5 py-0.5 rounded-full">Whitelist</span>
                </div>
              </td>
              <td class="p-4"><strong class="text-slate-800">{{ user.fullName || '-' }}</strong></td>
              <td class="p-4 text-slate-600 font-medium">{{ user.cardNumber || '-' }}</td>
              <td class="p-4 text-slate-500 font-semibold">{{ user.departmentName || user.departmentCode || '-' }}</td>
              <td class="p-4 text-slate-400 font-medium">{{ user.updatedAt ? formatDateTime(user.updatedAt) : '-' }}</td>
              <td class="p-4">
                <div class="flex gap-2">
                  <button class="text-indigo-500 hover:text-indigo-700 hover:bg-indigo-50 p-1.5 rounded-lg transition" @click="editItem(user)" title="Edit">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                    </svg>
                  </button>
                  <button class="text-red-400 hover:text-red-600 hover:bg-red-50 p-1.5 rounded-lg transition" @click="deleteItem(user)" title="Delete">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <PaginationFooter :current-page="currentPage" :page-size="pageSize" :total-items="totalItems" @update:current-page="currentPage = $event" />
    </div>

    <!-- Add/Edit User Modal -->
    <div v-if="isUserModalOpen" class="fixed inset-0 z-[70] flex items-center justify-center bg-slate-900/45 p-4">
      <div class="w-full max-w-2xl rounded-2xl border border-slate-200 bg-white shadow-2xl">
        <div class="flex items-center justify-between border-b border-slate-100 px-5 py-4">
          <h3 class="m-0 text-base font-extrabold text-slate-800">{{ userFormMode === 'create' ? 'Add New User' : 'Edit User' }}</h3>
          <ModalCloseButton @click="closeUserModal" />
        </div>

        <div class="p-5">
          <div :class="userFormMode === 'edit' && currentWhitelistAvatar ? 'grid grid-cols-1 gap-4 md:grid-cols-[7rem_1fr] md:items-start' : ''">
            <div
              v-if="userFormMode === 'edit' && currentWhitelistAvatar"
              class="flex justify-center md:justify-start"
            >
              <img
                :src="`data:image/jpeg;base64,${currentWhitelistAvatar}`"
                alt="Whitelist avatar"
                class="h-32 w-28 rounded-2xl border border-slate-200 object-cover shadow-sm"
              />
            </div>

            <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <div class="flex flex-col gap-1 text-left">
              <label class="text-xs font-bold text-slate-500">User ID</label>
              <input
                type="text"
                v-model="formState.userId"
                :disabled="userFormMode === 'edit'"
                class="w-full bg-white border border-slate-200 rounded-xl px-4 py-3 md:py-3.5 text-sm md:text-base outline-none focus:border-[#0e4391] focus:ring-4 focus:ring-[#0e4391]/10 focus:shadow-[0_0_15px_rgba(14,67,145,0.15)] disabled:opacity-50 transition-all duration-300"
              />
              <p v-if="formErrors.userId" class="text-[10px] text-red-500 font-semibold pl-1">{{ formErrors.userId }}</p>
            </div>

            <div class="flex flex-col gap-1 text-left">
              <label class="text-xs font-bold text-slate-500">Card Number</label>
              <input
                type="text"
                v-model="formState.cardNumber"
                class="w-full bg-white border border-slate-200 rounded-xl px-4 py-3 md:py-3.5 text-sm md:text-base outline-none focus:border-[#0e4391] focus:ring-4 focus:ring-[#0e4391]/10 focus:shadow-[0_0_15px_rgba(14,67,145,0.15)] transition-all duration-300"
              />
            </div>

            <div class="flex flex-col gap-1 text-left">
              <label class="text-xs font-bold text-slate-500">Full Name</label>
              <input
                type="text"
                v-model="formState.fullName"
                class="w-full bg-white border border-slate-200 rounded-xl px-4 py-3 md:py-3.5 text-sm md:text-base outline-none focus:border-[#0e4391] focus:ring-4 focus:ring-[#0e4391]/10 focus:shadow-[0_0_15px_rgba(14,67,145,0.15)] transition-all duration-300"
              />
              <p v-if="formErrors.fullName" class="text-[10px] text-red-500 font-semibold pl-1">{{ formErrors.fullName }}</p>
            </div>

            <div class="flex flex-col gap-1 text-left relative z-30">
              <label class="text-xs font-bold text-slate-500">Department</label>
              <AppDropdown
                v-model="formState.departmentCode"
                :items="userDepartmentOptions"
                value-key="departmentCode"
                label-key="departmentName"
                placeholder="Select department..."
                search-placeholder="Search department..."
                :searchable="true"
                :clearable="true"
              />
              <p v-if="formErrors.departmentCode" class="text-[10px] text-red-500 font-semibold pl-1">{{ formErrors.departmentCode }}</p>
            </div>

            </div>
          </div>

          <!-- Excel Section for Users (Only show in Create Mode) -->
          <div v-if="userFormMode === 'create'" class="space-y-3 pt-3">
            <div class="relative flex py-2 items-center">
              <div class="flex-grow border-t border-slate-200"></div>
              <span class="flex-shrink mx-4 text-xs font-bold text-slate-400 uppercase tracking-wider">Or Import via Excel</span>
              <div class="flex-grow border-t border-slate-200"></div>
            </div>
            
            <div class="flex flex-wrap items-center gap-3">
              <button 
                type="button"
                class="bg-primary/10 hover:bg-primary/20 text-primary border border-primary/20 ui-btn px-4 py-2.5 rounded-xl transition active:scale-95 font-semibold text-xs flex items-center gap-1.5 shadow-sm disabled:opacity-50" 
                :disabled="importingUsers"
                @click="triggerImportUsersFilePicker"
              >
                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
                </svg>
                {{ importingUsers ? 'Importing...' : 'Import Users Excel' }}
              </button>

              <button 
                type="button"
                class="bg-white hover:bg-slate-50 border border-slate-200 text-slate-700 ui-btn px-4 py-2.5 rounded-xl transition active:scale-95 font-semibold text-xs flex items-center gap-1.5 shadow-sm" 
                @click="handleDownloadTemplate"
              >
                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                </svg>
                Export Excel Template
              </button>
            </div>
          </div>
        </div>

        <div class="flex items-center justify-end gap-2 border-t border-slate-100 px-5 py-4">
          <button 
            v-if="userFormMode === 'edit' && isUserWhitelisted"
            type="button"
            class="bg-red-50 hover:bg-red-100 border border-red-200 text-red-700 ui-btn px-4 py-2.5 rounded-lg transition active:scale-95 text-xs font-bold" 
            :disabled="removingWhitelist"
            @click="handleRemoveFromWhitelist"
          >
            {{ removingWhitelist ? 'Removing...' : 'Remove Whitelist' }}
          </button>
          <button class="bg-primary text-white ui-btn px-4 py-2.5 rounded-lg hover:bg-primary-dark transition active:scale-95 shadow-md shadow-primary/10" @click="onSubmit">
            {{ userFormMode === 'create' ? 'Add User' : 'Save Changes' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Register Login User Modal -->
    <div v-if="isRegisterLoginModalOpen" class="fixed inset-0 z-[70] flex items-center justify-center bg-slate-900/45 p-4">
      <div class="w-full max-w-xl rounded-2xl border border-slate-200 bg-white shadow-2xl">
        <div class="flex items-center justify-between border-b border-slate-100 px-5 py-4">
          <h3 class="m-0 text-base font-extrabold text-slate-800">Register Login User</h3>
          <ModalCloseButton @click="closeRegisterLoginModal" />
        </div>
        <div class="space-y-3 p-5">
          <div class="flex flex-col gap-1 text-left">
            <label class="text-xs font-bold text-slate-500">User ID</label>
            <AppDropdown
              v-model="registerLoginForm.userId"
              :items="userDropdownItems"
              value-key="userId"
              label-key="label"
              placeholder="Select user..."
              search-placeholder="Search user..."
              :searchable="true"
              :clearable="true"
            />
          </div>
          <div class="flex flex-col gap-1 text-left">
            <label class="text-xs font-bold text-slate-500">Password</label>
            <input
              type="password"
              v-model="registerLoginForm.password"
              class="w-full bg-white border border-slate-200 rounded-xl px-4 py-3 md:py-3.5 text-sm md:text-base outline-none focus:border-[#0e4391] focus:ring-4 focus:ring-[#0e4391]/10 focus:shadow-[0_0_15px_rgba(14,67,145,0.15)] transition-all duration-300"
              placeholder="Enter password"
            />
          </div>
        </div>
        <div class="flex items-center justify-end gap-2 border-t border-slate-100 px-5 py-4">
          <button class="bg-primary text-white ui-btn px-4 py-2.5 rounded-lg hover:bg-primary-dark transition active:scale-95 shadow-md shadow-primary/10" @click="submitRegisterLoginUser">
            Save Login
          </button>
        </div>
      </div>
    </div>

    <!-- Whitelist Modal -->
    <div v-if="isWhitelistModalOpen" class="fixed inset-0 z-[70] flex items-center justify-center bg-slate-900/45 p-4">
      <div class="w-full max-w-xl rounded-2xl border border-slate-200 bg-white shadow-2xl">
        <div class="flex items-center justify-between border-b border-slate-100 px-5 py-4">
          <h3 class="m-0 text-base font-extrabold text-slate-800">Add to WhiteList</h3>
          <ModalCloseButton @click="closeWhitelistModal" />
        </div>

        <div class="space-y-4 p-5 text-left">
          <!-- User ID Input -->
          <div class="flex flex-col gap-1">
            <label class="text-xs font-bold text-slate-500">User ID</label>
            <input
              type="text"
              v-model="whitelistForm.userId"
              class="w-full bg-white border border-slate-200 rounded-xl px-4 py-3 md:py-3.5 text-sm md:text-base outline-none focus:border-[#0e4391] focus:ring-4 focus:ring-[#0e4391]/10 focus:shadow-[0_0_15px_rgba(14,67,145,0.15)] transition-all duration-300"
              placeholder="Type User ID..."
            />
          </div>

          <!-- Lookup details -->
          <div v-if="whitelistForm.userId.trim()">
            <div v-if="selectedWhitelistUser" class="bg-indigo-50/50 border border-indigo-100 rounded-2xl p-4 space-y-2">
              <div class="flex justify-between items-center border-b border-indigo-100/50 pb-2">
                <span class="text-xs font-bold text-indigo-600 tracking-wider uppercase">User Resolved</span>
                <span v-if="selectedWhitelistUser.isWhiteList" class="bg-emerald-50 text-emerald-700 border border-emerald-200/50 text-[10px] font-bold px-2 py-0.5 rounded-full">Already Whitelisted</span>
              </div>
              <div class="grid grid-cols-2 gap-x-4 gap-y-2 text-sm text-slate-600">
                <div>
                  <span class="block text-[10px] font-bold text-slate-400 uppercase">Full Name</span>
                  <span class="font-semibold text-slate-800">{{ selectedWhitelistUser.fullName || '-' }}</span>
                </div>
                <div>
                  <span class="block text-[10px] font-bold text-slate-400 uppercase">Card Number</span>
                  <span class="font-semibold text-slate-800">{{ selectedWhitelistUser.cardNumber || '-' }}</span>
                </div>
                <div>
                  <span class="block text-[10px] font-bold text-slate-400 uppercase">Department</span>
                  <span class="font-semibold text-slate-800">{{ selectedWhitelistUser.departmentName || selectedWhitelistUser.departmentCode || '-' }}</span>
                </div>
                <div>
                  <span class="block text-[10px] font-bold text-slate-400 uppercase">Company</span>
                  <span class="font-semibold text-slate-800">{{ selectedWhitelistUser.companyName || '-' }}</span>
                </div>
              </div>
            </div>
            <div v-else class="bg-red-50 border border-red-100 text-red-700 rounded-2xl p-3.5 text-sm flex items-center gap-2">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-red-500 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
              </svg>
              <span>User with ID "{{ whitelistForm.userId }}" not found.</span>
            </div>
          </div>

          <!-- Avatar Image selection -->
          <div class="flex flex-col gap-1">
            <label class="text-xs font-bold text-slate-500">Avatar Image</label>
            <input
              ref="whitelistFileRef"
              type="file"
              accept="image/*"
              class="hidden"
              @change="onWhitelistFileChange"
            />
            
            <div class="flex items-center gap-4 mt-1">
              <!-- Preview Frame -->
              <div class="w-20 h-20 bg-slate-50 border border-slate-200 rounded-2xl overflow-hidden flex items-center justify-center shrink-0">
                <img v-if="whitelistImagePreview" :src="whitelistImagePreview" class="w-full h-full object-cover" />
                <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-slate-300" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M5.121 17.804A13.937 13.937 0 0112 16c2.5 0 4.847.655 6.879 1.804M15 10a3 3 0 11-6 0 3 3 0 016 0zm6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>

              <!-- Action buttons -->
              <div class="flex flex-col gap-2">
                <button
                  type="button"
                  class="bg-white hover:bg-slate-50 border border-slate-200 text-slate-700 text-sm font-semibold px-4 py-2 rounded-xl transition active:scale-95 shadow-sm"
                  @click="triggerWhitelistFilePicker"
                >
                  Browse Image
                </button>
                <span class="text-[10px] text-slate-400">Supported formats: JPEG, PNG. Max size: 2MB.</span>
              </div>
            </div>
          </div>

          <!-- Excel Section Divider -->
          <div class="relative flex py-2 items-center">
            <div class="flex-grow border-t border-slate-200"></div>
            <span class="flex-shrink mx-4 text-xs font-bold text-slate-400 uppercase tracking-wider">Or Import via Excel</span>
            <div class="flex-grow border-t border-slate-200"></div>
          </div>

          <!-- Excel Actions -->
          <div class="flex flex-wrap items-center gap-3">
            <input ref="whitelistExcelFileRef" type="file" accept=".xlsx" @change="onWhitelistExcelImport" :disabled="importingWhitelist" class="hidden" />
            
            <button 
              type="button"
              class="bg-indigo-50 hover:bg-indigo-100 text-indigo-700 border border-indigo-200/50 ui-btn px-4 py-2.5 rounded-xl transition active:scale-95 font-semibold text-xs flex items-center gap-1.5 shadow-sm disabled:opacity-50" 
              :disabled="importingWhitelist"
              @click="triggerWhitelistExcelFilePicker"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
              </svg>
              {{ importingWhitelist ? 'Importing...' : 'Import Whitelist Excel' }}
            </button>

            <button 
              type="button"
              class="bg-white hover:bg-slate-50 border border-slate-200 text-slate-700 ui-btn px-4 py-2.5 rounded-xl transition active:scale-95 font-semibold text-xs flex items-center gap-1.5 shadow-sm" 
              @click="handleDownloadWhitelistTemplate"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
              </svg>
              Export Excel Template
            </button>
          </div>
        </div>

        <div class="flex items-center justify-end gap-2 border-t border-slate-100 px-5 py-4">
          <button
            class="bg-primary text-white ui-btn px-4 py-2.5 rounded-lg hover:bg-primary-dark transition active:scale-95 shadow-md shadow-primary/10 disabled:opacity-50 disabled:cursor-not-allowed"
            :disabled="!selectedWhitelistUser || !whitelistForm.avatar || whitelistLoading"
            @click="submitWhitelistUser"
          >
            {{ whitelistLoading ? 'Saving...' : 'Confirm' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
</style>
