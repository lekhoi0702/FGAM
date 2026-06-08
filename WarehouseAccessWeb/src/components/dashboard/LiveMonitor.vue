<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue';
import { useI18n } from '../../composables/useI18n';
import { useRecords } from '../../composables/useRecords';
import { useSweetAlert } from '../../composables/useSweetAlert';
import { getServerNow } from '../../services/api/api-client';

const { t } = useI18n();
const { 
  monitorItems, 
  monitorLoading, 
  monitorErrorMessage, 
  loadLiveMonitor,
  submitCheckOut
} = useRecords();
const { showError, showSuccess, showConfirm } = useSweetAlert()

const props = defineProps({
  interactive: { type: Boolean, default: true }
});

defineEmits(['detail']);

const now = ref(getServerNow());
const checkoutLoadingTransactionId = ref('')
let durationTimer = null;
let pollingTimer = null;

onMounted(() => {
  loadLiveMonitor();
  now.value = getServerNow();
  
  // Định kỳ cập nhật thời gian hiển thị lưu trú mỗi 10 giây (dùng đồng hồ sync server)
  durationTimer = setInterval(() => {
    now.value = getServerNow();
  }, 10000);

  // Định kỳ tải lại dữ liệu mới từ API mỗi 3 giây nếu hệ thống đang rảnh
  pollingTimer = setInterval(() => {
    if (!monitorLoading.value) {
      loadLiveMonitor();
    }
  }, 3000);
});

onUnmounted(() => {
  if (durationTimer) clearInterval(durationTimer);
  if (pollingTimer) clearInterval(pollingTimer);
});

// Stay Clocks
function parseIsoTime(value) {
  if (!value) return null;
  const timestamp = Date.parse(value);
  return Number.isNaN(timestamp) ? null : timestamp;
}

function getStayDuration(checkInTimeStr) {
  if (!checkInTimeStr) return "—";
  const entryTime = parseIsoTime(checkInTimeStr);
  if (entryTime == null) return "Invalid time";
  const ms = now.value - entryTime;
  if (ms < 0) return "0m";
  const m = Math.floor(ms / 60000);
  const h = Math.floor(m / 60);
  return h > 0 ? `${h}h ${m % 60}m` : `${m}m`;
}

function isLongStay(checkInTimeStr) {
  if (!checkInTimeStr) return false;
  const entryTime = parseIsoTime(checkInTimeStr);
  if (entryTime == null) return false;
  return (now.value - entryTime) > 3600000; // Greater than 1 hr
}

