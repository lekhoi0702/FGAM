<script setup>
const props = defineProps({
  searchText: { type: String, default: '' },
  searchPlaceholder: { type: String, default: 'Search...' },
  totalLabel: { type: String, default: '' },
  showingLabel: { type: String, default: '' },
  hideFormPanel: { type: Boolean, default: false }
})

const emit = defineEmits(['update:searchText'])

function onSearchInput(event) {
  emit('update:searchText', event?.target?.value || '')
}
</script>

<template>
  <div :class="props.hideFormPanel ? 'grid grid-cols-1 gap-6 text-left ui-text-sm' : 'grid grid-cols-1 md:grid-cols-[1fr_2fr] gap-6 text-left ui-text-sm'">
    <div v-if="!props.hideFormPanel" class="bg-slate-50 border border-slate-200/60 rounded-2xl p-5 space-y-4 h-fit">
      <slot name="form" />
    </div>

    <div class="bg-white rounded-2xl border border-slate-200/80 shadow-sm overflow-hidden glassmorphism h-fit">
      <div class="flex flex-col sm:flex-row gap-3 items-center justify-between p-4 bg-slate-50/70 border-b border-slate-100">
        <div class="flex flex-wrap items-center gap-3 w-full">
          <input
            :value="props.searchText"
            @input="onSearchInput"
            :placeholder="props.searchPlaceholder"
            class="bg-white border border-slate-200 rounded-lg px-4 py-2.5 ui-input outline-none focus:border-primary/50 min-w-[280px] sm:min-w-[360px] lg:min-w-[480px]"
          />
          <slot name="actions" />
        </div>
      </div>

      <slot name="table" />

      <div v-if="props.totalLabel || props.showingLabel" class="px-5 py-4 bg-slate-50/70 border-t border-slate-100 text-xs text-slate-500 font-semibold flex items-center justify-between">
        <span>{{ props.totalLabel }}</span>
        <span class="text-slate-400">{{ props.showingLabel }}</span>
      </div>
    </div>

    <!-- Default slot for modals and floating overlays -->
    <slot />
  </div>
</template>
