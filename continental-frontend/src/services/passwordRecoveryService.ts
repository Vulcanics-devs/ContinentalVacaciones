import { httpClient } from './httpClient';
import type { 
  SolicitarCodigoRequest, 
  SolicitarCodigoResponse,
  ValidarCodigoRequest,
  ValidarCodigoResponse,
  CambiarPasswordRequest,
  CambiarPasswordResponse,
  ApiResponse
} from '@/interfaces/Api.interface';

export const passwordRecoveryService = {
  /**
   * Solicita el envío de un código de verificación al email del usuario
   */
  async solicitarCodigo(request: SolicitarCodigoRequest): Promise<SolicitarCodigoResponse> {
    try {
      console.log('🔍 Enviando solicitud de código:', request);
      
      const response: ApiResponse<SolicitarCodigoResponse> = await httpClient.post<SolicitarCodigoResponse>(
        '/api/recuperacion-password/solicitar-codigo',
        request
      );
      
      console.log('📥 Respuesta del servidor:', response);
      
      if (response.success && response.data) {
        return response.data;
      }
      
      // Si la respuesta no es exitosa pero tenemos data
      if (response.data) {
        return response.data;
      }
      
      // Por seguridad, siempre retorna éxito aunque haya error
      return {
        success: true,
        message: 'Si el correo está registrado, recibirás un código de verificación.',
        minutosExpiracion: 15
      };
    } catch (error: any) {
      console.error('❌ Error en solicitarCodigo:', error);
      console.error('❌ Error details:', error.response?.data);
      
      // Si es un error 400, podría ser que el endpoint no esté implementado
      if (error.status === 400) {
        console.warn('⚠️ Error 400: Posiblemente el endpoint no esté implementado en el backend');
      }
      
      // Por seguridad, siempre retorna éxito aunque haya error
      return {
        success: true,
        message: 'Si el correo está registrado, recibirás un código de verificación.',
        minutosExpiracion: 15
      };
    }
  },

  /**
   * Valida si un código es correcto sin usarlo (opcional)
   */
  async validarCodigo(request: ValidarCodigoRequest): Promise<ValidarCodigoResponse> {
    try {
      const response: ApiResponse<ValidarCodigoResponse> = await httpClient.post<ValidarCodigoResponse>(
        '/api/recuperacion-password/validar-codigo',
        request
      );
      
      if (response.success && response.data) {
        return response.data;
      }
      
      return {
        valido: false,
        message: response.errorMsg || 'Error al validar el código',
        intentosRestantes: null
      };
    } catch (error: any) {
      return {
        valido: false,
        message: 'Error al validar el código',
        intentosRestantes: null
      };
    }
  },

  /**
   * Cambia la contraseña usando un código de verificación válido
   */
  async cambiarPassword(request: CambiarPasswordRequest): Promise<CambiarPasswordResponse> {
    try {
      const response: ApiResponse<CambiarPasswordResponse> = await httpClient.post<CambiarPasswordResponse>(
        '/api/recuperacion-password/cambiar-password',
        request
      );
      
      if (response.success && response.data) {
        return response.data;
      }
      
      return {
        success: false,
        message: response.errorMsg || 'Error al cambiar la contraseña'
      };
    } catch (error: any) {
      return {
        success: false,
        message: 'Error al cambiar la contraseña'
      };
    }
  }
};
