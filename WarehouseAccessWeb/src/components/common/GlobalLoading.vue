<script setup>
import { computed } from 'vue'
import { useLoadingState } from '../../stores/loading.store'
import { useI18n } from '../../composables/useI18n'

const state = useLoadingState()
const { lang } = useI18n()

const defaultText = computed(() => {
  if (lang.value === 'zh') return '處理中...'
  if (lang.value === 'en') return 'Processing...'
  return 'Đang xử lý...'
})

const subText = computed(() => {
  if (lang.value === 'zh') return '請稍候'
  if (lang.value === 'en') return 'Please wait a moment'
  return 'Vui lòng đợi trong giây lát'
})
</script>

<template>
  <transition name="fade">
    <div 
      v-if="state.isLoading" 
      class="fixed inset-0 z-[9999] flex flex-col items-center justify-center bg-slate-900/50 backdrop-blur-[3px] select-none"
    >
      <!-- Premium Glassmorphism Card Wrapper -->
      <div class="bg-white/95 border border-slate-200/60 rounded-3xl p-8 max-w-xs w-11/12 shadow-2xl flex flex-col items-center text-center space-y-4 animate-scale-up">
        <!-- Dual ring glowing spinner (Jia Hsin Royal Blue & Mint Green) -->
        <div class="relative flex items-center justify-center w-16 h-16">
          <span class="animate-ping absolute inline-flex h-12 w-12 rounded-full bg-primary/10 opacity-75"></span>
          <!-- outer ring -->
          <div class="absolute w-12 h-12 border-[3.5px] border-slate-100 border-t-primary rounded-full animate-spin"></div>
          <!-- inner opposite ring -->
          <div class="absolute w-8 h-8 border-2 border-slate-100 border-b-accent rounded-full animate-spin-reverse"></div>
        </div>

        <div class="space-y-1">
          <h3 class="text-sm font-black text-slate-800 tracking-wider uppercase">
            {{ state.loadingText || defaultText }}
          </h3>
          <p class="text-xs font-bold text-slate-400">
            {{ subText }}
          </p>
        </div>
      </div>
    </div>
  </transition>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.22s ease-out;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.animate-scale-up {
  animation: scaleUp 0.26s cubic-bezier(0.34, 1.56, 0.64, 1) forwards;
}

@keyframes scaleUp {
  from {
    opacity: 0;
    transform: scale(0.92);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

.animate-spin-reverse {
  animation: spin-reverse 1.2s linear infinite;
}

@keyframes spin-reverse {
  from {
    transform: rotate(360deg);
  }
  to {
    transform: rotate(0deg);
  }
}
</style>
