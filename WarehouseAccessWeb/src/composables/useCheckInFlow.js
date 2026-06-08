import { ref, reactive, computed, nextTick, onMounted, onBeforeUnmount, watch } from 'vue'
import { FilesetResolver, FaceDetector } from '@mediapipe/tasks-vision'
import { useCamera } from './useCamera'
import { useAudio } from './useAudio'
import { useToast } from './useToast'
import { useRecords } from './useRecords'
import { useAuthState } from '../stores/auth.store'
import { getUsers } from '../services/employee-management.service'

export function useCheckInFlow(options = {}) {
  const { onSuccess, notify } = options
  const authState = useAuthState()
  const { playBeep } = useAudio()
  const { showToast } = useToast()
  const { lookupCard, submitCheckIn, submitCheckOut, purposeItems, userTypeItems, loadPurposesCrud, loadUserTypesCrud } = useRecords()

  const {
    stream,
    ready: camReady,
    cameraError,
    cameraErrorMessage,
    facing,
    switching,
    hasMultipleCams,
    showCam,
    openCamera,
    closeCamera,
    flipCamera,
    capturePhoto
  } = useCamera()

  const notifyUser = (message, type = 'info') => {
    if (typeof notify === 'function') {
      notify(message, type)
      return
    }
    showToast(message, type)
  }

  const step = ref(1)
  const checkInCardNumber = ref('')
  const cardInputRef = ref(null)
  const lookupLoading = ref(false)
  const lookupMessage = ref('')
  const hasCardLookupResult = ref(false)
  const submitLoading = ref(false)
  const fieldsLockedByGuestData = ref(false)
  const lookupNote = ref('')
  const showManualForm = ref(false)
  const manualStep = ref(1)
  const step2Skipped = ref(false)
  const checkingUser = ref(false)

  const formState = reactive({
    cardNumber: '',
    userId: '',
    userTypeId: '',
    userTypeName: '',
    fullName: '',
    contactPerson: '',
    companyName: '',
    departmentCode: '',
    departmentName: '',
    purposeName: '',
    photo: ''
  })

  const errors = reactive({
    userId: '',
    fullName: '',
    userTypeId: '',
    companyName: '',
    purposeName: ''
  })

  const videoRef = ref(null)
  const canvasRef = ref(null)
  const autoCaptureSupported = ref(true)
  const autoCaptureActive = ref(false)
  const autoCaptureStatus = ref('idle')

  let mediaPipeFaceDetector = null
  let mediaPipeInitializing = false
  let autoCaptureTimer = null
  let stableFrameCount = 0
  let openCameraRetryTimer = null

  onMounted(() => {
    loadPurposesCrud()
    loadUserTypesCrud()
    nextTick(() => cardInputRef.value?.focus())
    formState.contactPerson = authState.currentUser?.fullName || ''
  })

  onBeforeUnmount(() => {
    stopAutoCaptureLoop()
    if (openCameraRetryTimer) {
      clearTimeout(openCameraRetryTimer)
      openCameraRetryTimer = null
    }
    if (mediaPipeFaceDetector?.close) mediaPipeFaceDetector.close()
    mediaPipeFaceDetector = null
  })

  watch(step, (newStep) => {
    if (newStep === 2 && !formState.photo && !showCam.value) {
      nextTick(() => triggerCamera())
    }
    if (newStep !== 2) stopAutoCaptureLoop()
  })

  watch(showCam, (isVisible) => {
    if (isVisible && step.value === 2 && !formState.photo) {
      startAutoCaptureLoop()
      return
    }
    stopAutoCaptureLoop()
  })

  watch(() => formState.photo, (photoValue) => {
    if (photoValue) stopAutoCaptureLoop()
  })

  const scannerState = computed(() => {
    if (lookupLoading.value) return 'detecting'
    if (hasCardLookupResult.value) return 'found'
    return 'waiting'
  })

  async function handleCardLookup() {
    const cardNum = checkInCardNumber.value.trim()
    if (!cardNum) {
      notifyUser('Vui lòng nhập số thẻ', 'warning')
      return
    }

    lookupLoading.value = true
    lookupMessage.value = ''
    hasCardLookupResult.value = false
    fieldsLockedByGuestData.value = false
    lookupNote.value = ''

    try {
      if (!purposeItems.value || purposeItems.value.length === 0) {
        await loadPurposesCrud()
      }
      if (!userTypeItems.value || userTypeItems.value.length === 0) {
        await loadUserTypesCrud()
      }

      const res = await lookupCard(cardNum)
      if (res?.success && res.data) {
        const isInside = res.data.isInside ?? false
        if (isInside) {
          const openTransactionId = res.data.openTransactionId
          if (!openTransactionId) {
            lookupMessage.value = 'Không tìm thấy thông tin lượt vào để ra cho thẻ này.'
            notifyUser(lookupMessage.value, 'error')
            checkInCardNumber.value = ''
            nextTick(() => cardInputRef.value?.focus())
            return
          }

          const checkoutResponse = await submitCheckOut(openTransactionId)
          if (checkoutResponse?.success) {
            playBeep(true)
            notifyUser('Xác nhận ra kho thành công.', 'success')
          } else {
            playBeep(false)
            notifyUser(checkoutResponse?.message || 'Xác nhận ra kho thất bại.', 'error')
          }

          checkInCardNumber.value = ''
          nextTick(() => cardInputRef.value?.focus())
          return
        }

        playBeep(true)
        hasCardLookupResult.value = true
        formState.cardNumber = res.data.cardNumber || cardNum
        formState.userId = res.data.userId || ''
        formState.userTypeId = res.data.userTypeId || ''
        formState.userTypeName = res.data.userTypeName || ''
        formState.fullName = res.data.fullName || ''
        formState.contactPerson = res.data.contactPerson || authState.currentUser?.fullName || ''
        formState.companyName = res.data.companyName || ''
        formState.departmentCode = res.data.departmentCode || ''
        formState.departmentName = res.data.departmentName || ''
        formState.purposeName = res.data.purposeName || 'Làm việc'
        lookupNote.value = res.data.lookupNote || ''
        fieldsLockedByGuestData.value = !!res.data.isExternalGuestDataApplied
        lookupMessage.value = ''



        if (step.value === 1) {
          const isValid = validateStepOne()
          if (isValid) {
            step.value = 2
          } else {
            if (!formState.fullName || !formState.fullName.trim()) {
              showManualForm.value = true
              manualStep.value = 1
              notifyUser('Vui lòng hoàn tất đăng ký thủ công: Thiếu họ và tên', 'warning')
            } else if (!formState.companyName || !formState.companyName.trim() || !formState.userTypeId) {
              showManualForm.value = true
              manualStep.value = 2
              notifyUser('Vui lòng hoàn tất đăng ký thủ công: Thiếu công ty hoặc loại đối tượng', 'warning')
            } else if (!formState.purposeName || !formState.purposeName.trim()) {
              showManualForm.value = true
              manualStep.value = 3
            } else {
              showManualForm.value = false
            }
          }
        }
      } else {
        playBeep(false)
        lookupMessage.value = res?.message || 'Không tìm thấy thông tin thẻ'
        notifyUser(lookupMessage.value, 'error')
        checkInCardNumber.value = ''
        nextTick(() => cardInputRef.value?.focus())
      }
    } catch {
      playBeep(false)
      lookupMessage.value = 'Lỗi kết nối hệ thống.'
      notifyUser(lookupMessage.value, 'error')
      checkInCardNumber.value = ''
      nextTick(() => cardInputRef.value?.focus())
    } finally {
      lookupLoading.value = false
    }
  }

  function validateStepOne() {
    if (!hasCardLookupResult.value) {
      lookupMessage.value = 'Vui lòng quét/kiểm tra thẻ trước.'
      return false
    }
    if (!formState.purposeName || !formState.purposeName.trim()) {
      formState.purposeName = 'Làm việc'
    }
    errors.userId = ''
    errors.fullName = formState.fullName.trim() ? '' : 'Yêu cầu điền họ và tên'
    errors.userTypeId = formState.userTypeId ? '' : 'Yêu cầu chọn loại đối tượng'
    errors.companyName = formState.companyName && formState.companyName.trim() ? '' : 'Yêu cầu điền tên công ty'
    errors.purposeName = formState.purposeName && formState.purposeName.trim() ? '' : 'Yêu cầu chọn mục đích'
    return !errors.userId && !errors.fullName && !errors.userTypeId && !errors.companyName && !errors.purposeName
  }

  function nextStep() {
    if (validateStepOne()) {
      step.value = 2
    } else {
      if (!formState.purposeName || !formState.purposeName.trim()) {
        notifyUser('Vui lòng chọn mục đích vào', 'warning')
      }
    }
  }

  function prevStep() {
    if (showCam.value) closeCamera()
    step.value = 1
  }

  function resetCheckInFlow() {
    stopAutoCaptureLoop()
    if (showCam.value) closeCamera()

    step.value = 1
    checkInCardNumber.value = ''
    lookupLoading.value = false
    lookupMessage.value = ''
    hasCardLookupResult.value = false
    submitLoading.value = false
    fieldsLockedByGuestData.value = false
    lookupNote.value = ''
    showManualForm.value = false
    manualStep.value = 1

    formState.cardNumber = ''
    formState.userId = ''
    formState.userTypeId = ''
    formState.userTypeName = ''
    formState.fullName = ''
    formState.contactPerson = authState.currentUser?.fullName || ''
    formState.companyName = ''
    formState.departmentCode = ''
    formState.departmentName = ''
    formState.purposeName = ''
    formState.photo = ''

    errors.userId = ''
    errors.fullName = ''
    errors.userTypeId = ''
    errors.companyName = ''
    errors.purposeName = ''

    nextTick(() => cardInputRef.value?.focus())
  }

  async function handleOpenManualForm() {
    resetCheckInFlow()
    if (checkInCardNumber.value) {
      formState.userId = checkInCardNumber.value.trim()
      formState.cardNumber = ''
    }
    showManualForm.value = true
    manualStep.value = 1

    try {
      if (!purposeItems.value || purposeItems.value.length === 0) {
        await loadPurposesCrud()
      }
      if (!userTypeItems.value || userTypeItems.value.length === 0) {
        await loadUserTypesCrud()
      }
    } catch (e) {
      console.error('Failed to load lookup data on manual registration', e)
    }
  }

  function handleCancelManualForm() {
    resetCheckInFlow()
    showManualForm.value = false
    manualStep.value = 1
  }

  async function handleManualNext() {
    if (manualStep.value === 1) {
      const enteredUserId = (formState.cardNumber || '').trim().toLowerCase()
      if (enteredUserId) {
        checkingUser.value = true
        try {
          const response = await getUsers()
          if (response?.success && Array.isArray(response.data)) {
            const matchedUser = response.data.find(user =>
              (user.userId && user.userId.trim().toLowerCase() === enteredUserId) ||
              (user.cardNumber && user.cardNumber.trim().toLowerCase() === enteredUserId)
            )
            if (matchedUser) {
              formState.fullName = matchedUser.fullName || ''
              formState.companyName = matchedUser.companyName || ''
              formState.userTypeId = matchedUser.userTypeId || ''
              step2Skipped.value = true
              manualStep.value = 3
              return
            }
          }
        } catch {
          // keep default fallback flow
        } finally {
          checkingUser.value = false
        }
      }
      formState.fullName = ''
      formState.companyName = ''
      formState.userTypeId = ''
      step2Skipped.value = false
      manualStep.value = 2
      return
    }
    if (manualStep.value === 2) {
      if (!formState.fullName || !formState.fullName.trim()) {
        notifyUser('Vui lòng nhập Họ và Tên', 'error')
        return
      }
      manualStep.value = 3
    }
  }

  function handleManualBack() {
    if (hasCardLookupResult.value) {
      handleCancelManualForm()
      return
    }
    if (manualStep.value === 3 && step2Skipped.value) {
      manualStep.value = 1
      return
    }
    if (manualStep.value > 1) {
      manualStep.value--
    } else {
      handleCancelManualForm()
    }
  }

  function handleManualFormSubmit() {
    if (!formState.fullName || !formState.fullName.trim()) {
      notifyUser('Vui lòng nhập Họ và Tên', 'error')
      return
    }
    if (!formState.companyName || !formState.companyName.trim()) {
      notifyUser('Vui lòng nhập tên Công ty', 'error')
      return
    }
    if (!formState.userTypeId) {
      notifyUser('Vui lòng chọn Loại đối tượng', 'error')
      return
    }
    hasCardLookupResult.value = true
    fieldsLockedByGuestData.value = false

    showManualForm.value = false
    manualStep.value = 1
    nextStep()
  }

  function selectUserType(userTypeId) {
    formState.userTypeId = userTypeId
    if (formState.fullName && formState.fullName.trim()) {
      handleManualNext()
    }
  }

  function selectPurpose(purposeName) {
    formState.purposeName = purposeName
    handleManualFormSubmit()
  }

  function triggerCamera() {
    if (showCam.value && stream.value) return
    showCam.value = true

    const maxAttempts = 12
    const retryDelayMs = 120
    let attempts = 0

    const tryOpen = () => {
      attempts += 1
      if (videoRef.value) {
        openCamera(videoRef.value)
        openCameraRetryTimer = null
        return
      }

      if (attempts >= maxAttempts) {
        openCameraRetryTimer = null
        notifyUser('Không thể khởi động camera.', 'error')
        return
      }

      openCameraRetryTimer = window.setTimeout(tryOpen, retryDelayMs)
    }

    nextTick(() => {
      if (openCameraRetryTimer) {
        clearTimeout(openCameraRetryTimer)
        openCameraRetryTimer = null
      }
      tryOpen()
    })
  }

  async function triggerCapture() {
    const photoBase64Url = capturePhoto(videoRef.value, canvasRef.value)
    if (!photoBase64Url) {
      notifyUser('Chụp ảnh thất bại. Vui lòng thử lại.', 'error')
      return
    }

    playBeep(true)
    const marker = 'base64,'
    const index = photoBase64Url.indexOf(marker)
    formState.photo = index >= 0 ? photoBase64Url.substring(index + marker.length) : photoBase64Url
    await handleCheckInSubmit()
  }

  async function ensureFaceDetector() {
    if (!autoCaptureSupported.value || mediaPipeFaceDetector || mediaPipeInitializing) return

    mediaPipeInitializing = true
    try {
      const vision = await FilesetResolver.forVisionTasks('https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@latest/wasm')
      mediaPipeFaceDetector = await FaceDetector.createFromOptions(vision, {
        baseOptions: {
          modelAssetPath:
            'https://storage.googleapis.com/mediapipe-models/face_detector/blaze_face_short_range/float16/latest/blaze_face_short_range.tflite'
        },
        runningMode: 'VIDEO',
        minDetectionConfidence: 0.6
      })
    } catch {
      autoCaptureSupported.value = false
      autoCaptureStatus.value = 'unsupported'
    } finally {
      mediaPipeInitializing = false
    }
  }

  function resolveFaceBox(face) {
    const box = face?.boundingBox || face
    if (!box) return null

    const x = Number.isFinite(box.x) ? box.x : box.originX
    const y = Number.isFinite(box.y) ? box.y : box.originY
    const width = box.width
    const height = box.height
    if (![x, y, width, height].every(Number.isFinite)) return null

    return { x, y, width, height }
  }

  function isFaceStable(face, videoElement) {
    const box = resolveFaceBox(face)
    if (!box || !videoElement?.videoWidth || !videoElement?.videoHeight) return false

    const frameWidth = videoElement.videoWidth
    const frameHeight = videoElement.videoHeight
    const faceAreaRatio = (box.width * box.height) / (frameWidth * frameHeight)
    if (faceAreaRatio < 0.04 || faceAreaRatio > 0.72) return false

    const faceCenterX = box.x + box.width / 2
    const faceCenterY = box.y + box.height / 2
    const offsetX = Math.abs(faceCenterX - frameWidth / 2) / frameWidth
    const offsetY = Math.abs(faceCenterY - frameHeight / 2) / frameHeight
    return offsetX < 0.28 && offsetY < 0.28
  }

  async function detectAndMaybeCapture() {
    if (!autoCaptureSupported.value || !showCam.value || formState.photo || step.value !== 2) return

    const videoElement = videoRef.value
    if (!videoElement || videoElement.readyState < 2) {
      autoCaptureStatus.value = 'warming'
      stableFrameCount = 0
      return
    }

    try {
      await ensureFaceDetector()
      if (!mediaPipeFaceDetector) {
        autoCaptureStatus.value = 'unsupported'
        return
      }

      const detectionResult = mediaPipeFaceDetector.detectForVideo(videoElement, performance.now())
      const faces = detectionResult?.detections || []
      if (!faces.length) {
        stableFrameCount = 0
        autoCaptureStatus.value = 'no-face'
        return
      }
      if (faces.length > 1) {
        stableFrameCount = 0
        autoCaptureStatus.value = 'multi-face'
        return
      }
      if (!isFaceStable(faces[0], videoElement)) {
        stableFrameCount = 0
        autoCaptureStatus.value = 'face-adjust'
        return
      }

      stableFrameCount += 1
      autoCaptureStatus.value = 'stabilizing'
      if (stableFrameCount >= 4) {
        autoCaptureStatus.value = 'capturing'
        triggerCapture()
      }
    } catch {
      stableFrameCount = 0
      autoCaptureStatus.value = 'unsupported'
      stopAutoCaptureLoop()
    }
  }

  function startAutoCaptureLoop() {
    if (!autoCaptureSupported.value || autoCaptureTimer) return
    stableFrameCount = 0
    autoCaptureActive.value = true
    autoCaptureStatus.value = 'warming'
    autoCaptureTimer = window.setInterval(() => {
      detectAndMaybeCapture()
    }, 300)
  }

  function stopAutoCaptureLoop() {
    autoCaptureActive.value = false
    stableFrameCount = 0
    if (autoCaptureTimer) {
      clearInterval(autoCaptureTimer)
      autoCaptureTimer = null
    }
    if (autoCaptureStatus.value !== 'unsupported') autoCaptureStatus.value = 'idle'
  }

  async function handleCheckInSubmit() {
    if (!validateStepOne()) {
      step.value = 1
      return false
    }

    submitLoading.value = true
    try {
      const payload = {
        cardNumber: formState.cardNumber ? formState.cardNumber.trim() : null,
        userId: formState.userId ? formState.userId.trim() : null,
        userTypeId: formState.userTypeId ? String(formState.userTypeId).trim() : null,
        fullName: formState.fullName.trim(),
        departmentCode: formState.departmentCode ? formState.departmentCode.trim() : null,
        companyName: formState.companyName ? formState.companyName.trim() : null,
        purposeName: formState.purposeName ? formState.purposeName.trim() : null,
        contactPerson: formState.contactPerson ? formState.contactPerson.trim() : null,
        photo: formState.photo || null
      }

      const res = await submitCheckIn(payload)
      if (res?.success) {
        playBeep(true)
        await onSuccess?.(res)
        return true
      }

      playBeep(false)
      notifyUser(res?.message || 'Gửi thông tin đăng ký thất bại.', 'error')
      return false
    } catch {
      playBeep(false)
      notifyUser('Lỗi hệ thống khi gửi thông tin đăng ký.', 'error')
      return false
    } finally {
      submitLoading.value = false
    }
  }

  return {
    stream,
    camReady,
    cameraError,
    cameraErrorMessage,
    facing,
    switching,
    hasMultipleCams,
    showCam,
    closeCamera,
    flipCamera,
    step,
    checkInCardNumber,
    cardInputRef,
    lookupLoading,
    lookupMessage,
    hasCardLookupResult,
    submitLoading,
    fieldsLockedByGuestData,
    lookupNote,
    formState,
    errors,
    videoRef,
    canvasRef,
    scannerState,
    autoCaptureSupported,
    autoCaptureActive,
    autoCaptureStatus,
    purposeItems,
    userTypeItems,
    step2Skipped,
    checkingUser,
    showManualForm,
    manualStep,
    handleCardLookup,
    nextStep,
    prevStep,
    resetCheckInFlow,
    triggerCamera,
    triggerCapture,
    handleCheckInSubmit,
    handleOpenManualForm,
    handleCancelManualForm,
    handleManualNext,
    handleManualBack,
    handleManualFormSubmit,
    selectUserType,
    selectPurpose
  }
}
