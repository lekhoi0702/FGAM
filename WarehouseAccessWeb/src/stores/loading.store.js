import { reactive, readonly } from 'vue'

const state = reactive({
  isLoading: false,
  loadingText: ''
})

export function showLoading(text = '') {
  state.isLoading = true
  state.loadingText = text
}

export function hideLoading() {
  state.isLoading = false
  state.loadingText = ''
}

export function useLoadingState() {
  return readonly(state)
}
