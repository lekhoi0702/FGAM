<script setup>
const props = defineProps({
  page: { type: Number, required: true },
  total: { type: Number, required: true },
  totalPages: { type: Number, required: true }
})

const emit = defineEmits(['update:page'])
</script>

<template>
  <div class="d-flex align-center justify-space-between mt-4">
    <p class="text-body-2 text-faded mb-0">Total: <strong>{{ props.total }}</strong> records</p>
    <v-pagination
      :model-value="props.page"
      :length="Math.max(props.totalPages, 1)"
      rounded="circle"
      density="comfortable"
      @update:model-value="emit('update:page', $event)"
    />
  </div>
</template>
