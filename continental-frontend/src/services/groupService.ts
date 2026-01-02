import { httpClient } from "@/services/httpClient";
import type { ApiResponse } from "@/interfaces/Api.interface";

export interface UpdateGroupLeaderRequest {
    userId: number;
}

export interface UpdateGroupShiftRequest {
    personasPorTurno: number;
    duracionDeturno: number;
}

export const groupService = {
    async updateGroupLeader(groupId: number, userId: number): Promise<void> {
        try {
            console.log(`Updating group ${groupId} leader to user ${userId}`);

            // El backend espera solo el número, no un objeto
            const response = await httpClient.put<ApiResponse<void>>(
                `/api/Grupo/${groupId}/Lider`,
                userId
            );

            console.log(`Group ${groupId} leader update response:`, response);
            console.log(`Successfully updated group ${groupId} leader`);
        } catch (error: any) {
            // Check if it's actually a success (204 No Content)
            if (error?.status === 204 || error?.response?.status === 204) {
                console.log(`Group ${groupId} leader updated successfully (204 No Content)`);
                return; // It's actually a success
            }

            console.error(`Error updating group ${groupId} leader:`, error);
            throw error;
        }
    },

    async removeGroupLeader(groupId: number): Promise<void> {
        try {
            console.log(`Removing leader from group ${groupId}`);

            // Enviar 0 para remover líder
            const response = await httpClient.put<ApiResponse<void>>(
                `/api/Grupo/${groupId}/Lider`,
                0
            );

            console.log(`Group ${groupId} leader removal response:`, response);
            console.log(`Successfully removed leader from group ${groupId}`);
        } catch (error: any) {
            // Check if it's actually a success (204 No Content)
            if (error?.status === 204 || error?.response?.status === 204) {
                console.log(`Group ${groupId} leader removed successfully (204 No Content)`);
                return; // It's actually a success
            }

            console.error(`Error removing leader from group ${groupId}:`, error);
            throw error;
        }
    },

    async updateGroupShift(groupId: number, shiftData: UpdateGroupShiftRequest): Promise<void> {
        try {
            console.log(`Updating group ${groupId} shift data:`, shiftData);

            const response = await httpClient.put<ApiResponse<void>>(
                `/api/Grupo/${groupId}/Turno`,
                shiftData
            );

            console.log(`Group ${groupId} shift update response:`, response);
            console.log(`Successfully updated group ${groupId} shift`);
        } catch (error: any) {
            // Check if it's actually a success (204 No Content)
            if (error?.status === 204 || error?.response?.status === 204) {
                console.log(`Group ${groupId} shift updated successfully (204 No Content)`);
                return; // It's actually a success
            }

            console.error(`Error updating group ${groupId} shift:`, error);
            throw error;
        }
    }
};