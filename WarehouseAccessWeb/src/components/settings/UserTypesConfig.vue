<script setup>
import { ref, reactive, onMounted, computed, watch } from 'vue';
import { useRecords } from '../../composables/useRecords';
import { useToast } from '../../composables/useToast';
import { useSweetAlert } from '../../composables/useSweetAlert';
import SettingsCrudLayout from '../common/SettingsCrudLayout.vue';
import PaginationFooter from '../common/PaginationFooter.vue';
import ModalCloseButton from '../common/ModalCloseButton.vue';

const {
  userTypeItems,
  masterDataErrorMessage,
  loadUserTypesCrud,
  saveUserType,
  removeUserType
} = useRecords();

const { showToast } = useToast();
const { showConfirm } = useSweetAlert();

const formMode = ref('create');
const formState = reactive({ userTypeId: '', userTypeName: '' });
const searchText = ref('');
const isModalOpen = ref(false);

onMounted(() => {
  loadUserTypesCrud();
});

function resetForm() {
  formMode.value = 'create';
  formState.userTypeId = '';
  formState.userTypeName = '';
}
function openCreateModal() {
  resetForm();
  isModalOpen.value = true;
}
function closeModal() {
  isModalOpen.value = false;
}

async function onSubmit() {
  masterDataErrorMessage.value = '';
  const userTypeName = formState.userTypeName.trim();

  if (!userTypeName) {
    masterDataErrorMessage.value = 'UserType Name is required.';
    return;
  }

  const payload = {
    userTypeId: formState.userTypeId ? String(formState.userTypeId) : null,
    userTypeName
  };
  try {
    const res = await saveUserType(payload, formMode.value === 'edit');
    if (res?.success) {
      showToast(formMode.value === 'edit' ? 'User type updated' : 'User type created');
      resetForm();
      closeModal();
    } else {
      masterDataErrorMessage.value = res?.message || 'Save failed';
    }
  } catch {
    masterDataErrorMessage.value = 'Save failed';
  }
}

function editItem(item) {
  formMode.value = 'edit';
  formState.userTypeId = item.userTypeId || '';
  formState.userTypeName = item.userTypeName || '';
  isModalOpen.value = true;
}

async function deleteItem(item) {
  const isConfirmed = await showConfirm();
  if (!isConfirmed) return;

  try {
    const res = await removeUserType(item.userTypeId);
    if (res?.success) {
      showToast('User type deleted successfully');
    } else {
      masterDataErrorMessage.value = res?.message || 'Delete failed';
    }
  } catch {
    masterDataErrorMessage.value = 'Delete failed';
  }
}

const filteredUserTypeItems = computed(() => {
  const keyword = (searchText.value || '').toLowerCase();
  if (!keyword) return userTypeItems.value;
  return userTypeItems.value.filter((item) =>
    (item.userTypeId || '').toLowerCase().includes(keyword) ||
    (item.userTypeName || '').toLowerCase().includes(keyword)
  );
});

// Pagination
const currentPage = ref(1);
const pageSize = ref(10);
const totalItems = computed(() => filteredUserTypeItems.value.length);
const paginatedItems = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return filteredUserTypeItems.value.slice(start, start + pageSize.value);
});
watch([searchText, userTypeItems], () => {
  currentPage.value = 1;
});
</script>

<template>
  <SettingsCrudLayout
    v-model:searchText="searchText"
    :hide-form-panel="true"
    search-placeholder="Search user type..."
    total-label=""
    showing-label=""
  >
    <template #actions>
      <button class="bg-emerald-600 text-white ui-btn px-4 py-2 rounded-lg hover:bg-emerald-700 transition active:scale-95" @click="openCreateModal">
        Add New
      </button>
      <button class="bg-white hover:bg-slate-100 border border-slate-200 text-slate-700 ui-btn px-4 py-2 rounded-lg transition active:scale-95" @click="loadUserTypesCrud">
        Refresh
      </button>
    </template>

    <template #table>
      <p v-if="masterDataErrorMessage" class="p-4 ui-text-sm text-red-500 font-semibold">{{ masterDataErrorMessage }}</p>
      <div class="overflow-x-auto w-full">
        <table class="w-full text-left border-collapse ui-table">
          <thead>
            <tr class="border-b border-slate-200 font-bold">
              <th class="p-4">Id</th>
              <th class="p-4">Name</th>
              <th class="p-4">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr v-if="paginatedItems.length === 0">
              <td colspan="3" class="p-8 text-center text-slate-400 font-semibold">No user types found.</td>
            </tr>
            <tr v-else v-for="item in paginatedItems" :key="item.userTypeId">
              <td class="p-4"><span class="text-slate-700 font-semibold">{{ item.userTypeId }}</span></td>
              <td class="p-4"><strong class="text-slate-800">{{ item.userTypeName }}</strong></td>
              <td class="p-4">
                <div class="flex gap-2">
                  <button class="text-indigo-500 hover:text-indigo-700 hover:bg-indigo-50 p-1.5 rounded-lg transition" @click="editItem(item)" title="Edit">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                    </svg>
                  </button>
                  <button class="text-red-400 hover:text-red-600 hover:bg-red-50 p-1.5 rounded-lg transition" @click="deleteItem(item)" title="Delete">
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
    </template>
    <div v-if="isModalOpen" class="fixed inset-0 z-[70] flex items-center justify-center bg-slate-900/45 p-4">
      <div class="w-full max-w-xl rounded-2xl border border-slate-200 bg-white shadow-2xl">
        <div class="flex items-center justify-between border-b border-slate-100 px-5 py-4">
          <h3 class="m-0 text-base font-extrabold text-slate-800">{{ formMode === 'create' ? 'Add User Type' : 'Edit User Type' }}</h3>
          <ModalCloseButton @click="closeModal" />
        </div>
        <div class="space-y-3 p-5">
          <div class="flex flex-col gap-1 text-left">
            <label class="text-xs font-bold text-slate-500">User Type Name</label>
            <input
              type="text"
              v-model="formState.userTypeName"
              class="w-full bg-white border border-slate-200 rounded-xl px-4 py-3 md:py-3.5 text-sm md:text-base outline-none focus:border-[#0e4391] focus:ring-4 focus:ring-[#0e4391]/10 focus:shadow-[0_0_15px_rgba(14,67,145,0.15)] transition-all duration-300"
            />
          </div>
        </div>
        <div class="flex items-center justify-end gap-2 border-t border-slate-100 px-5 py-4">
          <button class="bg-primary text-white ui-btn px-4 py-2.5 rounded-lg hover:bg-primary-dark transition active:scale-95 shadow-md shadow-primary/10" @click="onSubmit">
             {{ formMode === 'create' ? 'Add' : 'Save' }}
          </button>
        </div>
      </div>
    </div>
  </SettingsCrudLayout>
</template>
