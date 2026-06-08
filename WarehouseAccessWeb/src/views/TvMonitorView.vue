<script setup>
import { onMounted } from 'vue'
import { useRecords } from '../composables/useRecords'
import { useI18n } from '../composables/useI18n'

// Layout & Dashboard Components
import StatsGrid from '../components/dashboard/StatsGrid.vue'
import LiveMonitor from '../components/dashboard/LiveMonitor.vue'
import ToastContainer from '../components/common/ToastContainer.vue'
import WhitelistTicker from '../components/dashboard/WhitelistTicker.vue'

import logoImage from '../assets/logo-jiahsin-co-chu.png'

const { t } = useI18n()
const { loadLiveMonitor } = useRecords()

onMounted(async () => {
  // Initial load
  await loadLiveMonitor()
})
</script>

<template>
  <div class="min-h-screen bg-slate-50 text-slate-800 flex flex-col antialiased">
    <!-- Centered Header for TV screen -->
    <header class="w-full bg-gradient-to-r from-[#0a3575] to-[#0e4391] text-white py-4 border-b-[3.5px] border-accent shadow-xl">
      <div class="w-full px-6 lg:px-8 flex justify-center items-center">
        <div class="flex items-center gap-4 min-w-0">
          <img class="h-9 w-auto object-contain" :src="logoImage" alt="JHV" />
          <div class="w-[1.5px] h-9 bg-white/25"></div>
          <div class="font-sans leading-tight text-left">
            <h1 class="text-xl font-extrabold tracking-wide m-0 uppercase">{{ t.sysTitle }}</h1>
            <p class="text-sm text-white/65 font-medium mt-0.5">{{ t.sysSub }}</p>
          </div>  
        </div>
      </div>
    </header>

    <!-- Main Content Area -->
    <main class="flex-1 min-w-0 bg-slate-50 px-6 py-8 flex flex-col">
      <!-- Whitelist Auto-Scrolling Roster -->
      <WhitelistTicker />

      <!-- Metric Summary Stats -->
      <StatsGrid />
 
      <!-- Active View Panel -->
      <div class="flex-1 mt-6">
        <LiveMonitor :interactive="false" />
      </div>
    </main>

    <!-- Floating Global Toasts Alerts -->
    <ToastContainer />
  </div>
</template>

<style scoped>
/* Premium Slide transitions */
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.22s ease, transform 0.22s cubic-bezier(0.16, 1, 0.3, 1);
}
.fade-enter-from {
  opacity: 0;
  transform: translateY(8px);
}
.fade-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}
</style>
