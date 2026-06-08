<script setup>
import { useToast } from '../../composables/useToast';

const { toasts } = useToast();
</script>

<template>
  <div class="fixed top-6 right-6 z-[9999] flex flex-col gap-3 pointer-events-none w-full max-w-sm">
    <transition-group
      name="toast"
      enter-active-class="transition duration-300 ease-out transform"
      enter-from-class="translate-y-2 opacity-0 scale-95"
      enter-to-class="translate-y-0 opacity-100 scale-100"
      leave-active-class="transition duration-200 ease-in transform"
      leave-from-class="opacity-100 scale-100"
      leave-to-class="opacity-0 scale-95"
    >
      <div
        v-for="toast in toasts"
        :key="toast.id"
        class="pointer-events-auto flex items-center justify-between p-4 rounded-xl border shadow-xl glassmorphism"
        :class="[
          toast.type === 'error' 
            ? 'border-red-200/50 bg-red-50/90 text-red-800' 
            : toast.type === 'warning'
            ? 'border-yellow-200/50 bg-yellow-50/90 text-yellow-800'
            : 'border-emerald-200/50 bg-emerald-50/90 text-emerald-800'
        ]"
      >
        <div class="flex items-center gap-3">
          <span class="text-lg">
            <span v-if="toast.type === 'error'">✗</span>
            <span v-else-if="toast.type === 'warning'">⚠️</span>
            <span v-else>✓</span>
          </span>
          <p class="text-sm font-semibold leading-5">{{ toast.message }}</p>
        </div>
      </div>
    </transition-group>
  </div>
</template>
