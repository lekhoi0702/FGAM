<script setup>
import { useI18n } from '../../composables/useI18n';
import { useRecords } from '../../composables/useRecords';

const { t } = useI18n();
const { stats } = useRecords();

defineEmits(['select-monitor', 'select-history']);
</script>

<template>
  <section class="grid grid-cols-1 sm:grid-cols-2 gap-4 max-w-md mb-6">
    <!-- Active On-site Visitors -->
    <div 
      class="bg-white border border-slate-200/80 rounded-xl py-2.5 px-4 shadow-sm hover-lift cursor-pointer flex items-center justify-center gap-3"
      @click="$emit('select-monitor')"
    >
      <span class="text-3xl font-extrabold text-emerald-500 leading-none">{{ stats.onSite }}</span>
      <span class="text-xs font-bold text-slate-500 uppercase tracking-wider">{{ t.statsInside }}</span>
    </div>

    <!-- Today Visitors -->
    <div 
      class="bg-white border border-slate-200/80 rounded-xl py-2.5 px-4 shadow-sm hover-lift cursor-pointer flex items-center justify-center gap-3"
      @click="$emit('select-history')"
    >
      <span class="text-3xl font-extrabold text-indigo-400 leading-none">{{ stats.today }}</span>
      <span class="text-xs font-bold text-slate-500 uppercase tracking-wider">{{ t.statsToday }}</span>
    </div>
  </section>
</template>
