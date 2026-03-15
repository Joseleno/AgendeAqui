export type ValidationRule = (value: string) => string | null

export function validateRequired(label: string): ValidationRule {
  return (value) => value.trim() ? null : `${label} é obrigatório`
}

export function validateEmail(): ValidationRule {
  return (value) => {
    if (!value.trim()) return null
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value) ? null : 'Email inválido'
  }
}

export function validatePhone(): ValidationRule {
  return (value) => {
    if (!value.trim()) return null
    const digits = value.replace(/\D/g, '')
    return digits.length >= 10 && digits.length <= 13 ? null : 'Telefone inválido'
  }
}

export function validateMinLength(label: string, min: number): ValidationRule {
  return (value) => {
    if (!value) return null
    return value.length >= min ? null : `${label} deve ter no mínimo ${min} caracteres`
  }
}

export function validateMatch(otherValue: string, message: string): ValidationRule {
  return (value) => value === otherValue ? null : message
}

export function validate(value: string, ...rules: ValidationRule[]): string | null {
  for (const rule of rules) {
    const error = rule(value)
    if (error) return error
  }
  return null
}
