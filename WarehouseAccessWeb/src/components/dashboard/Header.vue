<script setup>
import { computed } from 'vue';
import { useI18n } from '../../composables/useI18n';
import { useAuthState } from '../../stores/auth.store';
import logoImage from '../../assets/logo-jiahsin-co-chu.png';

const { lang, t, setLanguage } = useI18n();
const authState = useAuthState();

const loginUserDisplay = computed(() => {
  const userId = authState.currentUser?.userId || '-';
  const fullName = authState.currentUser?.fullName || '-';
  return `${userId} - ${fullName}`;
});

defineEmits(['open-checkin', 'logout']);
</script>

<template>
  <header class="w-full bg-gradient-to-r from-[#0a3575] to-[#0e4391] text-white py-4 border-b-[3.5px] border-accent shadow-xl">
    <div class="w-full px-6 lg:px-8 flex justify-between items-center">
      <div class="flex items-center gap-4 min-w-0">
        <img class="h-9 w-auto object-contain" :src="logoImage" alt="JHV" />
        <div class="w-[1.5px] h-9 bg-white/25"></div>
        <div class="font-sans leading-tight text-left">
          <h1 class="text-xl font-extrabold tracking-wide m-0 uppercase">{{ t.sysTitle }}</h1>
          <p class="text-sm text-white/65 font-medium mt-0.5">{{ t.sysSub }}</p>
        </div>  
      </div>

      <div class="flex items-center gap-4">
        <div class="hidden md:flex items-center rounded-lg border border-white/20 bg-white/10 px-3 py-1.5">
          <span class="text-sm font-bold text-white/95 whitespace-nowrap">{{ loginUserDisplay }}</span>
        </div>

        <div class="flex bg-white/10 p-0.5 rounded-lg border border-white/15">
          <button
            v-for="(label, lCode) in { zh: 'ZH', en: 'EN', vi: 'VI' }"
            :key="lCode"
            :class="[
              'px-3 py-1 text-sm font-bold rounded-md transition-all duration-200',
              lang === lCode ? 'bg-white text-primary shadow-sm' : 'bg-transparent text-white/80 hover:text-white'
            ]"
            @click="setLanguage(lCode)"
          >
            {{ label }}
          </button>
        </div>

        <button
          class="bg-white hover:bg-blue-50 text-primary text-sm font-extrabold px-5 py-2.5 rounded-lg hover:translate-y-[-1px] transition-all duration-200 shadow-sm active:scale-95"
          @click="$emit('open-checkin')"
        >
          {{ t.checkInBtn }}
        </button>

        <button
          class="bg-red-600 hover:bg-red-500 text-white text-sm font-extrabold px-4 py-2.5 rounded-lg transition-all duration-200 shadow-sm active:scale-95"
          @click="$emit('logout')"
        >
          {{ t.logoutBtn }}
        </button>
      </div>
    </div>
  </header>
</template>

