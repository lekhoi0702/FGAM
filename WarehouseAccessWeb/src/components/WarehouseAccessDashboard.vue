<script setup>
import { ref, watch, onMounted, onBeforeUnmount, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useRecords } from '../composables/useRecords'
import { logout, useAuthState } from '../stores/auth.store'

// Layout & Dashboard Components
import Header from './dashboard/Header.vue'
import StatsGrid from './dashboard/StatsGrid.vue'
import LiveMonitor from './dashboard/LiveMonitor.vue'
import HistoryLog from './dashboard/HistoryLog.vue'
import ToastContainer from './common/ToastContainer.vue'
import WhitelistTicker from './dashboard/WhitelistTicker.vue'

// Settings Config Modules
import EmployeesConfig from './settings/EmployeesConfig.vue'
import FactoriesConfig from './settings/FactoriesConfig.vue'
import UserTypesConfig from './settings/UserTypesConfig.vue'
import DepartmentsConfig from './settings/DepartmentsConfig.vue'
import PurposesConfig from './settings/PurposesConfig.vue'

import DetailModal from './modals/DetailModal.vue'

const props = defineProps({
  initialTab: { type: String, default: 'monitor' },
  initialSettingsTab: { type: String, default: 'employees' }
})

const router = useRouter()
const {
  loadUserDepartments,
  loadDepartmentsCrud,
  loadPurposesCrud,
  loadLiveMonitor,
  loadHistoryRecords
} = useRecords()

const activeTab = ref(props.initialTab)
const settingsActiveTab = ref(props.initialSettingsTab)

const SIDEBAR_COLLAPSE_STORAGE_KEY = 'wa_sidebar_collapsed'
const MOBILE_BREAKPOINT_PX = 1024

const sidebarCollapsed = ref(localStorage.getItem(SIDEBAR_COLLAPSE_STORAGE_KEY) === '1')
const isMobileView = ref(false)
const mobileSidebarOpen = ref(false)

const selectedRecord = ref(null)

const authState = useAuthState()

const sidebarSections = computed(() => {
  const sections = [
    {
      code: 'operations',
      label: 'Operations',
      items: [
        { code: 'monitor', label: 'Live Monitor', icon: 'monitor', path: '/monitor' },
        { code: 'history', label: 'History', icon: 'history', path: '/history' }
      ]
    }
  ]

  const status = authState.currentUser?.recordStatus || authState.currentUser?.RecordStatus
  if (String(status) === '2') {
    sections.push({
      code: 'master-data',
      label: 'Master Data',
      items: [
        { code: 'settings', label: 'Master Data', icon: 'settings', path: '/settings/employees' }
      ]
    })
  }

  return sections
})

const settingsTabs = [
  { code: 'employees', label: 'Employees' },
  { code: 'factory', label: 'Factory' },
  { code: 'userTypes', label: 'User Types' },
  { code: 'departments', label: 'Department' },
  { code: 'purposes', label: 'Purpose' }
]

// Keep refs synced with route updates
watch(() => props.initialTab, (newTab) => {
  activeTab.value = newTab
})

watch(() => props.initialSettingsTab, (newSettingsTab) => {
  settingsActiveTab.value = newSettingsTab
})

watch(
  activeTab,
  async (tabCode) => {
    if (tabCode === 'monitor') {
      await loadLiveMonitor()
      return
    }

    if (tabCode === 'history') {
      await loadHistoryRecords()
    }
  },
  { immediate: true }
)

function updateViewportState() {
  isMobileView.value = window.innerWidth <= MOBILE_BREAKPOINT_PX
  if (isMobileView.value) {
    mobileSidebarOpen.value = false
  }
}

function toggleSidebarCollapsed() {
  sidebarCollapsed.value = !sidebarCollapsed.value
  localStorage.setItem(SIDEBAR_COLLAPSE_STORAGE_KEY, sidebarCollapsed.value ? '1' : '0')
}

function handleSidebarToggle() {
  if (isMobileView.value) {
    mobileSidebarOpen.value = !mobileSidebarOpen.value
  } else {
    toggleSidebarCollapsed()
  }
}

function isSidebarPageActive(pageCode) {
  if (pageCode.startsWith('settings:')) {
    const settingCode = pageCode.split(':')[1]
    return activeTab.value === 'settings' && settingsActiveTab.value === settingCode
  }
  return activeTab.value === pageCode
}

