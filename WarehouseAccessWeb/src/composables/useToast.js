import { ref } from 'vue';
import { useSweetAlert } from './useSweetAlert';

const toasts = ref([]);

export function useToast() {
  const { showSuccess, showError } = useSweetAlert()

  function showToast(message, type = 'success') {
    if (type === 'error' || type === 'warning') {
      showError(message)
      return
    }
    showSuccess(message)
  }

  return {
    toasts,
    showToast
  };
}
