<script setup>
import { computed } from 'vue';

const props = defineProps({
  currentPage: { type: Number, required: true },
  pageSize: { type: Number, default: 10 },
  totalItems: { type: Number, required: true }
});

const emit = defineEmits(['update:currentPage']);

const totalPages = computed(() => Math.max(1, Math.ceil(props.totalItems / props.pageSize)));

const showingFrom = computed(() => props.totalItems === 0 ? 0 : (props.currentPage - 1) * props.pageSize + 1);
const showingTo = computed(() => Math.min(props.currentPage * props.pageSize, props.totalItems));

const visiblePages = computed(() => {
  const pages = [];
  const total = totalPages.value;
  const current = props.currentPage;
  if (total <= 7) {
    for (let i = 1; i <= total; i++) pages.push(i);
  } else {
    pages.push(1);
    if (current > 3) pages.push('...');
    const start = Math.max(2, current - 1);
    const end = Math.min(total - 1, current + 1);
    for (let i = start; i <= end; i++) pages.push(i);
    if (current < total - 2) pages.push('...');
    pages.push(total);
  }
  return pages;
});

function goToPage(page) {
  if (page >= 1 && page <= totalPages.value && page !== props.currentPage) {
    emit('update:currentPage', page);
  }
}
</script>

<template>
  <div class="px-5 py-4 bg-slate-50/70 border-t border-slate-100 text-xs text-slate-500 font-semibold flex flex-col sm:flex-row items-center justify-between gap-4 select-none">
    <span class="order-2 sm:order-1 text-slate-500 text-xs">
      Showing 
      <strong class="text-slate-800 font-bold">{{ showingFrom }}</strong>
      to 
      <strong class="text-slate-800 font-bold">{{ showingTo }}</strong>
      of 
      <strong class="text-slate-800 text-sm font-black">{{ totalItems }}</strong> 
      records
    </span>

    <div class="flex items-center gap-1.5 order-1 sm:order-2">
      <!-- Previous page button -->
      <button 
        @click="goToPage(currentPage - 1)" 
        :disabled="currentPage === 1"
        class="px-2.5 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 active:scale-95 transition-all duration-150 disabled:opacity-50 disabled:cursor-not-allowed disabled:active:scale-100 flex items-center justify-center text-slate-600 shadow-sm"
        title="Previous Page"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7" />
        </svg>
      </button>

      <!-- Page numbers -->
      <template v-for="(p, idx) in visiblePages" :key="`p-${idx}`">
        <span v-if="p === '...'" class="px-2 text-slate-400 font-bold">...</span>
        <button 
          v-else
          @click="goToPage(p)"
          :class="[
            'min-w-[32px] h-[32px] px-2 rounded-lg text-xs font-bold transition-all duration-150 active:scale-95 shadow-sm border',
            p === currentPage
              ? 'bg-primary text-white border-primary hover:bg-primary-dark'
              : 'bg-white text-slate-600 border-slate-200 hover:bg-slate-50'
          ]"
        >
          {{ p }}
        </button>
      </template>

      <!-- Next page button -->
      <button 
        @click="goToPage(currentPage + 1)" 
        :disabled="currentPage === totalPages"
        class="px-2.5 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 active:scale-95 transition-all duration-150 disabled:opacity-50 disabled:cursor-not-allowed disabled:active:scale-100 flex items-center justify-center text-slate-600 shadow-sm"
        title="Next Page"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
        </svg>
      </button>
    </div>
  </div>
</template>
