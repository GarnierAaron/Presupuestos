interface ConfirmDialogProps {
  title: string
  message: string
  confirmLabel?: string
  cancelLabel?: string
  danger?: boolean
  onConfirm: () => void
  onCancel: () => void
}

/**
 * Modal de confirmación accesible que reemplaza window.confirm.
 * Se renderiza dentro de un backdrop con foco gestionado.
 */
export function ConfirmDialog({
  title,
  message,
  confirmLabel = 'Confirmar',
  cancelLabel = 'Cancelar',
  danger = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  return (
    <div
      className="modal-backdrop"
      role="presentation"
      onClick={(e) => {
        if (e.target === e.currentTarget) onCancel()
      }}
    >
      <div
        className="modal"
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="confirm-dialog-title"
        aria-describedby="confirm-dialog-message"
      >
        <div className="modal__head">
          <h2 id="confirm-dialog-title" style={{ fontSize: '1rem' }}>
            {title}
          </h2>
        </div>
        <div className="modal__body">
          <p id="confirm-dialog-message" style={{ margin: '0 0 1.25rem', color: 'var(--muted)', lineHeight: 1.6 }}>
            {message}
          </p>
          <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
            <button
              type="button"
              id="confirm-dialog-cancel"
              className="btn btn--ghost btn--small"
              onClick={onCancel}
            >
              {cancelLabel}
            </button>
            <button
              type="button"
              id="confirm-dialog-confirm"
              className={`btn btn--small ${danger ? 'btn--danger' : 'btn--primary'}`}
              // eslint-disable-next-line jsx-a11y/no-autofocus
              autoFocus
              onClick={onConfirm}
            >
              {confirmLabel}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
