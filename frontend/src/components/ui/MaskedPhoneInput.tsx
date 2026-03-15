import { useCallback } from 'react'

interface MaskedPhoneInputProps {
  value: string
  onChange: (raw: string, display: string) => void
  className?: string
  placeholder?: string
  required?: boolean
  id?: string
  name?: string
  onBlur?: () => void
}

function formatPhoneDisplay(digits: string): string {
  if (digits.length <= 2) return `(${digits}`
  if (digits.length <= 7) return `(${digits.slice(0, 2)}) ${digits.slice(2)}`
  return `(${digits.slice(0, 2)}) ${digits.slice(2, 7)}-${digits.slice(7, 11)}`
}

function toStorageFormat(digits: string): string {
  return digits.length >= 10 ? `55${digits}` : digits
}

export function MaskedPhoneInput({
  value,
  onChange,
  className,
  placeholder = '(11) 99999-9999',
  required,
  id,
  name,
  onBlur,
}: MaskedPhoneInputProps) {
  const displayValue = (() => {
    const raw = value.replace(/\D/g, '')
    const national = raw.startsWith('55') ? raw.slice(2) : raw
    return national.length > 0 ? formatPhoneDisplay(national) : ''
  })()

  const handleChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const digits = e.target.value.replace(/\D/g, '').slice(0, 11)
      const display = digits.length > 0 ? formatPhoneDisplay(digits) : ''
      const storage = toStorageFormat(digits)
      onChange(storage, display)
    },
    [onChange],
  )

  return (
    <input
      id={id}
      name={name}
      type="tel"
      inputMode="numeric"
      required={required}
      value={displayValue}
      onChange={handleChange}
      onBlur={onBlur}
      className={className}
      placeholder={placeholder}
    />
  )
}
