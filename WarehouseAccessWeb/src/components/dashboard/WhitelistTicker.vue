<script setup>
import { onMounted, onUnmounted, computed } from 'vue';
import { useI18n } from '../../composables/useI18n';
import { useRecords } from '../../composables/useRecords';

const { t } = useI18n();
const { 
  whitelistUsers, 
  whitelistUsersLoading, 
  loadWhitelistUsers,
  loadUserDepartments,
  userDepartmentOptions
} = useRecords();

let pollTimer = null;

onMounted(async () => {
  await loadUserDepartments();
  await loadWhitelistUsers();
  // Poll whitelist users list every 10 seconds to keep it updated dynamically
  pollTimer = setInterval(async () => {
    await loadWhitelistUsers();
  }, 10000);
});

onUnmounted(() => {
  if (pollTimer) clearInterval(pollTimer);
});

// Duplicate the items list to ensure seamless horizontal scrolling loop
const scrollingItems = computed(() => {
  const list = whitelistUsers.value || [];
  if (list.length === 0) return [];
  // If list is small, repeat it a few times to fill the horizontal track
  if (list.length < 8) {
    return [...list, ...list, ...list, ...list];
  }
  return [...list, ...list];
});

function getInitialsColor(name) {
  if (!name) return 'bg-indigo-100 text-indigo-700';
  const charCodeSum = name.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
  const colors = [
    'bg-indigo-50 text-indigo-700 border-indigo-100',
    'bg-emerald-50 text-emerald-700 border-emerald-100',
    'bg-sky-50 text-sky-700 border-sky-100',
    'bg-amber-50 text-amber-700 border-amber-100',
    'bg-rose-50 text-rose-700 border-rose-100',
    'bg-purple-50 text-purple-700 border-purple-100'
  ];
  return colors[charCodeSum % colors.length];
}

const targetDeptIds = ['B1FGW', 'B1QIP', 'SEC', 'IT'];

const deptCounts = computed(() => {
  const counts = { B1FGW: 0, B1QIP: 0, SEC: 0, IT: 0 };
  const list = whitelistUsers.value || [];
  for (const user of list) {
    const deptId = (user.departmentId || '').toUpperCase().trim();
    if (targetDeptIds.includes(deptId)) {
      counts[deptId]++;
    }
  }
  return counts;
});

const deptNames = computed(() => {
  const names = {
    B1FGW: 'B1FGW',
    B1QIP: 'B1QIP',
    SEC: 'SEC',
    IT: 'IT'
  };
  
  const options = userDepartmentOptions.value || [];
  for (const opt of options) {
    const deptId = (opt.departmentCode || '').toUpperCase().trim();
    if (targetDeptIds.includes(deptId) && opt.departmentName) {
      names[deptId] = opt.departmentName;
    }
  }
  
  const list = whitelistUsers.value || [];
  for (const user of list) {
    const deptId = (user.departmentId || '').toUpperCase().trim();
    if (targetDeptIds.includes(deptId) && user.departmentName && names[deptId] === deptId) {
      names[deptId] = user.departmentName;
    }
  }
  
  return names;
});
</script>

<template>
  <div v-if="whitelistUsers.length > 0" class="space-y-2 mb-6">
    <!-- Header Indicator & Stats -->
    <div class="flex flex-wrap items-center gap-x-4 gap-y-2 px-1 text-[12px] font-black text-slate-400 tracking-wider uppercase">
      <div class="flex items-center gap-2">
        <!-- Pulsing Radar green dot -->
        <span class="relative flex h-2 w-2">
          <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75"></span>
          <span class="relative inline-flex rounded-full h-2 w-2 bg-emerald-500"></span>
        </span>
        <span>{{ t.securedWhitelistRoster }}</span>
      </div>

      <span class="text-slate-300/80 hidden sm:inline">|</span>

      <!-- Department Stats Roster -->
      <div class="flex flex-wrap items-center gap-2">
        <span 
          v-for="deptId in targetDeptIds" 
          :key="deptId" 
          class="bg-slate-100 border border-slate-200 px-1.5 py-0.5 rounded text-[12px] font-black tracking-normal flex items-center gap-1 text-slate-500"
        >
          <span>{{ deptNames[deptId] }}:</span>
          <span class="text-primary font-extrabold">{{ deptCounts[deptId] }}</span>
        </span>
      </div>
    </div>
    <!-- Scrolling Ticker Container -->
    <div class="relative w-full bg-white/70 backdrop-blur-md border border-slate-200/60 rounded-2xl p-3.5 overflow-hidden shadow-sm flex items-center glassmorphism">
      <!-- Gradient masks on sides for premium fade effect -->
      <div class="absolute left-0 top-0 bottom-0 w-16 bg-gradient-to-r from-white to-transparent z-10 pointer-events-none"></div>
      <div class="absolute right-0 top-0 bottom-0 w-16 bg-gradient-to-l from-white to-transparent z-10 pointer-events-none"></div>

      <!-- Scrolling Track -->
      <div class="marquee-track-container w-full overflow-hidden">
        <div class="animate-marquee flex gap-4 py-1.5">
          <div 
            v-for="(user, idx) in scrollingItems" 
            :key="`${user.userId}-${idx}`"
            class="bg-white border border-t-0 border-slate-200/80 rounded-xl shadow-sm hover-lift flex flex-col relative overflow-hidden w-80 h-32 shrink-0 cursor-default select-none"
          >
            <!-- Card Header Border Accent (Jia Hsin Royal Blue gradient) -->
            <div class="h-1.5 w-full bg-gradient-to-r from-primary to-primary-light"></div>

            <!-- Card Body: Avatar & details -->
            <div class="p-4 flex-1 flex items-center">
              <div class="flex items-center gap-4 text-left w-full">
                <!-- Vertical Avatar Frame (4:5 Ratio) -->
                <div class="w-16 h-20 rounded-xl border border-slate-200 bg-slate-50 flex items-center justify-center overflow-hidden shrink-0 shadow-inner">
                  <img 
                    v-if="user.avatar" 
                    :src="`data:image/jpeg;base64,${user.avatar}`" 
                    alt="avatar" 
                    class="w-full h-full object-cover"
                  />
                  <div v-else :class="['w-full h-full flex items-center justify-center text-2xl font-black uppercase rounded-xl', getInitialsColor(user.fullName)]">
                    {{ (user.fullName || '?').slice(0, 1) }}
                  </div>
                </div>

                <!-- Identity Details -->
                <div class="flex-1 min-w-0 pr-2">
                  <h4 class="text-base font-extrabold text-slate-800 truncate leading-tight">{{ user.fullName || 'Unknown' }}</h4>
                  <p class="text-xs font-bold text-slate-500 mt-1 truncate">
                    ID: {{ user.userId || '-' }}
                  </p>
                  <p class="text-xs font-bold text-slate-500 mt-0.5 uppercase">
                    Bộ phận: {{ user.departmentName || 'N/A' }}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
@keyframes marquee {
  0% {
    transform: translateX(0%);
  }
  100% {
    transform: translateX(-50%);
  }
}

.animate-marquee {
  display: flex;
  width: max-content;
  animation: marquee 400s linear infinite;
}

.animate-marquee:hover {
  animation-play-state: paused;
}

/* Glassmorphism fallback if not supported */
.glassmorphism {
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
}
</style>
