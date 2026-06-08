<script setup>
import { ref, reactive, onMounted, computed, watch } from 'vue';
import { useRecords } from '../../composables/useRecords';
import { useToast } from '../../composables/useToast';
import { useSweetAlert } from '../../composables/useSweetAlert';
import SettingsCrudLayout from '../common/SettingsCrudLayout.vue';
import PaginationFooter from '../common/PaginationFooter.vue';
import ModalCloseButton from '../common/ModalCloseButton.vue';

const {
  departmentItems,
  masterDataErrorMessage,
  loadDepartmentsCrud,
  saveDepartment,
  removeDepartment,
  importDepartments,
  downloadDepartmentsTemplate
} = useRecords();

const { showToast } = useToast();
const { showConfirm } = useSweetAlert();

const formMode = ref('create');
const formState = reactive({ departmentCode: '', departmentName: '' });
const searchText = ref('');
const isModalOpen = ref(false);
const importingDepartments = ref(false);
const departmentImportInputRef = ref(null);

onMounted(async () => {
  await loadDepartmentsCrud();
});

function resetForm() {
  formMode.value = 'create';
  formState.departmentCode = '';
  formState.departmentName = '';
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
  const departmentCode = formState.departmentCode.trim();
  const departmentName = formState.departmentName.trim();

  if (!departmentCode || !departmentName) {
    masterDataErrorMessage.value = 'Department Code and Department Name are required.';
    return;
  }

  const payload = { departmentCode, departmentName };
  const response = await saveDepartment(payload, formMode.value === 'edit');
  if (response?.success) {
    showToast(formMode.value === 'edit' ? 'Department updated' : 'Department created');
    closeModal();
    resetForm();
  } else {
    masterDataErrorMessage.value = response?.message || 'Save failed';
  }
}

function editItem(item) {
  formMode.value = 'edit';
  formState.departmentCode = item.departmentCode || '';
  formState.departmentName = item.departmentName || '';
  isModalOpen.value = true;
}

async function deleteItem(item) {
  const isConfirmed = await showConfirm();
  if (!isConfirmed) return;
  const response = await removeDepartment(item.departmentCode);
  if (response?.success) {
    showToast('Department deleted');
  } else {
    masterDataErrorMessage.value = response?.message || 'Delete failed';
  }
}

function triggerImportDepartmentsFilePicker() {
  if (importingDepartments.value) return;
  departmentImportInputRef.value?.click();
}

async function onDepartmentsFileChange(event) {
  const file = event?.target?.files?.[0];
  event.target.value = '';
  if (!file) return;

  importingDepartments.value = true;
  masterDataErrorMessage.value = '';
  const response = await importDepartments(file, '');
  if (response?.success) {
    const inserted = response?.data?.insertedCount ?? 0;
    const skipped = response?.data?.skippedCount ?? 0;
    showToast(`Import completed. Inserted: ${inserted}, Skipped: ${skipped}`);
    if (Array.isArray(response?.data?.errors) && response.data.errors.length > 0) {
      masterDataErrorMessage.value = response.data.errors.slice(0, 5).map((errorItem) => `Row ${errorItem.rowNumber}: ${errorItem.message}`).join(' | ');
    }
  } else {
    masterDataErrorMessage.value = response?.message || 'Import failed';
  }
  importingDepartments.value = false;
}

async function exportDepartmentsTemplate() {
  const response = await downloadDepartmentsTemplate();
  if (response?.success) {
    showToast('Template downloaded');
  } else {
    masterDataErrorMessage.value = response?.message || 'Export template failed';
  }
}

const filteredDepartmentItems = computed(() => {
  const keyword = (searchText.value || '').toLowerCase();
  if (!keyword) return departmentItems.value;
  return departmentItems.value.filter((item) =>
    (item.departmentCode || '').toLowerCase().includes(keyword) ||
    (item.departmentName || '').toLowerCase().includes(keyword)
  );
});

