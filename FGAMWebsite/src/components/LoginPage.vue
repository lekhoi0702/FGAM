<script setup>
import { computed, nextTick, onBeforeUnmount, onMounted, ref } from 'vue'
import bgImage from '../assets/background.jpg'
import headerLogo from '../assets/jhv_logo.png'

const languages = [
  { code: 'vi', name: 'Tiếng Việt', flagClass: 'fi-vn' },
  { code: 'en', name: 'English', flagClass: 'fi-gb' },
  { code: 'zh', name: '中文', flagClass: 'fi-tw' }
]

const T = {
  vi: {
    sysTitle: "HỆ THỐNG CỔNG FGAM",
    sysSub: "QUẢN LÝ RA VÀO CỔNG NHÀ MÁY",
    loginUserId: "Mã nhân viên",
    loginPassword: "Mật khẩu",
    placeholderEnterUserId: "Nhập mã nhân viên",
    placeholderEnterPassword: "Nhập mật khẩu",
    btnSignIn: "Đăng nhập",
    btnAuthenticating: "Đang xác thực...",
    captchaLabel: "Mã xác nhận",
    placeholderEnterCaptcha: "Nhập mã xác nhận",
    refreshCaptcha: "Làm mới",
    captchaRequired: "Vui lòng nhập mã xác nhận",
    captchaMismatch: "Mã xác nhận không đúng",
    loginError: "Không thể đăng nhập",
    factoryLabel: "Xưởng",
    placeholderSelectFactory: "Chọn xưởng"
  },
  en: {
    sysTitle: "FGAM ACCESS SYSTEM",
    sysSub: "FACTORY GATE ACCESS MANAGEMENT",
    loginUserId: "Employee ID",
    loginPassword: "Password",
    placeholderEnterUserId: "Enter Employee ID",
    placeholderEnterPassword: "Enter password",
    btnSignIn: "Sign In",
    btnAuthenticating: "Authenticating...",
    captchaLabel: "Captcha",
    placeholderEnterCaptcha: "Enter captcha",
    refreshCaptcha: "Refresh",
    captchaRequired: "Please enter captcha",
    captchaMismatch: "Captcha does not match",
    loginError: "Unable to login",
    factoryLabel: "Factory",
    placeholderSelectFactory: "Select factory"
  },
  zh: {
    sysTitle: "FGAM 門禁系統",
    sysSub: "工廠閘門進出管理",
    loginUserId: "員工工號",
    loginPassword: "密碼",
    placeholderEnterUserId: "輸入工號",
    placeholderEnterPassword: "輸入密碼",
    btnSignIn: "登入",
    btnAuthenticating: "驗證中...",
    captchaLabel: "驗證碼",
    placeholderEnterCaptcha: "輸入驗證碼",
    refreshCaptcha: "重新整理",
    captchaRequired: "請輸入驗證碼",
    captchaMismatch: "驗證碼錯誤",
    loginError: "登入失敗",
    factoryLabel: "工廠",
    placeholderSelectFactory: "選擇工廠"
  }
}

const factories = ref([
  { factoryId: 'F001', factoryName: 'JIA HSIN - Factory A' },
  { factoryId: 'F002', factoryName: 'JIA HSIN - Factory B' },
  { factoryId: 'F003', factoryName: 'JIA HSIN - Factory C' }
])

const language = ref(localStorage.getItem('wa_language') || 'vi')
const showLangMenu = ref(false)
const showFactoryMenu = ref(false)
const factoryId = ref(factories.value[0].factoryId)
const employeeId = ref('')
const password = ref('')
const captchaInput = ref('')
const captchaCode = ref('')
const loading = ref(false)

const userIdInputRef = ref(null)
const captchaCanvasRef = ref(null)

const currentLanguage = computed(() => languages.find(item => item.code === language.value) || languages[0])
const selectedFactory = computed(() => factories.value.find(item => item.factoryId === factoryId.value) || factories.value[0])
const t = computed(() => T[language.value])

