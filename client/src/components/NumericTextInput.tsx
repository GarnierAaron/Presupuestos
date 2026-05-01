import {
  normalizeDecimalTyping,
  normalizeUnsignedIntTyping,
  trimLeadingZerosDecimal,
  trimLeadingZerosInt,
} from '../lib/numericStrings'

type Variant = 'decimal' | 'unsignedInt'

type Props = Omit<
  React.InputHTMLAttributes<HTMLInputElement>,
  'type' | 'value' | 'onChange' | 'inputMode'
> & {
  value: string
  onChange: (value: string) => void
  variant: Variant
  /** Si true, al perder foco normaliza ceros a la izquierda en enteros (evita "01"). */
  trimIntOnBlur?: boolean
  /** Si true y variant decimal, al blur quita ceros a la izquierda en la parte entera (evita "01", "012.3"). */
  trimDecimalLeadingZerosOnBlur?: boolean
}

/**
 * Sustituye `type="number"`: permite vacío, doble clic selecciona todo, sin "0" pegajoso.
 */
export function NumericTextInput({
  value,
  onChange,
  variant,
  trimIntOnBlur = true,
  trimDecimalLeadingZerosOnBlur = false,
  onBlur,
  onDoubleClick,
  ...rest
}: Props) {
  const handleChange = (raw: string) => {
    onChange(
      variant === 'decimal'
        ? normalizeDecimalTyping(raw)
        : normalizeUnsignedIntTyping(raw)
    )
  }

  return (
    <input
      {...rest}
      type="text"
      inputMode={variant === 'decimal' ? 'decimal' : 'numeric'}
      autoComplete="off"
      value={value}
      onChange={(e) => handleChange(e.target.value)}
      onDoubleClick={(e) => {
        ;(e.target as HTMLInputElement).select()
        onDoubleClick?.(e)
      }}
      onBlur={(e) => {
        const v = e.target.value
        if (v !== '') {
          if (variant === 'decimal' && trimDecimalLeadingZerosOnBlur) {
            onChange(trimLeadingZerosDecimal(v))
          } else if (variant === 'unsignedInt' && trimIntOnBlur) {
            onChange(trimLeadingZerosInt(v))
          }
        }
        onBlur?.(e)
      }}
    />
  )
}
