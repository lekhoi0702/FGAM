import Swal from 'sweetalert2'

const getGlobalText = (key, fallback) => {
  const value = globalThis?.[key]
  return typeof value === 'string' && value.trim() ? value.trim() : fallback
}

const escapeHtml = (value) => String(value ?? '')
  .replaceAll('&', '&amp;')
  .replaceAll('<', '&lt;')
  .replaceAll('>', '&gt;')
  .replaceAll('"', '&quot;')
  .replaceAll("'", '&#39;')

export function useSweetAlert() {
  async function showError(message) {
    await Swal.fire({
      icon: 'error',
      title: getGlobalText('hfSwalErrorTitle', 'Error'),
      text: message || '',
      timer: 1400,
      showConfirmButton: false,
      buttonsStyling: false,
      customClass: {
        container: 'swal2-container-over-modal',
        popup: 'of-swal-popup',
        title: 'of-swal-title',
        htmlContainer: 'of-swal-html',
        actions: 'of-swal-actions',
        confirmButton: 'of-swal-btn of-swal-btn-confirm'
      }
    })
  }

  async function showSuccess(message) {
    await Swal.fire({
      icon: 'success',
      text: message || getGlobalText('hfSwalSuccessText', 'Success'),
      timer: 1200,
      showConfirmButton: false,
      customClass: {
        container: 'swal2-container-over-modal'
      }
    })
  }

  async function showConfirm(title = 'Confirm?', okText = null, cancelText = 'Cancel') {
    const result = await Swal.fire({
      icon: 'question',
      title: title,
      showCancelButton: true,
      confirmButtonText: okText || getGlobalText('hfSwalOkBtn', 'OK'),
      cancelButtonText: cancelText,
      buttonsStyling: false,
      customClass: {
        container: 'swal2-container-over-modal',
        popup: 'of-swal-popup',
        title: 'of-swal-title',
        htmlContainer: 'of-swal-html',
        actions: 'of-swal-actions',
        confirmButton: 'of-swal-btn of-swal-btn-confirm',
        cancelButton: 'of-swal-btn'
      }
    })
    return !!result.isConfirmed
  }

  async function showImportResult(result, options = {}) {
    const errors = Array.isArray(result?.errors) ? result.errors : []
    const itemKey = options.itemKey || 'id'
    const itemLabel = options.itemLabel || 'ID'
    const rowsHtml = errors.length
      ? errors.map((errorItem) => {
          const rowNumber = escapeHtml(errorItem.rowNumber || '')
          const itemValue = escapeHtml(errorItem[itemKey] || '(blank)')
          const message = escapeHtml(errorItem.message || '')
          return `<div style="padding:6px 0;border-bottom:1px solid #fee2e2;"><strong>Row ${rowNumber}</strong> - ${itemLabel}: ${itemValue}<br><span>${message}</span></div>`
        }).join('')
      : '<div style="color:#047857;font-weight:600;">No validation errors.</div>'

    await Swal.fire({
      icon: errors.length ? 'error' : 'success',
      title: options.title || 'Import Result',
      width: 560,
      html: `
        <div style="text-align:left;font-size:13px;color:#334155;">
          <div style="display:grid;grid-template-columns:1fr 1fr 1fr;gap:8px;margin-bottom:12px;">
            <div><strong>Total</strong><br>${escapeHtml(result?.totalRows ?? 0)}</div>
            <div><strong>Inserted</strong><br>${escapeHtml(result?.insertedCount ?? 0)}</div>
            <div><strong>Skipped</strong><br>${escapeHtml(result?.skippedCount ?? 0)}</div>
          </div>
          <div style="max-height:260px;overflow-y:auto;border:1px solid ${errors.length ? '#fecaca' : '#bbf7d0'};border-radius:8px;padding:8px;background:${errors.length ? '#fff7f7' : '#f0fdf4'};">
            ${rowsHtml}
          </div>
        </div>
      `,
      confirmButtonText: getGlobalText('hfSwalOkBtn', 'OK'),
      buttonsStyling: false,
      customClass: {
        container: 'swal2-container-over-modal',
        popup: 'of-swal-popup',
        title: 'of-swal-title',
        htmlContainer: 'of-swal-html',
        actions: 'of-swal-actions',
        confirmButton: 'of-swal-btn of-swal-btn-confirm'
      }
    })
  }

  return {
    showError,
    showSuccess,
    showConfirm,
    showImportResult
  }
}
