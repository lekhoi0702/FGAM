import { ref, nextTick } from 'vue';

const CAMERA_ERROR_MESSAGES = {
  unsupported: "Camera is not supported in this browser or WebView.",
  insecure: "Camera requires a trusted HTTPS origin on mobile browsers. Run npm run cert:dev, npm run dev:https, then open the HTTPS LAN URL.",
  permission_denied: "Camera permission was denied. Allow camera access in browser or app settings, then try again.",
  not_found: "No camera device was found on this device.",
  in_use: "Camera is already in use by another app or browser tab.",
  constraints: "The requested camera is unavailable. Trying another camera may help.",
  unknown: "Camera access failed. Check browser permissions, HTTPS, or device camera settings."
};

export function useCamera() {
  const stream = ref(null);
  const ready = ref(false);
  const cameraError = ref("");
  const cameraErrorMessage = ref("");
  const facing = ref("user"); // "user" | "environment"
  const switching = ref(false);
  const hasMultipleCams = ref(false);
  const showCam = ref(false);

  function setCameraError(code, error) {
    cameraError.value = code;
    cameraErrorMessage.value = CAMERA_ERROR_MESSAGES[code] || CAMERA_ERROR_MESSAGES.unknown;

    if (error) {
      console.error("Camera access failed", error);
    }
  }

  function clearCameraError() {
    cameraError.value = "";
    cameraErrorMessage.value = "";
  }

  function resolveCameraError(error) {
    switch (error?.name) {
      case "NotAllowedError":
      case "PermissionDeniedError":
      case "SecurityError":
        return "permission_denied";
      case "NotFoundError":
      case "DevicesNotFoundError":
        return "not_found";
      case "NotReadableError":
      case "TrackStartError":
        return "in_use";
      case "OverconstrainedError":
      case "ConstraintNotSatisfiedError":
        return "constraints";
      default:
        return "unknown";
    }
  }

  async function requestCamera(mode) {
    try {
      return await navigator.mediaDevices.getUserMedia({
        video: { facingMode: mode },
        audio: false
      });
    } catch (error) {
      const canFallback = error?.name === "OverconstrainedError" || error?.name === "ConstraintNotSatisfiedError";
      if (!canFallback) {
        throw error;
      }

      return navigator.mediaDevices.getUserMedia({ video: true, audio: false });
    }
  }

  async function startCamera(videoRef, mode) {
    if (stream.value) {
      stream.value.getTracks().forEach(tr => tr.stop());
    }
    ready.value = false;
    switching.value = true;
    clearCameraError();

    try {
      if (!navigator.mediaDevices?.getUserMedia) {
        setCameraError("unsupported");
        return;
      }

      const s = await requestCamera(mode);
      stream.value = s;
      await nextTick();
      if (videoRef) {
        videoRef.onloadedmetadata = () => {
          ready.value = true;
          switching.value = false;
        };
        videoRef.srcObject = s;
        await videoRef.play?.().catch(() => {});
      } else {
        switching.value = false;
      }
    } catch (e) {
      if (window.isSecureContext === false) {
        setCameraError("insecure", e);
      } else {
        setCameraError(resolveCameraError(e), e);
      }
    } finally {
      switching.value = false;
    }
  }

  function openCamera(videoRef) {
    showCam.value = true;
    facing.value = "user";
    clearCameraError();
    navigator.mediaDevices?.enumerateDevices?.().then(devices => {
      hasMultipleCams.value = devices.filter(d => d.kind === "videoinput").length > 1;
    }).catch(() => {
      hasMultipleCams.value = false;
    });
    startCamera(videoRef, "user");
  }

  function closeCamera() {
    if (stream.value) {
      stream.value.getTracks().forEach(tr => tr.stop());
      stream.value = null;
    }
    showCam.value = false;
    ready.value = false;
  }

  function flipCamera(videoRef) {
    const next = facing.value === "user" ? "environment" : "user";
    facing.value = next;
    startCamera(videoRef, next);
  }

  function capturePhoto(videoEl, canvasEl) {
    if (!videoEl || !canvasEl) return null;
    canvasEl.width = videoEl.videoWidth;
    canvasEl.height = videoEl.videoHeight;
    const ctx = canvasEl.getContext("2d");

    if (facing.value === "user") {
      ctx.translate(videoEl.videoWidth, 0);
      ctx.scale(-1, 1);
    }
    ctx.drawImage(videoEl, 0, 0);
    const photoData = canvasEl.toDataURL("image/jpeg", 0.82);
    closeCamera();
    return photoData;
  }

  return {
    stream,
    ready,
    cameraError,
    cameraErrorMessage,
    facing,
    switching,
    hasMultipleCams,
    showCam,
    startCamera,
    openCamera,
    closeCamera,
    flipCamera,
    capturePhoto
  };
}
