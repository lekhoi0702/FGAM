<script setup>
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'

const props = defineProps({
  modelValue: {
    type: [String, Number],
    default: ''
  },
  items: {
    type: Array,
    default: () => []
  },
  valueKey: {
    type: String,
    default: 'value'
  },
  labelKey: {
    type: String,
    default: 'label'
  },
  placeholder: {
    type: String,
    default: 'Select...'
  },
  searchPlaceholder: {
    type: String,
    default: 'Search...'
  },
  disabled: {
    type: Boolean,
    default: false
  },
  searchable: {
    type: Boolean,
    default: false
  },
  clearable: {
    type: Boolean,
    default: true
  },
  large: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:modelValue'])

const dropdownOpen = ref(false)
const dropdownRef = ref(null)
const searchKeyword = ref('')

const normalizedItems = computed(() => {
  return (props.items || []).map((item) => ({
    value: item?.[props.valueKey] ?? '',
    label: item?.[props.labelKey] ?? ''
  }))
})

const selectedLabel = computed(() => {
  const selected = normalizedItems.value.find((item) => String(item.value) === String(props.modelValue ?? ''))
  return selected?.label || props.placeholder
})

const filteredItems = computed(() => {
  if (!props.searchable) return normalizedItems.value
  const keywordValue = searchKeyword.value.trim().toLowerCase()
  if (!keywordValue) return normalizedItems.value
  return normalizedItems.value.filter((item) =>
    String(item.label || '').toLowerCase().includes(keywordValue) ||
    String(item.value || '').toLowerCase().includes(keywordValue)
  )
})

function toggleDropdown() {
  if (props.disabled) return
  dropdownOpen.value = !dropdownOpen.value
}

function selectValue(value) {
  emit('update:modelValue', value)
  dropdownOpen.value = false
}

function handleClickOutside(event) {
  if (!dropdownRef.value) return
  if (!dropdownRef.value.contains(event.target)) {
    dropdownOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside)
})

watch(() => props.modelValue, () => {
  // keep typed search lightweight across selections
  if (!dropdownOpen.value) {
    searchKeyword.value = ''
  }
})
</script>

<template>
  <div ref="dropdownRef" class="relative z-[120]">
    <button
      type="button"
      :disabled="disabled"
      class="w-full bg-white border border-slate-200 text-left outline-none focus:border-[#0e4391] focus:ring-4 focus:ring-[#0e4391]/10 focus:shadow-[0_0_15px_rgba(14,67,145,0.15)] flex items-center justify-between disabled:opacity-50 transition-all duration-300"
      :class="[large ? 'rounded-2xl px-5 py-4 text-lg font-semibold text-slate-800' : 'rounded-xl px-4 py-3 md:py-3.5 text-sm md:text-base']"
      @click="toggleDropdown"
    >
      <span class="truncate">{{ selectedLabel }}</span>
      <svg xmlns="http://www.w3.org/2000/svg" class="text-slate-400 shrink-0 transition-transform duration-200" :class="[large ? 'h-5 w-5' : 'h-4 w-4', dropdownOpen ? 'rotate-180' : '']" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
        <path stroke-linecap="round" stroke-linejoin="round" d="M19 9l-7 7-7-7" />
      </svg>
    </button>

    <transition name="dropdown-fade">
      <div v-if="dropdownOpen" class="absolute z-[9999] mt-2 w-full border border-slate-200 bg-white shadow-xl p-2 space-y-2" :class="[large ? 'rounded-2xl' : 'rounded-xl']">
        <input
          v-if="searchable"
          type="text"
          v-model="searchKeyword"
          :placeholder="searchPlaceholder"
          class="w-full border border-slate-200 outline-none focus:border-[#0e4391] focus:ring-4 focus:ring-[#0e4391]/10 focus:shadow-[0_0_10px_rgba(14,67,145,0.1)] transition-all duration-300"
          :class="[large ? 'rounded-xl px-4 py-3 text-base md:text-lg font-medium' : 'rounded-lg px-3 py-2.5 md:py-3 text-sm md:text-base']"
        />

        <div class="max-h-44 overflow-auto">
          <button
            v-if="clearable"
            type="button"
            class="w-full text-left rounded-lg transition-all duration-200"
            :class="[
              large ? 'px-4 py-3 text-base md:text-lg font-medium' : 'px-3 py-2.5 md:py-3 text-sm md:text-base',
              !modelValue
                ? 'bg-blue-50/80 text-[#0e4391] font-bold border-l-2 border-[#0e4391] pl-2'
                : 'text-slate-700 hover:bg-slate-50 hover:text-slate-900'
            ]"
            @click="selectValue('')"
          >
            {{ placeholder }}
          </button>

          <button
            v-for="item in filteredItems"
            :key="`${item.value}`"
            type="button"
            class="w-full text-left rounded-lg transition-all duration-200"
            :class="[
              large ? 'px-4 py-3 text-base md:text-lg font-medium' : 'px-3 py-2.5 md:py-3 text-sm md:text-base',
              String(item.value) === String(modelValue)
                ? 'bg-blue-50/80 text-[#0e4391] font-bold border-l-2 border-[#0e4391] pl-2'
                : 'text-slate-700 hover:bg-slate-50 hover:text-slate-900'
            ]"
            @click="selectValue(item.value)"
          >
            {{ item.label }}
          </button>

          <p class="text-slate-400" :class="[large ? 'px-4 py-3 text-base md:text-lg font-medium' : 'px-3 py-2.5 md:py-3 text-sm md:text-base']" v-if="filteredItems.length === 0">No data found.</p>
        </div>
      </div>
    </transition>
  </div>
</template>

<style scoped>
.dropdown-fade-enter-active,
.dropdown-fade-leave-active {
  transition: opacity 0.18s cubic-bezier(0.16, 1, 0.3, 1), transform 0.18s cubic-bezier(0.16, 1, 0.3, 1);
}
.dropdown-fade-enter-from,
.dropdown-fade-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}
</style>
