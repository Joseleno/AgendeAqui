import type { ReactNode } from 'react'

interface FormFieldProps {
  label?: string
  error?: string | null
  children: ReactNode
  htmlFor?: string
}

export function FormField({ label, error, children, htmlFor }: FormFieldProps) {
  return (
    <div>
      {label && (
        <label
          htmlFor={htmlFor}
          className="block text-sm font-medium text-gray-700 mb-1"
        >
          {label}
        </label>
      )}
      {children}
      <div
        className={`overflow-hidden transition-all duration-200 ${
          error ? 'max-h-8 opacity-100 mt-1' : 'max-h-0 opacity-0'
        }`}
      >
        <p className="text-xs text-red-500 flex items-center gap-1">
          <svg className="w-3 h-3 shrink-0" viewBox="0 0 16 16" fill="currentColor">
            <path d="M8 1a7 7 0 100 14A7 7 0 008 1zm-.75 4.25a.75.75 0 011.5 0v3a.75.75 0 01-1.5 0v-3zM8 11a1 1 0 100-2 1 1 0 000 2z" />
          </svg>
          {error}
        </p>
      </div>
    </div>
  )
}
