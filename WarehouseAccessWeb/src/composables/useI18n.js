import { ref, computed } from 'vue';
import { T } from '../config/i18n';

const lang = ref(localStorage.getItem('wa_language') || 'vi');

export function useI18n() {
  const t = computed(() => T[lang.value]);

  function setLanguage(newLang) {
    if (T[newLang]) {
      lang.value = newLang;
      localStorage.setItem('wa_language', newLang);
    }
  }

  return {
    lang,
    t,
    setLanguage
  };
}
