/**
 * Validation Utilities
 * Common validation functions for forms and data
 */

export interface ValidationResult {
  isValid: boolean;
  errors: string[];
}

export const validators = {
  email: (email: string): ValidationResult => {
    const errors: string[] = [];
    
    if (!email) {
      errors.push('El correo es requerido');
    } else {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(email)) {
        errors.push('El formato del correo no es válido');
      }
    }
    
    return {
      isValid: errors.length === 0,
      errors,
    };
  },

  password: (password: string): ValidationResult => {
    const errors: string[] = [];
    
    if (!password) {
      errors.push('La contraseña es requerida');
    } else {
      if (password.length < 6) {
        errors.push('La contraseña debe tener al menos 6 caracteres');
      }
      if (password.length > 50) {
        errors.push('La contraseña no puede tener más de 50 caracteres');
      }
    }
    
    return {
      isValid: errors.length === 0,
      errors,
    };
  },

  nomina: (nomina: string): ValidationResult => {
    const errors: string[] = [];
    
    if (!nomina) {
      errors.push('El número de nómina es requerido');
    } else {
      // Basic nomina validation - adjust according to your business rules
      if (nomina.length < 3) {
        errors.push('El número de nómina debe tener al menos 3 caracteres');
      }
      if (nomina.length > 20) {
        errors.push('El número de nómina no puede tener más de 20 caracteres');
      }
      // Only alphanumeric characters
      if (!/^[a-zA-Z0-9]+$/.test(nomina)) {
        errors.push('El número de nómina solo puede contener letras y números');
      }
    }
    
    return {
      isValid: errors.length === 0,
      errors,
    };
  },

  required: (value: any, fieldName: string): ValidationResult => {
    const errors: string[] = [];
    
    if (!value || (typeof value === 'string' && value.trim() === '')) {
      errors.push(`${fieldName} es requerido`);
    }
    
    return {
      isValid: errors.length === 0,
      errors,
    };
  },

  minLength: (value: string, minLength: number, fieldName: string): ValidationResult => {
    const errors: string[] = [];
    
    if (value && value.length < minLength) {
      errors.push(`${fieldName} debe tener al menos ${minLength} caracteres`);
    }
    
    return {
      isValid: errors.length === 0,
      errors,
    };
  },

  maxLength: (value: string, maxLength: number, fieldName: string): ValidationResult => {
    const errors: string[] = [];
    
    if (value && value.length > maxLength) {
      errors.push(`${fieldName} no puede tener más de ${maxLength} caracteres`);
    }
    
    return {
      isValid: errors.length === 0,
      errors,
    };
  },
};

export const validateLoginForm = (email: string, password: string): ValidationResult => {
  const emailValidation = validators.email(email);
  const passwordValidation = validators.password(password);
  
  const allErrors = [...emailValidation.errors, ...passwordValidation.errors];
  
  return {
    isValid: allErrors.length === 0,
    errors: allErrors,
  };
};

export const validateEmployeeLoginForm = (nomina: string, password: string): ValidationResult => {
  const nominaValidation = validators.nomina(nomina);
  const passwordValidation = validators.password(password);
  
  const allErrors = [...nominaValidation.errors, ...passwordValidation.errors];
  
  return {
    isValid: allErrors.length === 0,
    errors: allErrors,
  };
};

export default validators;