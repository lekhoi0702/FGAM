<script setup>
import { ref, computed } from 'vue'
import { formatDateTime24h } from '../../utils/dateTime'
import { useRecords } from '../../composables/useRecords'
import { useSweetAlert } from '../../composables/useSweetAlert'
import { useI18n } from '../../composables/useI18n'
import ModalCloseButton from '../common/ModalCloseButton.vue'

const props = defineProps({
  record: {
    type: Object,
    required: true
  }
})

const emit = defineEmits(['close'])

const { submitCheckOut } = useRecords()
const { showError, showSuccess, showConfirm } = useSweetAlert()
const { t } = useI18n()

const isCheckedOut = computed(() => !!props.record.checkOutTime)
const checkoutLoading = ref(false)

function formatDateTime(value) {
  return formatDateTime24h(value)
}

const stayDuration = computed(() => {
  if (!props.record.checkInTime) return '-'
  const start = new Date(props.record.checkInTime).getTime()
  const end = props.record.checkOutTime ? new Date(props.record.checkOutTime).getTime() : Date.now()

  const diffMs = end - start
  if (diffMs < 0) return '-'

  const mins = Math.floor(diffMs / 60000)
  const hrs = Math.floor(mins / 60)
  const days = Math.floor(hrs / 24)

  if (days > 0) return `${days}d ${hrs % 24}h ${mins % 60}m`
  if (hrs > 0) return `${hrs}h ${mins % 60}m`
  return `${mins}m`
})

async function handleCheckOut() {
  const transactionId = props.record.transactionId
  if (!transactionId) return

  const confirmed = await showConfirm(
    t.value.confirmCheckoutTitle || 'Xác nhận ra kho?',
    t.value.confirmOk || 'Xác nhận',
    t.value.confirmCancel || 'Hủy'
  )
  if (!confirmed) return

  checkoutLoading.value = true
  try {
    const response = await submitCheckOut(props.record.transactionId)
    if (response?.success) {
      await showSuccess(t.value.checkoutSuccess || 'Xác nhận ra kho thành công.')
      emit('close')
    } else {
      await showError(response?.message || t.value.checkoutFailed || 'Xác nhận ra kho thất bại.')
    }
  } catch (e) {
    await showError(t.value.checkoutFailed || 'Xác nhận ra kho thất bại.')
  } finally {
    checkoutLoading.value = false
  }
}
</script>

<template>
  <div class="fixed inset-0 bg-slate-900/60 backdrop-blur-md z-[80] flex items-center justify-center p-4">
    <div class="bg-white rounded-3xl w-full max-w-4xl shadow-2xl overflow-hidden border border-slate-100 flex flex-col max-h-[90vh]">
      <div class="bg-slate-50 border-b border-slate-100 px-6 py-5 flex justify-between items-center">
        <div>
          <h2 class="text-lg font-bold text-slate-800">Check-In Profile</h2>
          <p class="text-xs text-slate-400 mt-0.5">Transaction ID: #{{ record.transactionId }}</p>
        </div>
        <ModalCloseButton @click="emit('close')" />
      </div>

      <div class="p-6 flex-1 overflow-y-auto">
        <div class="grid grid-cols-1 lg:grid-cols-[280px_1fr] gap-6">
          <div class="space-y-3">
            <div class="relative w-full aspect-[3/4] rounded-2xl overflow-hidden border border-slate-200 shadow-md bg-slate-50 flex items-center justify-center">
              <img
                v-if="record.photoPath"
                :src="`data:image/jpeg;base64,${record.photoPath}`"
                class="w-full h-full object-cover"
                alt="employee verification"
              />
              <div v-else class="text-slate-300 select-none">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
              </div>
            </div>
          </div>

          <div class="space-y-5">
            <div class="space-y-3">
              <h3 class="text-xs font-bold text-slate-400 uppercase tracking-wide border-b border-slate-100 pb-1.5">Employee Information</h3>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-y-3.5 gap-x-4 text-xs">
                <div>
                  <span class="text-slate-400 block mb-0.5">Full Name</span>
                  <span class="font-bold text-slate-800 text-sm">{{ record.fullName || 'Unknown' }}</span>
                </div>
                <div>
                  <span class="text-slate-400 block mb-0.5">User ID</span>
                  <span class="font-semibold text-slate-700">{{ record.userId || '-' }}</span>
                </div>
                <div>
                  <span class="text-slate-400 block mb-0.5">Card ID</span>
                  <span class="font-semibold text-slate-700">{{ record.cardNumber || '-' }}</span>
                </div>
                <div>
                  <span class="text-slate-400 block mb-0.5">Company</span>
                  <span class="font-semibold text-slate-700">{{ record.companyName || '-' }}</span>
                </div>
                <div>
                  <span class="text-slate-400 block mb-0.5">Department</span>
                  <span class="font-semibold text-slate-700">{{ record.departmentName || record.departmentCode || '-' }}</span>
                </div>
                <div>
                  <span class="text-slate-400 block mb-0.5">User Type Name</span>
                  <span class="font-semibold text-slate-700">{{ record.userTypeName || '-' }}</span>
                </div>
              </div>
            </div>

            <div class="space-y-3">
              <h3 class="text-xs font-bold text-slate-400 uppercase tracking-wide border-b border-slate-100 pb-1.5">Lượt vào hiện tại</h3>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-y-3.5 gap-x-4 text-xs">
                <div>
                  <span class="text-slate-400 block mb-0.5">Người liên hệ</span>
                  <span class="font-semibold text-slate-700">{{ record.contactPerson || '-' }}</span>
                </div>
                <div>
                  <span class="text-slate-400 block mb-0.5">Mục đích vào</span>
                  <span class="font-semibold text-slate-700">{{ record.purposeName || '-' }}</span>
                </div>
                <div>
                  <span class="text-slate-400 block mb-0.5">Giờ vào</span>
                  <span class="font-semibold text-slate-700">{{ formatDateTime(record.checkInTime) }}</span>
                </div>
                <div>
                  <span class="text-slate-400 block mb-0.5">Giờ ra</span>
                  <span class="font-semibold text-slate-700" :class="isCheckedOut ? 'text-slate-700' : 'text-amber-600 font-bold italic'">
                    {{ isCheckedOut ? formatDateTime(record.checkOutTime) : 'Đang trong kho' }}
                  </span>
                </div>
                <div class="sm:col-span-2">
                  <span class="text-slate-400 block mb-0.5">Thời gian ở</span>
                  <span class="font-bold" :class="isCheckedOut ? 'text-slate-700' : 'text-emerald-600'">
                    {{ stayDuration }}
                  </span>
                </div>
              </div>
            </div>



          </div>
        </div>
      </div>

      <!-- Modal Footer -->
      <div class="bg-slate-50 border-t border-slate-100 px-6 py-4 flex justify-end gap-3 shrink-0">
        <button 
          v-if="!isCheckedOut"
          @click="handleCheckOut"
          :disabled="checkoutLoading"
          class="px-5 py-2.5 bg-red-600 hover:bg-red-500 disabled:opacity-60 text-white text-xs font-bold rounded-xl active:scale-95 transition flex items-center gap-1.5 shadow-md shadow-red-500/10"
        >
          <span v-if="checkoutLoading" class="w-3.5 h-3.5 border-2 border-white/30 border-t-white rounded-full animate-spin"></span>
          <span>Xác nhận ra kho</span>
        </button>
      </div>

    </div>
  </div>
</template>
