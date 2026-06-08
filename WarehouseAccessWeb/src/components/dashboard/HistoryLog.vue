<script setup>
import { ref, computed, onMounted } from 'vue';
import { useI18n } from '../../composables/useI18n';
import { useRecords } from '../../composables/useRecords';
import { useToast } from '../../composables/useToast';
import { useSweetAlert } from '../../composables/useSweetAlert';
import PaginationFooter from '../common/PaginationFooter.vue';
import ModalCloseButton from '../common/ModalCloseButton.vue';

const { t } = useI18n();
const { 
  historyItems, 
  historyLoading, 
  historyErrorMessage, 
  historyPage,
  historyPageSize,
  historyTotal,
  historyTotalPages,
  loadHistoryRecords,
  exportHistory
} = useRecords();
const { showToast } = useToast();
const { showError } = useSweetAlert();

const keyword = ref('');
const fromDate = ref('');
const toDate = ref('');

const selectedPhoto = ref('');

onMounted(() => {
  loadHistoryRecords();
});

function handleSearch() {
  loadHistoryRecords(keyword.value, fromDate.value, toDate.value, 1, historyPageSize.value);
}

function handlePageChange(newPage) {
  loadHistoryRecords(keyword.value, fromDate.value, toDate.value, newPage, historyPageSize.value);
}

async function handleExportExcel() {
  try {
    const res = await exportHistory(keyword.value, fromDate.value, toDate.value);
    if (res.success && res.data?.blob) {
      const downloadUrl = URL.createObjectURL(res.data.blob);
      const a = document.createElement('a');
      a.href = downloadUrl;
      a.download = res.data.fileName || 'access-log.xlsx';
      document.body.appendChild(a);
      a.click();
      a.remove();
      URL.revokeObjectURL(downloadUrl);
      showToast(t.value.toastCSV || "Excel exported");
    } else {
      await showError(res.message || "Export Excel failed");
    }
  } catch (e) {
    await showError("Export failed");
  }
}

function handleRefresh() {
  loadHistoryRecords(keyword.value, fromDate.value, toDate.value, historyPage.value, historyPageSize.value);
}

function parseDate(value) {
  if (!value) return null
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? null : date
}

function padDatePart(value) {
  return String(value).padStart(2, '0')
}

function formatHistoryDate(value) {
  const date = parseDate(value)
  if (!date) return '-'
  return `${padDatePart(date.getDate())}/${padDatePart(date.getMonth() + 1)}/${date.getFullYear()}`
}

function formatHistoryTime(value) {
  const date = parseDate(value)
  if (!date) return '-'
  return `${padDatePart(date.getHours())}:${padDatePart(date.getMinutes())}:${padDatePart(date.getSeconds())}`
}

function formatHistoryDateTime(value) {
  const date = parseDate(value)
  if (!date) return '-'
  return `${padDatePart(date.getDate())}/${padDatePart(date.getMonth() + 1)}/${date.getFullYear()} ${padDatePart(date.getHours())}:${padDatePart(date.getMinutes())}:${padDatePart(date.getSeconds())}`
}
</script>

