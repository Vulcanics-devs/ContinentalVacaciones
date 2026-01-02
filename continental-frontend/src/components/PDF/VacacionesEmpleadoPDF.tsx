import React from 'react';
import { Document, Page, Text, View, StyleSheet, PDFDownloadLink } from '@react-pdf/renderer';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

// Estilos para el PDF
const styles = StyleSheet.create({
  page: {
    flexDirection: 'column',
    backgroundColor: '#ffffff',
    padding: 30,
  },
  header: {
    marginBottom: 20,
    borderBottom: '2px solid #0A4AA3',
    paddingBottom: 10,
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#0A4AA3',
    marginBottom: 5,
  },
  subtitle: {
    fontSize: 12,
    color: '#666',
  },
  infoSection: {
    marginBottom: 20,
    padding: 10,
    backgroundColor: '#f8f9fa',
    borderRadius: 5,
  },
  infoRow: {
    flexDirection: 'row',
    marginBottom: 5,
  },
  infoLabel: {
    fontSize: 10,
    fontWeight: 'bold',
    width: 150,
    color: '#333',
  },
  infoValue: {
    fontSize: 10,
    color: '#666',
  },
  sectionTitle: {
    fontSize: 14,
    fontWeight: 'bold',
    color: '#0A4AA3',
    marginTop: 20,
    marginBottom: 10,
    padding: 5,
    backgroundColor: '#f0f7ff',
  },
  vacationSection: {
    marginBottom: 20,
  },
  vacationListHeader: {
    flexDirection: 'row',
    backgroundColor: '#0A4AA3',
    padding: 8,
    marginBottom: 5,
  },
  vacationListHeaderText: {
    color: '#ffffff',
    fontSize: 10,
    fontWeight: 'bold',
  },
  vacationItem: {
    flexDirection: 'row',
    padding: 8,
    borderBottomWidth: 1,
    borderBottomColor: '#e0e0e0',
  },
  vacationItemText: {
    fontSize: 9,
    color: '#333',
    flex: 1,
  },
  summarySection: {
    marginTop: 20,
    padding: 15,
    backgroundColor: '#f0f7ff',
    borderRadius: 5,
  },
  summaryRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 8,
  },
  summaryLabel: {
    fontSize: 11,
    fontWeight: 'bold',
    color: '#333',
  },
  summaryValue: {
    fontSize: 11,
    color: '#0A4AA3',
    fontWeight: 'bold',
  },
  footer: {
    position: 'absolute',
    bottom: 30,
    left: 30,
    right: 30,
    textAlign: 'center',
    fontSize: 8,
    color: '#999',
  },
  divider: {
    marginVertical: 10,
    borderBottomWidth: 1,
    borderBottomColor: '#e0e0e0',
  },
});

// Interfaz para los datos de vacaciones
export interface VacacionesEmpleadoData {
  empleado: {
    nombre: string;
    username: string;
    area?: string;
    grupo?: string;
  };
  periodo: {
    inicio: string;
  };
  diasSeleccionados: { date: string }[];
  diasAsignados: { date: string }[];
  resumen?: {
    diasDisponibles?: number;
    diasProgramados?: number;
    diasRestantes?: number;
  };
}

