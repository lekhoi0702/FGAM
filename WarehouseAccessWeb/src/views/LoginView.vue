<script setup>
import { computed, nextTick, onMounted, onUnmounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import headerLogo from '../assets/jhv_logo.png'
import bgImage from '../assets/background.jpg'
import { login } from '../stores/auth.store'
import { useSweetAlert } from '../composables/useSweetAlert'
import { useI18n } from '../composables/useI18n'
import { getFactories } from '../services/master-data.service'

const route = useRoute()
const router = useRouter()
const { lang, t, setLanguage } = useI18n()
const { showError } = useSweetAlert()

const loginMode = ref('password')
const cardNumber = ref('')
const userIdInput = ref('')
const password = ref('')
const captchaInput = ref('')
const captchaUppercaseInput = computed({
  get: () => captchaInput.value,
  set: value => { captchaInput.value = value.toUpperCase() }
})
const captchaCode = ref('')
const loading = ref(false)
const cardInputRef = ref(null)
const userIdInputRef = ref(null)
const captchaCanvasRef = ref(null)

const languages = [
  { code: 'vi', name: 'Tiếng Việt', flagClass: 'fi-vn' },
  { code: 'en', name: 'English', flagClass: 'fi-gb' },
  { code: 'zh', name: '中文', flagClass: 'fi-tw' }
]
const currentLanguage = computed(() => languages.find(l => l.code === lang.value) || languages[0])

function generateRandomCaptcha(length = 5) {
  const chars = 'ABCDEFGHJKMNPQRSTUVWXYZ23456789'
  return Array.from({ length }, () => chars[Math.floor(Math.random() * chars.length)]).join('')
}

function drawCaptcha() {
  const canvas = captchaCanvasRef.value
  if (!canvas) return

  const context = canvas.getContext('2d')
  if (!context) return

  const width = 140
  const height = 52
  canvas.width = width
  canvas.height = height

  context.clearRect(0, 0, width, height)
  context.fillStyle = '#f8fafc'
  context.fillRect(0, 0, width, height)

  for (let i = 0; i < 20; i += 1) {
    context.strokeStyle = `rgba(${Math.floor(Math.random() * 120)}, ${Math.floor(Math.random() * 120)}, ${Math.floor(Math.random() * 120)}, 0.25)`
    context.beginPath()
    context.moveTo(Math.random() * width, Math.random() * height)
    context.lineTo(Math.random() * width, Math.random() * height)
    context.stroke()
  }

  const chars = captchaCode.value.split('')
  chars.forEach((char, index) => {
    context.font = `${24 + Math.floor(Math.random() * 8)}px sans-serif`
    context.fillStyle = `rgb(${100 + Math.floor(Math.random() * 120)}, ${50 + Math.floor(Math.random() * 120)}, ${80 + Math.floor(Math.random() * 120)})`
    const x = 16 + index * 24
    const y = 30 + Math.random() * 10
    context.save()
    context.translate(x, y)
    context.rotate((Math.random() - 0.5) * 0.3)
    context.fillText(char, 0, 0)
    context.restore()
  })
}

function refreshCaptcha() {
  captchaInput.value = ''
  captchaCode.value = generateRandomCaptcha()
  nextTick(drawCaptcha)
}

function validateCaptcha() {
  if (!captchaInput.value.trim()) {
    return { valid: false, message: t.value.captchaRequired }
  }
  if (captchaInput.value.trim().toUpperCase() !== captchaCode.value.toUpperCase()) {
    return { valid: false, message: t.value.captchaMismatch }
  }
  return { valid: true }
}

function focusCardInput() {
  nextTick(() => {
    if (loginMode.value === 'card') {
      cardInputRef.value?.focus()
      return
    }
    userIdInputRef.value?.focus()
  })
}

const factories = ref([])
const factoryId = ref('')

onMounted(async () => {
  focusCardInput()
  refreshCaptcha()
  try {
    const res = await getFactories()
    if (res?.success) {
      factories.value = res.data || []
      if (factories.value.length > 0) {
        factoryId.value = factories.value[0].factoryId
      }
    }
  } catch (err) {
    console.error('Failed to load factories', err)
  }
})

const showLangMenu = ref(false)
const showFactoryMenu = ref(false)

const selectedFactoryName = computed(() => {
  const f = factories.value.find(x => x.factoryId === factoryId.value)
  return f ? f.factoryName : 'Chọn xưởng'
})

function toggleLangMenu(e) {
  e.stopPropagation()
  showLangMenu.value = !showLangMenu.value
  if (showLangMenu.value) {
    document.addEventListener('click', closeLangMenu)
  }
}

function closeLangMenu() {
  showLangMenu.value = false
  document.removeEventListener('click', closeLangMenu)
}

function selectLang(code) {
  setLanguage(code)
  closeLangMenu()
}

async function toggleFactoryMenu(e) {
  e.stopPropagation()
  showFactoryMenu.value = !showFactoryMenu.value
  if (showFactoryMenu.value) {
    document.addEventListener('click', closeFactoryMenu)
    try {
      const res = await getFactories()
      if (res?.success) {
        factories.value = res.data || []
      }
    } catch (err) {
      console.error('Failed to reload factories', err)
    }
  }
}

function closeFactoryMenu() {
  showFactoryMenu.value = false
  document.removeEventListener('click', closeFactoryMenu)
}

function selectFactory(id) {
  factoryId.value = id
  closeFactoryMenu()
}

onUnmounted(() => {
  document.removeEventListener('click', closeLangMenu)
  document.removeEventListener('click', closeFactoryMenu)
})

async function submitLogin() {
  if (loading.value) return

  const captchaResult = validateCaptcha()
  if (!captchaResult.valid) {
    await showError(captchaResult.message || t.value.loginError || 'Unable to login')
    refreshCaptcha()
    return
  }

  const payload = loginMode.value === 'card'
    ? { cardNumber: cardNumber.value.trim() }
    : { userId: userIdInput.value.trim(), password: password.value.trim(), factoryId: factoryId.value }

  if ((loginMode.value === 'card' && !payload.cardNumber) || (loginMode.value !== 'card' && (!payload.userId || !payload.password || !payload.factoryId))) {
    return
  }

  loading.value = true
  const response = await login(payload, loginMode.value === 'card' ? 'card' : 'password')
  loading.value = false

  if (!response?.success) {
    await showError(response?.message || t.value.loginError || 'Unable to login')
    cardNumber.value = ''
    userIdInput.value = ''
    password.value = ''
    refreshCaptcha()
    focusCardInput()
    return
  }

  const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/monitor'
  router.replace(redirect)
}
</script>

<template>
  <div class="min-h-screen relative flex items-center justify-center p-4 md:p-6 overflow-hidden font-sans">
    <!-- Full-screen Background Image with dark overlay and blur for high readability -->
    <div class="absolute inset-0 z-0">
      <img :src="bgImage" class="w-full h-full object-cover" alt="Warehouse Background" />
      <div class="absolute inset-0 bg-slate-900/40 backdrop-blur-[2px]"></div>
    </div>

    <!-- Centered Login Card Container -->
    <div class="w-full max-w-[650px] bg-white border border-slate-200/80 rounded-3xl py-8 px-6 md:px-8 shadow-2xl relative z-10 transition-all duration-300 hover:shadow-2xl hover:border-slate-300 space-y-6 animate-fade-in-up delay-1">
      <!-- Language Selector Dropdown (Inside top right of Login Box) -->
      <div class="absolute top-6 right-6 z-20">
        <button 
          @click="toggleLangMenu"
          class="flex items-center gap-2 bg-white hover:bg-slate-50 border border-slate-300 px-3.5 py-2.5 rounded-xl text-sm font-semibold text-slate-700 transition-all duration-200 shadow-sm active:scale-95"
        >
          <span class="fi shrink-0 shadow-sm border border-slate-200/50 rounded-sm" :class="currentLanguage.flagClass"></span>
          <span>{{ currentLanguage.name }}</span>
          <i class="fa-solid fa-chevron-down text-xs text-slate-400 transition-transform duration-200" :class="{ 'rotate-180': showLangMenu }"></i>
        </button>
        
        <transition name="pop-in">
          <div 
            v-if="showLangMenu" 
            class="absolute right-0 mt-2 w-40 bg-white border border-slate-300 rounded-xl shadow-xl py-1.5 z-30 overflow-hidden"
          >
            <button 
              v-for="item in languages" 
              :key="item.code"
              @click.stop="selectLang(item.code)"
              class="w-full text-left px-3.5 py-3 text-sm font-semibold text-slate-700 hover:bg-slate-50 transition-colors duration-150 flex items-center justify-between"
            >
              <div class="flex items-center gap-2">
                <span class="fi shrink-0 shadow-sm border border-slate-200/50 rounded-sm" :class="item.flagClass"></span>
                <span>{{ item.name }}</span>
              </div>
              <i v-if="lang === item.code" class="fa-solid fa-check text-xs text-blue-600 shrink-0"></i>
            </button>
          </div>
        </transition>
      </div>

      <!-- Logo block -->
      <div class="flex items-center gap-3 sm:gap-4 !mt-0 mb-4 pr-36">
        <img class="h-10 w-auto object-contain shrink-0" :src="headerLogo" alt="JIA HSIN" />
        <span class="h-8 w-px bg-slate-200 shrink-0"></span>
        <div class="min-w-0 text-left leading-tight">
          <h1 class="text-lg sm:text-xl font-black whitespace-nowrap tracking-wide text-slate-800 uppercase">{{ t.sysTitle }}</h1>
          <p class="text-xs sm:text-sm text-slate-400 font-semibold mt-0.5 uppercase">{{ t.sysSub }}</p>
        </div>
      </div>

      <!-- Credentials Input form -->
      <div class="space-y-4">
        <transition name="fade-slide" mode="out-in">
          <div :key="loginMode" class="space-y-4">
            <!-- Custom Factory Selection Dropdown -->
            <div v-if="loginMode !== 'card'" class="space-y-2 text-left relative">
              <label class="text-xs font-bold text-slate-400 uppercase tracking-wider">
                Xưởng
              </label>
              <div class="relative flex items-center">
                <button
                  type="button"
                  @click="toggleFactoryMenu"
                  :disabled="loading"
                  class="w-full bg-white border border-slate-300 rounded-xl pl-11 pr-10 py-2.5 text-slate-800 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500/20 transition-all duration-200 font-semibold text-left flex justify-between items-center"
                >
                  <span>{{ selectedFactoryName }}</span>
                </button>
                <div class="absolute left-4 text-slate-400 pointer-events-none w-5 text-center flex items-center justify-center">
                  <i class="fa-solid fa-industry text-base"></i>
                </div>
                <div class="absolute right-4 text-slate-400 pointer-events-none flex items-center justify-center">
                  <i class="fa-solid fa-chevron-down text-xs transition-transform duration-200" :class="{ 'rotate-180': showFactoryMenu }"></i>
                </div>
              </div>

              <!-- Floating Custom Dropdown List -->
              <transition name="pop-in">
                <div 
                  v-if="showFactoryMenu" 
                  class="absolute left-0 right-0 mt-2 bg-white border border-slate-300 rounded-xl shadow-xl py-1.5 z-30 overflow-hidden"
                >
                  <button 
                    v-for="f in factories" 
                    :key="f.factoryId"
                    type="button"
                    @click.stop="selectFactory(f.factoryId)"
                    class="w-full text-left px-3.5 py-3.5 text-sm font-semibold text-slate-700 hover:bg-slate-50 transition-colors duration-150 flex items-center justify-between"
                  >
                    <span>{{ f.factoryName }}</span>
                    <i v-if="factoryId === f.factoryId" class="fa-solid fa-check text-xs text-blue-600 shrink-0"></i>
                  </button>
                </div>
              </transition>
            </div>

            <div class="space-y-2 text-left">
              <label class="text-xs font-bold text-slate-400 uppercase tracking-wider">
                {{ loginMode === 'card' ? t.loginCardNumber : t.loginUserId }}
              </label>
              <div class="relative flex items-center">
                <input
                  ref="cardInputRef"
                  v-model="cardNumber"
                  v-if="loginMode === 'card'"
                  :disabled="loading"
                  @keydown.enter.prevent="submitLogin"
                  class="w-full bg-white border border-slate-300 rounded-xl pl-11 pr-4 py-2.5 text-slate-800 placeholder-slate-400 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500/20 transition-all duration-200"
                  :placeholder="t.placeholderScanCard"
                />
                <input
                  ref="userIdInputRef"
                  v-model="userIdInput"
                  v-else
                  :disabled="loading"
                  @keydown.enter.prevent="submitLogin"
                  class="w-full bg-white border border-slate-300 rounded-xl pl-11 pr-4 py-2.5 text-slate-800 placeholder-slate-400 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500/20 transition-all duration-200"
                  :placeholder="t.placeholderEnterUserId"
                />
                <div class="absolute left-4 text-slate-400 w-5 text-center flex items-center justify-center">
                  <i class="fa-solid fa-user text-base"></i>
                </div>
              </div>
            </div>
            <div v-if="loginMode !== 'card'" class="space-y-2 text-left">
              <label class="text-xs font-bold text-slate-400 uppercase tracking-wider">{{ t.loginPassword }}</label>
              <input
                v-model="password"
                :disabled="loading"
                type="password"
                @keydown.enter.prevent="submitLogin"
                class="w-full bg-white border border-slate-300 rounded-xl px-4 py-2.5 text-slate-800 placeholder-slate-400 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500/20 transition-all duration-200"
                :placeholder="t.placeholderEnterPassword"
              />
            </div>

            <!-- Captcha Block -->
            <div class="space-y-2 text-left">
              <label class="text-xs font-bold text-slate-400 uppercase tracking-wider">{{ t.captchaLabel }}</label>
              <div class="flex flex-col sm:flex-row gap-3 items-center">
                <div class="rounded-2xl border border-slate-300 bg-slate-50 p-2 flex items-center justify-center">
                  <canvas ref="captchaCanvasRef" class="h-12 w-36" aria-label="Captcha image"></canvas>
                </div>
                <button
                  type="button"
                  @click="refreshCaptcha"
                  class="shrink-0 px-3 py-2 bg-slate-100 text-slate-700 rounded-xl text-xs font-semibold hover:bg-slate-200 transition-all duration-150"
                >
                  {{ t.refreshCaptcha }}
                </button>
              </div>
              <input
                v-model="captchaUppercaseInput"
                :disabled="loading"
                @keydown.enter.prevent="submitLogin"
                class="w-full bg-white border border-slate-300 rounded-xl px-4 py-2.5 text-slate-800 placeholder-slate-400 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500/20 transition-all duration-200"
                :placeholder="t.placeholderEnterCaptcha"
              />
            </div>
          </div>
        </transition>

        <button
          v-if="loginMode !== 'card'"
          @click="submitLogin"
          :disabled="loading"
          class="w-full py-2.5 rounded-xl bg-blue-600 hover:bg-blue-500 text-white font-semibold shadow-lg shadow-blue-500/10 active:scale-[0.98] transition-all duration-200 disabled:opacity-50 disabled:pointer-events-none flex items-center justify-center gap-2"
        >
          <span v-if="loading" class="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin"></span>
          <span>{{ loading ? t.btnAuthenticating : t.btnSignIn }}</span>
        </button>
      </div>

      <!-- Footer -->
      <p class="text-xs text-slate-400 text-center">
        &copy; 2026 JIA HSIN Co., Ltd. All rights reserved.
      </p>
    </div>
  </div>
</template>

<style scoped>
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: all 0.25s ease-in-out;
}
.fade-slide-enter-from {
  opacity: 0;
  transform: translateX(12px);
}
.fade-slide-leave-to {
  opacity: 0;
  transform: translateX(-12px);
}

.pop-in-enter-active,
.pop-in-leave-active {
  transition: all 0.2s cubic-bezier(0.16, 1, 0.3, 1);
}
.pop-in-enter-from,
.pop-in-leave-to {
  opacity: 0;
  transform: scale(0.95) translateY(-4px);
}
</style>