function formatSimpleTime(checkInTimeStr) {
  if (!checkInTimeStr) return "—";
  const timestamp = parseIsoTime(checkInTimeStr);
  if (timestamp == null) return "Invalid time";
  const d = new Date(timestamp);
  const pad = (n) => n.toString().padStart(2, '0');
  return `${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
}

async function handleCardCheckOut(record) {
  const transactionId = record?.transactionId == null ? '' : String(record.transactionId)
  if (!transactionId) {
    await showError(t.value.transactionIdMissing || 'Transaction ID is missing.')
    return
  }

  const confirmed = await showConfirm(
    t.value.confirmCheckoutTitle || 'Confirm?',
    t.value.confirmOk || 'OK',
    t.value.confirmCancel || 'Cancel'
  )
  if (!confirmed) return

  checkoutLoadingTransactionId.value = transactionId
  try {
    const response = await submitCheckOut(transactionId)
    if (response?.success) {
      await showSuccess(t.value.checkoutSuccess || 'Check-out successful.')
      return
    }
    await showError(response?.message || t.value.checkoutFailed || 'Check-out failed.')
  } catch {
    await showError(t.value.checkoutFailed || 'Check-out failed.')
  } finally {
    checkoutLoadingTransactionId.value = ''
  }
}
</script>

<template>
  <div class="space-y-6">

    <!-- Error View -->
    <p v-if="monitorErrorMessage" class="text-xs text-red-500 font-semibold">{{ monitorErrorMessage }}</p>

    <!-- Active List Grid -->
    <div>
      <div v-if="monitorLoading && monitorItems.length === 0" class="py-20 text-center text-slate-400 font-semibold text-sm">
        <div class="w-8 h-8 border-4 border-slate-200 border-t-primary rounded-full animate-spin mx-auto mb-3"></div>
        {{ t.loadingMonitor }}
      </div>

      <div v-else-if="monitorItems.length === 0" class="flex flex-col items-center justify-center py-20 text-center bg-white/50 rounded-xl border border-dashed border-slate-300/80 glassmorphism">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-12 w-12 text-slate-300 mb-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M5 19a2 2 0 01-2-2V7a2 2 0 012-2h4l2 2h4a2 2 0 012 2v1M5 19h14a2 2 0 002-2v-5a2 2 0 00-2-2H9l-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
        </svg>
        <p class="text-slate-500 font-medium text-sm">{{ t.noVisitors }}</p>
      </div>

      <!-- Cards Grid -->
      <transition-group 
        v-else
        tag="div" 
        name="monitor-list"
        class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-6 gap-4"
        enter-active-class="transition duration-300 ease-out transform"
        enter-from-class="opacity-0 scale-95 translate-y-4"
        enter-to-class="opacity-100 scale-100 translate-y-0"
        leave-active-class="transition duration-200 ease-in transform absolute"
        leave-from-class="opacity-100 scale-100 translate-y-0"
        leave-to-class="opacity-0 scale-95 translate-y-4"
      >
        <div 
          v-for="r in monitorItems" 
          :key="r.transactionId" 
          class="bg-white border border-slate-200/80 rounded-xl shadow-sm flex flex-col relative overflow-hidden"
          :class="{ 'cursor-pointer hover-lift': props.interactive }"
          @click="props.interactive && $emit('detail', r)"
        >
          <!-- Card Header Border Accent -->
          <div class="h-1.5 w-full bg-gradient-to-r from-primary to-primary-light"></div>

          <!-- Card Body: Avatar, details -->
          <div class="p-3.5 flex-1 flex flex-col justify-between">
            <div class="flex items-start gap-3">
              <!-- Photo Container -->
              <div class="shrink-0 w-[80px]">
                <!-- Avatar -->
                <div 
                  class="w-[80px] h-[100px] rounded-xl border border-slate-200/60 flex items-center justify-center overflow-hidden bg-slate-50 shadow-inner relative group"
                  :class="{ 'cursor-pointer': props.interactive }"
                  @click="props.interactive && $emit('detail', r)"
                >
                  <img 
                    v-if="r.photoPath" 
                    :src="`data:image/jpeg;base64,${r.photoPath}`" 
                    alt="avatar" 
                    class="w-full h-full object-cover" 
                  />
                  <div v-else class="text-primary/70 font-extrabold text-xl uppercase">
                    {{ (r.fullName || '?').slice(0, 1) }}
                  </div>
                  <!-- View Hover Overlay -->
                  <div v-if="props.interactive" class="absolute inset-0 bg-black/40 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                    <div class="flex items-center gap-1">
                      <svg xmlns="http://www.w3.org/2000/svg" class="h-3 w-3 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                      </svg>
                      <span class="text-white text-[9px] font-bold">{{ t.view }}</span>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Identity Details -->
              <div class="flex-1 min-w-0 pr-1 text-left">
                <h3 
                  class="text-sm sm:text-base font-extrabold text-slate-800 transition leading-tight"
                  :class="{ 'hover:text-primary cursor-pointer': props.interactive }"
                  @click="props.interactive && $emit('detail', r)"
                >
                  {{ r.fullName || 'Unknown' }}
                </h3>
                <p class="text-xs font-bold text-slate-400 mt-1 truncate">
                  ID: {{ r.userId || '-' }}
                </p>
                <p class="text-xs font-bold text-slate-400 mt-0.5 truncate">
                  {{ t.company }}: {{ r.companyName || '-' }}
                </p>
                <!-- User Type badge -->
                <div class="mt-1">
                  <span 
                    class="inline-block px-1.5 py-0.5 rounded bg-slate-100 text-slate-600 text-[9px] font-extrabold tracking-wide uppercase border border-slate-200 max-w-full truncate"
                    :title="r.userTypeName || '-'"
                  >
                    {{ r.userTypeName || '-' }}
                  </span>
                </div>
              </div>
            </div>

            <!-- Time Indicator row -->
            <div class="mt-3 flex items-center justify-between border-t border-slate-100 pt-2.5">
              <div class="flex flex-col text-left">
                <span class="text-[9px] text-slate-400 font-bold uppercase tracking-wider">{{ t.entryTime }}</span>
                <span class="text-[11px] font-bold text-slate-700 mt-0.5 flex items-center gap-1">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-3.5 w-3.5 text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <span>{{ formatSimpleTime(r.checkInTime) }}</span>
                </span>
              </div>
              <div class="flex flex-col text-right">
                <span class="text-[9px] text-slate-400 font-bold uppercase tracking-wider">{{ t.stayTime }}</span>
                <span 
                  class="text-[11px] font-black mt-0.5 flex items-center gap-1 justify-end"
                  :class="isLongStay(r.checkInTime) ? 'text-red-500' : 'text-emerald-500'"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-3.5 w-3.5" :class="isLongStay(r.checkInTime) ? 'text-red-400' : 'text-emerald-400'" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M6 2h12v4l-4 4 4 4v4H6v-4l4-4-4-4V2z" />
                  </svg>
                  <span>{{ getStayDuration(r.checkInTime) }}</span>
                </span>
              </div>
            </div>
          </div>

          <!-- Card Bottom Action -->
        </div>
      </transition-group>
    </div>
  </div>
</template>

<style scoped>
.monitor-list-move {
  transition: transform 0.3s ease;
}
</style>
