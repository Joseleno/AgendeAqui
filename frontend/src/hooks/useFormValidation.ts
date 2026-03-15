import { useState, useCallback } from 'react'
import type { ValidationRule } from '../lib/validators'

type FieldRules = Record<string, ValidationRule[]>
type FieldErrors = Record<string, string | null>

export function useFormValidation<T extends Record<string, string>>(
  rules: FieldRules,
) {
  const [errors, setErrors] = useState<FieldErrors>({})
  const [touched, setTouched] = useState<Record<string, boolean>>({})

  const validateField = useCallback(
    (name: string, value: string) => {
      const fieldRules = rules[name]
      if (!fieldRules) return null
      for (const rule of fieldRules) {
        const error = rule(value)
        if (error) return error
      }
      return null
    },
    [rules],
  )

  const onBlur = useCallback(
    (name: string, value: string) => {
      setTouched((prev) => ({ ...prev, [name]: true }))
      const error = validateField(name, value)
      setErrors((prev) => ({ ...prev, [name]: error }))
    },
    [validateField],
  )

  const validateAll = useCallback(
    (values: T): boolean => {
      const newErrors: FieldErrors = {}
      let isValid = true
      for (const [name, fieldRules] of Object.entries(rules)) {
        for (const rule of fieldRules) {
          const error = rule(values[name] ?? '')
          if (error) {
            newErrors[name] = error
            isValid = false
            break
          }
        }
      }
      setErrors(newErrors)
      setTouched(
        Object.keys(rules).reduce((acc, key) => ({ ...acc, [key]: true }), {}),
      )
      return isValid
    },
    [rules],
  )

  const getError = useCallback(
    (name: string) => (touched[name] ? errors[name] ?? null : null),
    [errors, touched],
  )

  const clearErrors = useCallback(() => {
    setErrors({})
    setTouched({})
  }, [])

  return { errors, touched, onBlur, validateAll, getError, clearErrors }
}