<template>
  <div class="space-y-6">
    <!-- Error state -->
    <p v-if="historyErrorMessage" class="text-xs text-red-500 font-semibold">{{ historyErrorMessage }}</p>

    <!-- Table Card Container -->
    <div class="bg-white rounded-2xl border border-slate-200/80 shadow-sm overflow-hidden glassmorphism">
      <!-- Card Header Filter bar -->
      <div class="flex flex-col sm:flex-row gap-3 items-center justify-between p-4 bg-slate-50/70 border-b border-slate-100">
        <div class="flex flex-wrap items-center gap-3 w-full">
          <input 
            type="text" 
            v-model="keyword" 
            :placeholder="t.historySearchPlaceholder" 
            class="bg-white border border-slate-200 rounded-lg px-4 py-2.5 ui-input outline-none focus:border-primary/50 min-w-[280px] sm:min-w-[360px] lg:min-w-[480px]"
          />
          
          <!-- Date ranges -->
          <input 
            type="date" 
            v-model="fromDate" 
            class="bg-white border border-slate-200 rounded-lg px-4 py-2.5 ui-input outline-none focus:border-primary/50 transition-all"
          />
          <span class="text-slate-400 font-bold">~</span>
          <input 
            type="date" 
            v-model="toDate" 
            class="bg-white border border-slate-200 rounded-lg px-4 py-2.5 ui-input outline-none focus:border-primary/50 transition-all"
          />

          <button 
            class="bg-primary hover:bg-primary-dark text-white ui-btn px-5 py-2.5 rounded-lg text-xs font-extrabold transition active:scale-95 shadow-sm"
            @click="handleSearch"
          >
            {{ t.searchBtn }}
          </button>

          <button 
            class="bg-emerald-600 hover:bg-emerald-700 text-white ui-btn px-5 py-2.5 rounded-lg transition active:scale-95 flex items-center gap-1.5 shadow-sm font-extrabold text-xs"
            @click="handleExportExcel"
          >
            {{ t.exportBtn }}
          </button>

          <button 
            class="bg-white hover:bg-slate-100 border border-slate-200 text-slate-700 ui-btn px-5 py-2.5 rounded-lg active:scale-95 transition font-extrabold text-xs shadow-sm"
            @click="handleRefresh"
            :disabled="historyLoading"
          >
            <span>{{ t.refreshBtn }}</span>
          </button>
        </div>
      </div>

      <!-- Table Body -->
      <div class="overflow-x-auto w-full">
        <table class="w-full text-left border-collapse ui-table">
          <thead>
            <tr class="border-b border-slate-200 font-bold">
              <th class="p-4">{{ t.stt }}</th>
              <th class="p-4">{{ t.company }}</th>
              <th class="p-4">{{ t.userId }}</th>
              <th class="p-4">{{ t.fullName }}</th>
              <th class="p-4">{{ t.cardNumber }}</th>
              <th class="p-4">{{ t.userType }}</th>
              <th class="p-4">{{ t.contactPerson }}</th>
              <th class="p-4">{{ t.purpose }}</th>
              <th class="p-4">{{ t.checkIn }}</th>
              <th class="p-4">{{ t.checkOut }}</th>
              <th class="p-4">{{ t.status }}</th>
              <th class="p-4">{{ t.photo }}</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr v-if="historyLoading && historyItems.length === 0">
              <td colspan="12" class="p-10 text-center text-sm text-slate-400 font-semibold">
                <div class="w-6 h-6 border-2 border-slate-200 border-t-primary rounded-full animate-spin mx-auto mb-2"></div>
                {{ t.loadingHistory }}
              </td>
            </tr>
            <tr v-else-if="historyItems.length === 0">
              <td colspan="12" class="p-10 text-center text-sm text-slate-400 font-semibold">{{ t.noHistoryFound }}</td>
            </tr>
            <tr 
              v-else 
              v-for="(r, index) in historyItems" 
              :key="`history-${r.transactionId}`"
            >
              <td class="p-4"><span class="text-slate-700 font-semibold">{{ (historyPage - 1) * historyPageSize + index + 1 }}</span></td>
              <td class="p-4 text-slate-500">
                <span class="history-company-text">{{ r.companyName || '-' }}</span>
              </td>
              <td class="p-4 text-slate-700 font-bold">{{ r.userId || '-' }}</td>
              <td class="p-4"><strong class="text-slate-800">{{ r.fullName || '-' }}</strong></td>
              <td class="p-4 text-slate-500 font-medium">{{ r.cardNumber || '-' }}</td>
              <td class="p-4 text-slate-500">{{ r.userTypeName || '-' }}</td>
              <td class="p-4 text-slate-500">{{ r.contactPerson || '-' }}</td>
              <td class="p-4"><span class="px-2 py-0.5 bg-slate-100 text-slate-600 rounded-md font-semibold" v-if="r.purposeName">{{ r.purposeName }}</span><span v-else>-</span></td>
              <td class="p-4 text-slate-500 font-medium">{{ formatHistoryDateTime(r.checkInTime) }}</td>
              <td class="p-4 text-slate-500 font-medium">{{ formatHistoryDateTime(r.checkOutTime) }}</td>
              <td class="p-4">
                <span 
                  class="px-2.5 py-0.5 rounded-full text-[10px] font-bold border"
                  :class="[
                    r.checkOutTime 
                      ? 'bg-blue-50 text-blue-600 border-blue-200/60' 
                      : 'bg-emerald-50 text-emerald-600 border-emerald-200/60'
                  ]"
                >
                  {{ r.checkOutTime ? t.statusOut : t.statusIn }}
                </span>
              </td>
              <td class="p-4">
                <div 
                  v-if="r.photoPath"
                  class="w-10 h-10 rounded-lg overflow-hidden border border-slate-200 cursor-zoom-in relative group shrink-0"
                  @click="selectedPhoto = r.photoPath"
                >
                  <img :src="`data:image/jpeg;base64,${r.photoPath}`" class="w-full h-full object-cover" alt="photo" />
                  <div class="absolute inset-0 bg-black/40 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                    </svg>
                  </div>
                </div>
                <span v-else class="text-slate-400 font-semibold">-</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <!-- Pagination Footer -->
      <PaginationFooter :current-page="historyPage" :page-size="historyPageSize" :total-items="historyTotal" @update:current-page="handlePageChange" />
    </div>

    <!-- Photo Zoom Modal Layer -->
    <transition name="modal">
      <div 
        class="fixed inset-0 bg-[#0a142d]/85 backdrop-blur-md z-[8500] flex items-center justify-center p-4" 
        v-if="selectedPhoto"
        @click="selectedPhoto = ''"
      >
        <div class="bg-white border border-slate-200 rounded-3xl p-6 w-full max-w-[800px] text-slate-800 shadow-2xl relative" @click.stop>
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-sm font-extrabold text-slate-900">{{ t.photoPreview }}</h2>
            <ModalCloseButton :title="t.close || 'Close'" @click="selectedPhoto = ''" />
          </div>
          <div class="w-full aspect-[4/3] max-h-[65vh] rounded-2xl border border-slate-200 bg-slate-50 overflow-hidden shadow-inner flex items-center justify-center">
            <img :src="`data:image/jpeg;base64,${selectedPhoto}`" class="max-w-full max-h-full object-contain" alt="zoom preview" />
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<style scoped>
.history-company-text {
  font-family: Inter, "Noto Sans", "Microsoft JhengHei", "PingFang TC", "PingFang SC", "Segoe UI", Arial, sans-serif;
  font-weight: 600;
  letter-spacing: 0;
  word-break: break-word;
}
</style>