function onSidebarPageClick(item) {
  if (isMobileView.value) {
    mobileSidebarOpen.value = false
  }
  router.push(item.path)
}

function openSettingsPanel() {
  router.push('/settings/employees')
}

function openCheckInTerminal() {
  router.push('/check-in-mobile')
}

function handleLogout() {
  logout()
  router.replace('/login')
}

function openDetail(record) {
  selectedRecord.value = record
}

async function handleCheckInSuccess() {
  await Promise.all([
    loadLiveMonitor(),
    loadHistoryRecords()
  ])
}

onMounted(async () => {
  updateViewportState()
  window.addEventListener('resize', updateViewportState)

  // Prefetch settings data for check-in dropdowns
  await Promise.all([
    loadUserDepartments(),
    loadDepartmentsCrud(),
    loadPurposesCrud()
  ])
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', updateViewportState)
})
</script>

<template>
  <div class="min-h-screen bg-slate-50 text-slate-800 flex flex-col antialiased">
    <!-- Top Header -->
    <Header 
      @open-settings="openSettingsPanel"
      @open-checkin="openCheckInTerminal"
      @logout="handleLogout"
    />

    <!-- Layout Wrapper -->
    <div class="flex-1 flex relative">
      <!-- Mobile Sidebar Burger Toggle -->
      <button 
        v-if="isMobileView" 
        @click="mobileSidebarOpen = !mobileSidebarOpen"
        class="fixed bottom-6 right-6 w-14 h-14 bg-[#0e4391] hover:bg-[#0a3575] text-white rounded-full shadow-2xl flex items-center justify-center z-40 transition-transform active:scale-90"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M4 6h16M4 12h16M4 18h16" />
        </svg>
      </button>

      <!-- Sidebar Overlay (Mobile) -->
      <div 
        v-if="isMobileView && mobileSidebarOpen" 
        @click="mobileSidebarOpen = false"
        class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm z-40"
      ></div>

      <!-- Navigation Sidebar -->
      <aside
        :class="[
          'bg-slate-900 text-slate-400 flex flex-col transition-all duration-300 z-50 shrink-0 border-r border-slate-800 shadow-2xl',
          isMobileView 
            ? 'fixed top-0 bottom-0 left-0 w-64 shadow-2xl transform transition-transform duration-300' 
            : sidebarCollapsed ? 'w-20' : 'w-64',
          isMobileView && !mobileSidebarOpen ? '-translate-x-full' : 'translate-x-0'
        ]"
      >
        <!-- Sidebar Brand Banner -->
        <div 
          :class="[
            'border-b border-slate-800 flex items-center transition-all duration-300',
            sidebarCollapsed && !isMobileView ? 'p-4 justify-center' : 'p-6 justify-between'
          ]"
        >
          <div v-if="!sidebarCollapsed || isMobileView" class="flex items-center gap-3 overflow-hidden">
            <img src="/jhv-Photoroom.png" alt="JHV Logo" class="w-8 h-8 object-contain shrink-0" />
            <div class="leading-none text-left">
              <span class="text-sm font-black text-white block">JIAHSIN CO., LTD.</span>
              <span class="text-xs text-slate-400 font-semibold mt-0.5 block">Access System</span>
            </div>
          </div>
          
          <!-- Animated Hamburger Button inside Sidebar Header -->
          <button 
            @click="handleSidebarToggle"
            class="flex flex-col justify-center items-center w-8 h-8 rounded-lg hover:bg-slate-800/50 active:scale-95 transition-all duration-200 focus:outline-none relative shrink-0"
            title="Toggle Sidebar"
          >
            <span 
              :class="[
                'w-4 h-0.5 bg-slate-400 rounded-full transition-all duration-300 origin-center',
                (isMobileView ? mobileSidebarOpen : !sidebarCollapsed) ? 'rotate-45 translate-y-[4.5px]' : ''
              ]"
            ></span>
            <span 
              :class="[
                'w-4 h-0.5 bg-slate-400 rounded-full my-[3px] transition-all duration-200',
                (isMobileView ? mobileSidebarOpen : !sidebarCollapsed) ? 'opacity-0 scale-x-0' : 'opacity-100'
              ]"
            ></span>
            <span 
              :class="[
                'w-4 h-0.5 bg-slate-400 rounded-full transition-all duration-300 origin-center',
                (isMobileView ? mobileSidebarOpen : !sidebarCollapsed) ? '-rotate-45 -translate-y-[4.5px]' : ''
              ]"
            ></span>
          </button>
        </div>
 
        <!-- Navigation Lists -->
        <nav class="flex-1 p-4 space-y-6 overflow-y-auto">
          <div v-for="section in sidebarSections" :key="section.code" class="space-y-1 text-left">
            <span 
              v-if="!sidebarCollapsed || isMobileView" 
              class="px-3 text-xs font-bold text-slate-400 uppercase tracking-widest block mb-2"
            >
              {{ section.label }}
            </span>
            <button
              v-for="item in section.items"
              :key="item.code"
              @click="onSidebarPageClick(item)"
              :class="[
                'w-full px-3 py-2.5 rounded-xl text-sm font-semibold flex items-center gap-3 transition-all duration-150',
                isSidebarPageActive(item.code)
                  ? 'bg-primary text-white shadow-lg shadow-primary/20'
                  : 'text-slate-400 hover:text-white hover:bg-slate-800/50'
              ]"
              :title="sidebarCollapsed && !isMobileView ? item.label : ''"
            >
              <span class="text-base shrink-0 flex items-center justify-center">
                <svg v-if="item.code === 'monitor'" xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  <path stroke-linecap="round" stroke-linejoin="round" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                </svg>
                <svg v-else-if="item.code === 'history'" xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 002-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
                </svg>
                <svg v-else-if="item.code === 'settings'" xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                  <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <span v-else>{{ item.icon }}</span>
              </span>
              <span v-if="!sidebarCollapsed || isMobileView" class="truncate">{{ item.label }}</span>
            </button>
          </div>
        </nav>
      </aside>
 
      <!-- Main Panel View Container -->
      <main class="flex-1 min-w-0 bg-slate-50 px-6 py-8 flex flex-col">
        <!-- Whitelist Auto-Scrolling Roster -->
        <WhitelistTicker v-if="activeTab === 'monitor'" />

        <!-- Metric Summary Stats (Only visible in Operations pages) -->
        <StatsGrid 
          v-if="activeTab === 'monitor' || activeTab === 'history'"
          @select-monitor="router.push('/monitor')"
          @select-history="router.push('/history')"
        />
 
        <!-- Active View Panel -->
        <div class="flex-1">
          <transition name="fade" mode="out-in">
            <!-- Operations Monitor -->
            <LiveMonitor 
              v-if="activeTab === 'monitor'" 
              @detail="openDetail"
            />
            
            <!-- Operations History -->
            <HistoryLog 
              v-else-if="activeTab === 'history'"
              @detail="openDetail"
            />
 
            <!-- Configuration Panels Layout -->
            <div v-else-if="activeTab === 'settings'" class="bg-white border border-slate-200/80 rounded-2xl p-6 shadow-sm flex flex-col h-full text-left">
              <!-- Secondary Settings Tab Row -->
              <nav class="flex flex-wrap gap-2 mb-6 border-b border-slate-100 pb-3">
                <button
                  v-for="tab in settingsTabs"
                  :key="tab.code"
                  @click="router.push(`/settings/${tab.code === 'employees' ? 'employees' : tab.code === 'factory' ? 'factories' : tab.code === 'userTypes' ? 'user-types' : tab.code === 'departments' ? 'departments' : tab.code === 'purposes' ? 'purposes' : 'employees'}`)"
                  :class="[
                    'px-4 py-2 text-sm font-bold rounded-lg border transition duration-150',
                    settingsActiveTab === tab.code
                      ? 'bg-primary text-white border-primary'
                      : 'bg-white text-slate-600 border-slate-200 hover:bg-slate-50'
                  ]"
                >
                  {{ tab.label }}
                </button>
              </nav>

              <!-- Settings View Panel Component Switcher -->
              <div class="flex-1">
                <EmployeesConfig v-if="settingsActiveTab === 'employees'" />
                <FactoriesConfig v-else-if="settingsActiveTab === 'factory'" />
                <UserTypesConfig v-else-if="settingsActiveTab === 'userTypes'" />
                <DepartmentsConfig v-else-if="settingsActiveTab === 'departments'" />
                <PurposesConfig v-else-if="settingsActiveTab === 'purposes'" />
              </div>
            </div>
          </transition>
        </div>
      </main>
    </div>

    <!-- Modals Layer -->
    <transition name="modal">
      <DetailModal 
        v-slot="{}"
        v-if="selectedRecord"
        :record="selectedRecord"
        @close="selectedRecord = null"
      />
    </transition>

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