// Pagination
const currentPage = ref(1);
const pageSize = ref(10);
const totalItems = computed(() => filteredDepartmentItems.value.length);
const paginatedItems = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return filteredDepartmentItems.value.slice(start, start + pageSize.value);
});
watch([searchText, departmentItems], () => {
  currentPage.value = 1;
});
</script>

<template>
  <SettingsCrudLayout
    v-model:searchText="searchText"
    :hide-form-panel="true"
    search-placeholder="Search department..."
    total-label=""
    showing-label=""
  >
    <template #actions>
      <button class="bg-emerald-600 text-white ui-btn px-4 py-2 rounded-lg hover:bg-emerald-700 transition active:scale-95" @click="openCreateModal">
        Add New
      </button>
      <button class="bg-primary/10 hover:bg-primary/20 text-primary border border-primary/20 ui-btn px-4 py-2 rounded-lg transition active:scale-95 disabled:opacity-50" :disabled="importingDepartments" @click="triggerImportDepartmentsFilePicker">
        {{ importingDepartments ? 'Importing...' : 'Import Excel' }}
      </button>
      <button class="bg-white hover:bg-slate-100 border border-slate-200 text-slate-700 ui-btn px-4 py-2 rounded-lg transition active:scale-95" @click="exportDepartmentsTemplate">
        Export Template
      </button>
      <button class="bg-white hover:bg-slate-100 border border-slate-200 text-slate-700 ui-btn px-4 py-2 rounded-lg transition active:scale-95" @click="loadDepartmentsCrud">
        Refresh
      </button>
      <input ref="departmentImportInputRef" type="file" accept=".xlsx" class="hidden" @change="onDepartmentsFileChange" />
    </template>

    <template #table>
      <p v-if="masterDataErrorMessage" class="p-4 ui-text-sm text-red-500 font-semibold">{{ masterDataErrorMessage }}</p>
      <div class="overflow-x-auto w-full">
        <table class="w-full text-left border-collapse ui-table">
          <thead>
            <tr class="border-b border-slate-200 font-bold">
              <th class="p-4">Department Code</th>
              <th class="p-4">Department Name</th>
              <th class="p-4">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr v-if="paginatedItems.length === 0">
              <td colspan="3" class="p-8 text-center text-slate-400 font-semibold">No departments found.</td>
            </tr>
            <tr v-else v-for="item in paginatedItems" :key="item.departmentCode">
              <td class="p-4"><span class="text-slate-700 font-semibold">{{ item.departmentCode }}</span></td>
              <td class="p-4"><strong class="text-slate-800">{{ item.departmentName }}</strong></td>
              <td class="p-4">
                <div class="flex gap-2">
                  <button class="ui-btn text-indigo-500 hover:text-indigo-700 hover:bg-indigo-50 transition" @click="editItem(item)" title="Edit">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                    </svg>
                  </button>
                  <button class="ui-btn text-red-400 hover:text-red-600 hover:bg-red-50 transition" @click="deleteItem(item)" title="Delete">
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
          <h3 class="m-0 text-base font-extrabold text-slate-800">{{ formMode === 'create' ? 'Add Department' : 'Edit Department' }}</h3>
          <ModalCloseButton @click="closeModal" />
        </div>
        <div class="space-y-3 p-5">
          <div class="flex flex-col gap-1 text-left">
            <label class="text-xs font-bold text-slate-500">Department Code</label>
            <input
              type="text"
              v-model="formState.departmentCode"
              :disabled="formMode === 'edit'"
              class="w-full bg-white border border-slate-200 rounded-xl px-4 py-3 md:py-3.5 text-sm md:text-base outline-none focus:border-[#0e4391] focus:ring-4 focus:ring-[#0e4391]/10 focus:shadow-[0_0_15px_rgba(14,67,145,0.15)] disabled:opacity-50 transition-all duration-300"
            />
          </div>
          <div class="flex flex-col gap-1 text-left">
            <label class="text-xs font-bold text-slate-500">Department Name</label>
            <input
              type="text"
              v-model="formState.departmentName"
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
