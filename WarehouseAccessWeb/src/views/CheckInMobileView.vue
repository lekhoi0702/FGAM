<script setup>
import { ref, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { useCheckInFlow } from '../composables/useCheckInFlow'

import bgImage from '../assets/background.jpg'
import { useSweetAlert } from '../composables/useSweetAlert'
import { logout } from '../stores/auth.store'
import logoImage from '../assets/logo-jiahsin-co-chu.png'

const router = useRouter()
const { showError, showSuccess } = useSweetAlert()

function handleBackToDashboard() {
  logout()
  router.replace('/login')
}

const fireMobileAlert = async (message, type = 'info') => {
  if (type === 'success') {
    await showSuccess(message)
    return
  }
  await showError(message)
}

const {
  cameraError,
  cameraErrorMessage,
  facing,
  switching,
  hasMultipleCams,
  showCam,
  flipCamera,
  step,
  checkInCardNumber,
  cardInputRef,
  lookupLoading,
  lookupMessage,
  hasCardLookupResult,
  submitLoading,
  fieldsLockedByGuestData,
  formState,
  videoRef,
  canvasRef,
  scannerState,
  autoCaptureSupported,
  autoCaptureEnabled,
  autoCaptureStatus,
  contactDeptItems,
  purposeItems,
  userTypeItems,
  handleCardLookup,
  nextStep,
  prevStep,
  resetCheckInFlow,
  triggerCamera,
  handleCheckInSubmit,
  showManualForm,
  manualStep,
  checkingUser,
  handleOpenManualForm,
  handleCancelManualForm,
  handleManualNext,
  handleManualBack,
  handleManualFormSubmit,
  selectUserType,
  selectPurpose
} = useCheckInFlow({
  notify: fireMobileAlert,
  onSuccess: async () => {
    await showSuccess('Chào mừng đến với Jiahsin! Đăng ký vào kho thành công.')
    window.location.href = '/check-in-mobile'
  }
})

const translateUserTypeName = (name) => {
  if (!name) return ''
  const lower = name.toLowerCase()
  if (lower.includes('staff') || lower.includes('nhân viên') || lower.includes('nội bộ')) return 'Nhân Viên Nội Bộ'
  if (lower.includes('vendor') || lower.includes('nhà cung cấp') || lower.includes('ncc')) return 'Nhà Cung Cấp'
  if (lower.includes('brand') || lower.includes('khách hàng') || lower.includes('đối tác')) return 'Khách Hàng'
  if (lower.includes('audit') || lower.includes('kiểm toán')) return 'Kiểm Toán Bên Thứ 3'
  return name
}

const translatePurposeName = (name) => {
  if (!name) return ''
  const lower = name.toLowerCase()
  if (lower.includes('work') || lower.includes('làm việc') || lower.includes('công tác')) return 'Làm việc / Liên hệ công tác'
  if (lower.includes('delivery') || lower.includes('giao hàng') || lower.includes('nhận hàng')) return 'Giao nhận hàng hóa'
  if (lower.includes('audit') || lower.includes('kiểm toán') || lower.includes('đánh giá')) return 'Đánh giá / Kiểm toán'
  if (lower.includes('meeting') || lower.includes('họp')) return 'Hội họp / Gặp gỡ'
  if (lower.includes('interview') || lower.includes('phỏng vấn')) return 'Phỏng vấn xin việc'
  return name
}

const handleInputBlur = () => {
  if (step.value === 1 && !hasCardLookupResult.value && !lookupLoading.value && !showManualForm.value) {
    nextTick(() => {
      cardInputRef.value?.focus()
    })
  }
}
</script>

<template>
  <div class="checkin-mobile-page h-screen w-full flex bg-white font-sans overflow-hidden">
    <!-- Unified Kiosk Card (Full Screen Layout) -->
    <div class="checkin-shell w-full h-screen bg-white p-4 sm:p-6 md:p-8 flex flex-col justify-between overflow-hidden space-y-6">
      
      <!-- Top Branding Header -->
      <div class="flex items-center gap-4 justify-center py-6 sm:py-8 -mx-4 sm:-mx-6 md:-mx-8 px-4 sm:px-6 md:px-8 -mt-4 sm:-mt-6 md:-mt-8 bg-gradient-to-r from-[#0a3575] to-[#0e4391] border-b-[4px] border-accent">
        <img class="h-10 sm:h-14 w-auto object-contain" :src="logoImage" alt="JIA HSIN Logo" />
        <span class="h-6 sm:h-10 w-px bg-white/25"></span>
        <div class="leading-none text-left">
          <h2 class="text-sm sm:text-2xl font-black text-white uppercase tracking-wider whitespace-nowrap">Kho Thành Phẩm</h2>
          <span class="text-[10px] sm:text-sm text-white/70 font-bold uppercase mt-1 block whitespace-nowrap">Quản Lý Ra Vào</span>
        </div>
      </div>

      <!-- Content Area -->
      <div class="flex-1 overflow-hidden flex flex-col justify-center items-center w-full">
        <transition name="slide-fade" mode="out-in">
          <!-- Step 1 Layout -->
          <div v-if="step === 1" class="w-full max-w-xl mx-auto space-y-8 py-6 sm:py-10 text-left">
            <transition name="slide-fade" mode="out-in">
              <!-- Scan Mode -->
              <div v-if="!showManualForm" class="space-y-8">
                <!-- Pulse NFC/Card Scanning Animation (Only shown if card not yet scanned) -->
                <transition name="slide-fade">
                  <div v-if="!hasCardLookupResult" class="flex flex-col items-center justify-center py-16 bg-blue-50/30 border border-blue-100/50 rounded-[32px] shadow-sm">
                    <div class="relative w-44 h-44 flex items-center justify-center">
                      <!-- Glowing radar circles -->
                      <div class="absolute inset-0 rounded-full border-2 border-blue-500/20 animate-ping"></div>
                      <div class="absolute inset-2 rounded-full border border-blue-400/30 animate-pulse"></div>
                      <div class="absolute inset-6 rounded-full bg-blue-600/10 border border-blue-500/30 flex items-center justify-center shadow-inner">
                        <span class="text-5xl animate-bounce select-none">💳</span>
                      </div>
                    </div>
                    <h3 class="text-2xl font-black text-slate-800 mt-8 tracking-wider uppercase">VUI LÒNG QUÉT THẺ</h3>
                    <p class="text-sm text-slate-400 font-extrabold mt-2 uppercase tracking-widest">Đặt thẻ gần vùng cảm biến đầu đọc</p>
                  </div>
                </transition>

                <!-- Card Scan Input Field -->
                <div class="space-y-3">
                  <div class="flex gap-3 relative w-full items-center">
                    <div class="relative flex items-center flex-1">
                      <input
                        ref="cardInputRef"
                        v-model="checkInCardNumber"
                        :disabled="lookupLoading"
                        @keydown.enter.prevent="handleCardLookup"
                        @blur="handleInputBlur"
                        inputmode="none"
                        class="w-full border border-slate-200 bg-slate-50 rounded-2xl pl-7 pr-16 py-5 focus:outline-none focus:ring-4 focus:ring-blue-500/10 focus:border-blue-500 focus:bg-white transition-all font-black text-2xl tracking-wide text-slate-800 text-center placeholder-slate-400"
                        placeholder=""
                      />
                      <div v-if="lookupLoading" class="absolute right-5 flex items-center pointer-events-none">
                        <span class="w-7 h-7 border-2 border-blue-600/30 border-t-blue-600 rounded-full animate-spin"></span>
                      </div>
                    </div>
                    <button
                      v-if="false"
                      @mousedown.prevent
                      @click="handleOpenManualForm"
                      :disabled="lookupLoading"
                      class="px-6 py-4 bg-blue-600 hover:bg-blue-500 text-white font-extrabold rounded-2xl transition-all duration-200 active:scale-95 shadow-md shadow-blue-500/10 shrink-0 flex items-center text-base"
                    >
                      Register
                    </button>
                  </div>
                  <p v-if="lookupMessage && !hasCardLookupResult" class="text-xs font-semibold mt-1 text-rose-600">
                    {{ lookupMessage }}
                  </p>
                </div>

              </div>

              <!-- Manual Form Mode -->
              <div v-else class="space-y-6 bg-slate-50 border border-slate-200/50 p-6 md:p-8 rounded-[32px] relative z-30 shadow-sm">
                <!-- Transition-group or switchable layout for steps -->
                <transition name="slide-fade" mode="out-in">
                  <!-- Step 1: User ID -->
                  <div v-if="manualStep === 1" key="step1" class="space-y-4">
                    <!-- User ID -->
                    <div class="flex flex-col gap-2">
                      <label class="text-xs text-slate-500 font-extrabold uppercase tracking-wider">Mã nhân viên / ID</label>
                      <input
                        v-model="formState.cardNumber"
                        class="w-full border border-slate-200 bg-white rounded-2xl px-5 py-4 focus:outline-none focus:ring-2 focus:ring-blue-300 transition-all font-semibold text-lg text-slate-800"
                        placeholder="Nhập mã nhân viên hoặc ID"
                      />
                    </div>
                  </div>

                  <!-- Step 2: Full Name, Company & User Type -->
                  <div v-else-if="manualStep === 2" key="step2" class="space-y-4">
                    <!-- Full Name -->
                    <div class="flex flex-col gap-2">
                      <label class="text-xs text-slate-500 font-extrabold uppercase tracking-wider">Họ và tên *</label>
                      <input
                        v-model="formState.fullName"
                        class="w-full border border-slate-200 bg-white rounded-2xl px-5 py-4 focus:outline-none focus:ring-2 focus:ring-blue-300 transition-all font-semibold text-lg text-slate-800"
                        placeholder="Nhập họ và tên"
                      />
                    </div>

                    <!-- Company -->
                    <div class="flex flex-col gap-2">
                      <label class="text-xs text-slate-500 font-extrabold uppercase tracking-wider">Công ty</label>
                      <input
                        v-model="formState.companyName"
                        class="w-full border border-slate-200 bg-white rounded-2xl px-5 py-4 focus:outline-none focus:ring-2 focus:ring-blue-300 transition-all font-semibold text-lg text-slate-800"
                        placeholder="Nhập tên công ty"
                      />
                    </div>
                    <!-- User Type -->
                    <div class="flex flex-col gap-2">
                      <label class="text-xs text-slate-500 font-extrabold uppercase tracking-wider">Loại đối tượng</label>
                      <div class="flex flex-wrap gap-3">
                        <button
                          v-for="item in userTypeItems"
                          :key="item.userTypeId"
                          type="button"
                          @click="selectUserType(item.userTypeId)"
                          class="flex-1 py-4 px-5 rounded-2xl border font-semibold text-base transition-all duration-200 active:scale-[0.98] text-center min-w-[120px]"
                          :class="[
                            formState.userTypeId === item.userTypeId
                              ? 'bg-blue-600 border-blue-600 text-white shadow-lg shadow-blue-500/20'
                              : 'bg-white border-slate-200 text-slate-700 hover:bg-slate-50 hover:border-slate-300'
                          ]"
                        >
                          {{ translateUserTypeName(item.userTypeName) }}
                        </button>
                      </div>
                    </div>
                  </div>

                  <!-- Step 3: Purpose -->
                  <div v-else-if="manualStep === 3" key="step3" class="space-y-4">
                    <!-- Purpose -->
                    <div class="flex flex-col gap-2">
                      <label class="text-xs text-slate-500 font-extrabold uppercase tracking-wider">Mục đích ra vào</label>
                      <div class="flex flex-wrap gap-3">
                        <button
                          v-for="item in purposeItems"
                          :key="item.purposeId || item.purposeName"
                          type="button"
                          @click="selectPurpose(item.purposeName)"
                          class="flex-1 py-4 px-5 rounded-2xl border font-semibold text-base transition-all duration-200 active:scale-[0.98] text-center min-w-[120px]"
                          :class="[
                            formState.purposeName === item.purposeName
                              ? 'bg-blue-600 border-blue-600 text-white shadow-lg shadow-blue-500/20'
                              : 'bg-white border-slate-200 text-slate-700 hover:bg-slate-50 hover:border-slate-300'
                          ]"
                        >
                          {{ translatePurposeName(item.purposeName) }}
                        </button>
                      </div>
                    </div>
                  </div>
                </transition>

                <!-- Form actions -->
                <div class="flex gap-4 pt-4 border-t border-slate-200 mt-4">
                  <button
                    @click="handleManualBack"
                    class="flex-1 py-3.5 border border-slate-200 bg-white hover:bg-slate-50 text-slate-600 rounded-2xl font-bold transition active:scale-[0.98] text-sm"
                  >
                    {{ manualStep === 1 ? 'Quay lại quét thẻ' : 'Quay lại' }}
                  </button>
                  <button
                    v-if="manualStep === 1"
                    @click="handleManualNext"
                    :disabled="checkingUser"
                    class="flex-1 py-3.5 bg-blue-600 hover:bg-blue-500 text-white rounded-2xl font-bold transition active:scale-[0.98] text-sm shadow-md shadow-blue-500/10 disabled:opacity-50 flex items-center justify-center gap-2"
                  >
                    <span v-if="checkingUser" class="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin"></span>
                    <span>Tiếp theo</span>
                  </button>
                </div>
              </div>
            </transition>
          </div>

          <!-- Step 2 Layout -->
          <div v-else class="w-full max-w-3xl mx-auto space-y-6 py-6 sm:py-10 text-center">

            <!-- Large HUD Camera View -->
            <div class="relative w-full checkin-camera-frame rounded-[32px] bg-slate-950 overflow-hidden border border-slate-200/80 shadow-lg">
              <img v-if="formState.photo" :src="`data:image/jpeg;base64,${formState.photo}`" class="w-full h-full object-cover" alt="captured selfie" />
              <div v-else-if="showCam" class="w-full h-full relative">
                <video ref="videoRef" autoplay playsinline muted class="w-full h-full object-cover" :class="[facing === 'user' ? 'scale-x-[-1]' : '', switching ? 'opacity-30' : 'opacity-100']"></video>
                
                <!-- HUD Corner Frame Indicators -->
                <div class="border-t-4 border-l-4 border-white/60 w-8 h-8 absolute top-6 left-6 pointer-events-none rounded-tl-lg"></div>
                <div class="border-t-4 border-r-4 border-white/60 w-8 h-8 absolute top-6 right-6 pointer-events-none rounded-tr-lg"></div>
                <div class="border-b-4 border-l-4 border-white/60 w-8 h-8 absolute bottom-6 left-6 pointer-events-none rounded-bl-lg"></div>
                <div class="border-b-4 border-r-4 border-white/60 w-8 h-8 absolute bottom-6 right-6 pointer-events-none rounded-br-lg"></div>
 
                <!-- Pulsing facial circle HUD guideline -->
                <div class="absolute inset-0 flex items-center justify-center pointer-events-none z-10">
                  <div class="border-2 border-dashed border-blue-400/50 rounded-full w-80 h-80 animate-pulse"></div>
                </div>

                <!-- Laser scanner line overlay sweep -->
                <div class="absolute left-0 right-0 h-0.5 bg-gradient-to-r from-transparent via-[#00df89] to-transparent scanner-line-sweep shadow-[0_0_12px_#00df89] pointer-events-none z-20"></div>
              </div>
              <div v-else class="w-full h-full flex flex-col items-center justify-center text-slate-400 gap-3">
                <span class="text-sm font-semibold">Camera đang tắt</span>
                <button @click="triggerCamera" class="px-5 py-2.5 rounded-xl bg-primary text-white text-xs font-bold active:scale-95 transition shadow-md shadow-primary/20">Mở Camera</button>
              </div>
              <canvas ref="canvasRef" style="display:none;"></canvas>
            </div>

            <p v-if="cameraError" class="text-xs text-rose-600 font-semibold">{{ cameraErrorMessage }}</p>
          </div>
        </transition>
      </div>
    </div>
  </div>
</template>

<style scoped>
.checkin-mobile-page {
  z-index: 1;
}

.checkin-shell {
  width: 100%;
}

.checkin-camera-frame {
  aspect-ratio: 1 / 1;
}

/* Slide fade transition frames */
.slide-fade-enter-active, .slide-fade-leave-active {
  transition: all 0.28s cubic-bezier(0.16, 1, 0.3, 1);
}
.slide-fade-enter-from {
  opacity: 0;
  transform: translateY(8px);
}
.slide-fade-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}
</style>