const captchaUppercaseInput = computed({
  get: () => captchaInput.value,
  set: value => { captchaInput.value = value.toUpperCase() }
})

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

  captchaCode.value.split('').forEach((char, index) => {
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

function focusUserIdInput() {
  nextTick(() => {
    userIdInputRef.value?.focus?.()
  })
}

function closeLangMenu() {
  showLangMenu.value = false
  document.removeEventListener('click', closeLangMenu)
}

function closeFactoryMenu() {
  showFactoryMenu.value = false
  document.removeEventListener('click', closeFactoryMenu)
}

function toggleLangMenu(e) {
  e.stopPropagation()
  showLangMenu.value = !showLangMenu.value
  if (showLangMenu.value) {
    document.addEventListener('click', closeLangMenu)
  }
}

function selectLang(code) {
  language.value = code
  localStorage.setItem('wa_language', code)
  closeLangMenu()
  nextTick(drawCaptcha)
}

function toggleFactoryMenu(e) {
  e.stopPropagation()
  showFactoryMenu.value = !showFactoryMenu.value
  if (showFactoryMenu.value) {
    document.addEventListener('click', closeFactoryMenu)
  }
}

function selectFactory(id) {
  factoryId.value = id
  closeFactoryMenu()
}

async function submitLogin() {
  if (loading.value) return

  const captchaResult = validateCaptcha()
  if (!captchaResult.valid) {
    alert(captchaResult.message)
    refreshCaptcha()
    return
  }

  if (!employeeId.value.trim() || !password.value.trim() || !factoryId.value) {
    return
  }

  loading.value = true
  await new Promise(resolve => setTimeout(resolve, 600))
  loading.value = false
  alert('Success Login: ' + employeeId.value)
}

onMounted(() => {
  refreshCaptcha()
  focusUserIdInput()
})

onBeforeUnmount(() => {
  document.removeEventListener('click', closeLangMenu)
  document.removeEventListener('click', closeFactoryMenu)
})
</script>

<template>
  <div class="min-h-screen relative flex items-center justify-center p-4 md:p-6 overflow-hidden font-sans">
    <!-- Full-screen Background Image with dark overlay for high readability -->
    <div class="absolute inset-0 z-0">
      <img :src="bgImage" class="w-full h-full object-cover" alt="Warehouse Background" />
      <div class="absolute inset-0 bg-slate-900/50"></div>
    </div>

    <!-- Centered Login Card Container -->
    <div class="w-full max-w-[520px] bg-white border border-slate-200/80 rounded-3xl py-8 px-6 md:px-8 shadow-2xl relative z-10 transition-all duration-300 hover:shadow-2xl hover:border-slate-300 space-y-6 animate-fade-in-up delay-1">
      <!-- Language Selector Dropdown (Inside top right of Login Box) -->
      <div class="absolute top-6 right-6 z-20">
        <button 
          @click="toggleLangMenu"
          class="flex items-center justify-center bg-white hover:bg-slate-50 border border-slate-300 w-10 h-10 rounded-xl transition-all duration-200 shadow-sm active:scale-95"
        >
          <span class="fi shrink-0 shadow-sm border border-slate-200/50 rounded-sm" :class="currentLanguage.flagClass"></span>
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
              <i v-if="language === item.code" class="fa-solid fa-check text-xs text-blue-600 shrink-0"></i>
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
        <!-- Custom Factory Selection Dropdown -->
        <div class="space-y-2 text-left relative">
          <label class="text-xs font-bold text-slate-400 uppercase tracking-wider">
            {{ t.factoryLabel }}
          </label>
          <div class="relative flex items-center">
            <button
              type="button"
              @click="toggleFactoryMenu"
              :disabled="loading"
              class="w-full bg-white border border-slate-300 rounded-xl pl-11 pr-10 py-2.5 text-slate-800 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500/20 transition-all duration-200 font-semibold text-left flex justify-between items-center"
            >
              <span>{{ selectedFactory.factoryName }}</span>
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
            {{ t.loginUserId }}
          </label>
          <div class="relative flex items-center">
            <input
              ref="userIdInputRef"
              v-model="employeeId"
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

        <div class="space-y-2 text-left">
          <label class="text-xs font-bold text-slate-400 uppercase tracking-wider">{{ t.loginPassword }}</label>
          <div class="relative flex items-center">
            <input
              v-model="password"
              :disabled="loading"
              type="password"
              @keydown.enter.prevent="submitLogin"
              class="w-full bg-white border border-slate-300 rounded-xl pl-11 pr-4 py-2.5 text-slate-800 placeholder-slate-400 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500/20 transition-all duration-200"
              :placeholder="t.placeholderEnterPassword"
            />
            <div class="absolute left-4 text-slate-400 w-5 text-center flex items-center justify-center">
              <i class="fa-solid fa-lock text-base"></i>
            </div>
          </div>
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

        <button
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
.pop-in-enter-active,
.pop-in-leave-active {
  transition: transform 0.25s cubic-bezier(0.34, 1.56, 0.64, 1), opacity 0.2s ease-out;
  will-change: transform, opacity;
}
.pop-in-enter-from,
.pop-in-leave-to {
  opacity: 0;
  transform: scale(0.96) translateY(-6px) translateZ(0);
}
</style>