// Componente del documento PDF
const VacacionesEmpleadoDocument: React.FC<{ data: VacacionesEmpleadoData }> = ({ data }) => {
  

  const formatShortDate = (dateString: string) => {
    try {
      const date = dateString.includes('-')
        ? new Date(dateString + 'T00:00:00')
        : new Date(dateString);
      return format(date, "dd/MM/yyyy", { locale: es });
    } catch {
      return dateString;
    }
  };

  // Agrupar días consecutivos para mejor visualización
  const groupConsecutiveDays = (days: { date: string }[]) => {
    if (!days || days.length === 0) return [];

    const sortedDays = [...days].sort((a, b) =>
      new Date(a.date).getTime() - new Date(b.date).getTime()
    );

    const groups: { start: string; end: string; count: number }[] = [];
    let currentGroup = { start: sortedDays[0].date, end: sortedDays[0].date, count: 1 };

    for (let i = 1; i < sortedDays.length; i++) {
      const prevDate = new Date(sortedDays[i - 1].date);
      const currDate = new Date(sortedDays[i].date);
      const diffDays = (currDate.getTime() - prevDate.getTime()) / (1000 * 60 * 60 * 24);

      if (diffDays === 1) {
        currentGroup.end = sortedDays[i].date;
        currentGroup.count++;
      } else {
        groups.push({ ...currentGroup });
        currentGroup = { start: sortedDays[i].date, end: sortedDays[i].date, count: 1 };
      }
    }
    groups.push(currentGroup);

    return groups;
  };

  const selectedGroups = groupConsecutiveDays(data.diasSeleccionados);
  const assignedGroups = groupConsecutiveDays(data.diasAsignados);

  return (
    <Document>
      <Page size="A4" style={styles.page}>
        {/* Header */}
        <View style={styles.header}>
          <Text style={styles.title}>Reporte de Vacaciones</Text>
          <Text style={styles.subtitle}>
            Generado el {format(new Date(), "dd 'de' MMMM 'de' yyyy, HH:mm", { locale: es })}
          </Text>
        </View>

        {/* Información del empleado */}
        <View style={styles.infoSection}>
          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Empleado:</Text>
            <Text style={styles.infoValue}>{data.empleado.nombre}</Text>
          </View>
          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Número de nómina:</Text>
            <Text style={styles.infoValue}>{data.empleado.username}</Text>
          </View>
          {data.empleado.area && (
            <View style={styles.infoRow}>
              <Text style={styles.infoLabel}>Área:</Text>
              <Text style={styles.infoValue}>{data.empleado.area}</Text>
            </View>
          )}
          {data.empleado.grupo && (
            <View style={styles.infoRow}>
              <Text style={styles.infoLabel}>Grupo:</Text>
              <Text style={styles.infoValue}>{data.empleado.grupo}</Text>
            </View>
          )}
          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Período de vacaciones:</Text>
            <Text style={styles.infoValue}>
              {data.periodo.inicio}
            </Text>
          </View>
        </View>

        {/* Días Seleccionados */}
        <View style={styles.vacationSection}>
          <Text style={styles.sectionTitle}>
            Días Seleccionados ({data.diasSeleccionados.length} días)
          </Text>

          <View style={styles.vacationListHeader}>
            <Text style={[styles.vacationListHeaderText, { width: '30%' }]}>Fecha Inicio</Text>
            <Text style={[styles.vacationListHeaderText, { width: '30%' }]}>Fecha Fin</Text>
            <Text style={[styles.vacationListHeaderText, { width: '20%' }]}>Días</Text>
            <Text style={[styles.vacationListHeaderText, { width: '20%' }]}>Estado</Text>
          </View>

          {selectedGroups.map((group, index) => (
            <View key={index} style={[
              styles.vacationItem,
              { backgroundColor: index % 2 === 0 ? '#ffffff' : '#f9f9f9' }
            ]}>
              <Text style={[styles.vacationItemText, { width: '30%' }]}>
                {formatShortDate(group.start)}
              </Text>
              <Text style={[styles.vacationItemText, { width: '30%' }]}>
                {formatShortDate(group.end)}
              </Text>
              <Text style={[styles.vacationItemText, { width: '20%' }]}>
                {group.count}
              </Text>
              <Text style={[styles.vacationItemText, { width: '20%', color: '#f59e0b' }]}>
                Asignado
              </Text>
            </View>
          ))}
        </View>

        {/* Días Asignados */}
        <View style={styles.vacationSection}>
          <Text style={styles.sectionTitle}>
            Días Asignados por el Sistema ({data.diasAsignados.length} días)
          </Text>

          <View style={styles.vacationListHeader}>
            <Text style={[styles.vacationListHeaderText, { width: '30%' }]}>Fecha Inicio</Text>
            <Text style={[styles.vacationListHeaderText, { width: '30%' }]}>Fecha Fin</Text>
            <Text style={[styles.vacationListHeaderText, { width: '20%' }]}>Días</Text>
            <Text style={[styles.vacationListHeaderText, { width: '20%' }]}>Estado</Text>
          </View>

          {assignedGroups.map((group, index) => (
            <View key={index} style={[
              styles.vacationItem,
              { backgroundColor: index % 2 === 0 ? '#ffffff' : '#f9f9f9' }
            ]}>
              <Text style={[styles.vacationItemText, { width: '30%' }]}>
                {formatShortDate(group.start)}
              </Text>
              <Text style={[styles.vacationItemText, { width: '30%' }]}>
                {formatShortDate(group.end)}
              </Text>
              <Text style={[styles.vacationItemText, { width: '20%' }]}>
                {group.count}
              </Text>
              <Text style={[styles.vacationItemText, { width: '20%', color: '#10b981' }]}>
                Confirmado
              </Text>
            </View>
          ))}
        </View>

        {/* Resumen */}
        {data.resumen && (
          <View style={styles.summarySection}>
            <Text style={[styles.sectionTitle, { marginTop: 0, marginBottom: 10 }]}>
              Resumen de Vacaciones
            </Text>

            <View style={styles.summaryRow}>
              <Text style={styles.summaryLabel}>Días disponibles:</Text>
              <Text style={styles.summaryValue}>{data.resumen.diasDisponibles || 0}</Text>
            </View>

            <View style={styles.summaryRow}>
              <Text style={styles.summaryLabel}>Días programados:</Text>
              <Text style={styles.summaryValue}>{data.resumen.diasProgramados || 0}</Text>
            </View>

            <View style={styles.divider} />

            <View style={styles.summaryRow}>
              <Text style={styles.summaryLabel}>Días restantes:</Text>
              <Text style={[styles.summaryValue, { color: '#ef4444' }]}>
                {data.resumen.diasRestantes || 0}
              </Text>
            </View>
          </View>
        )}

        {/* Footer */}
        <Text style={styles.footer}>
          Continental - Sistema de Gestión de Vacaciones
        </Text>
      </Page>
    </Document>
  );
};

// Componente para el botón de descarga PDF
export const VacacionesEmpleadoPDFDownloadLink: React.FC<{
  data: VacacionesEmpleadoData;
  children: React.ReactNode;
  className?: string;
}> = ({ data, children, className }) => {
  const fileName = `Vacaciones_${data.empleado.username}_${format(new Date(), 'yyyyMMdd_HHmmss')}.pdf`;

  return (
    <PDFDownloadLink
      document={<VacacionesEmpleadoDocument data={data} />}
      fileName={fileName}
      className={className}
    >
      {({ loading }) => (
        loading ? 'Generando PDF...' : children
      )}
    </PDFDownloadLink>
  );
};

export default VacacionesEmpleadoDocument;