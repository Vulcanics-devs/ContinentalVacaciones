import csv
import pyodbc
from datetime import datetime

# Database connection
conn_str = (
    'DRIVER={ODBC Driver 18 for SQL Server};'
    'SERVER=localhost;'
    'DATABASE=Vacaciones;'
    'UID=sa;'
    'PWD=YourStrong@Passw0rd;'
    'TrustServerCertificate=yes;'
)

conn = pyodbc.connect(conn_str)
cursor = conn.cursor()

print("Connected to database successfully!")

# First, let's populate Areas and Grupos from the group data
print("\n=== Processing Groups Data ===")
with open('New Data/personas por grupo.xlsx - Sheet1.csv', 'r', encoding='utf-8') as f:
    reader = csv.reader(f)
    rows = list(reader)

    # Skip header rows (first 2 rows)
    for row in rows[2:]:
        if len(row) > 3 and row[3].strip():
            grupo_name = row[3].strip()
            manning = row[4].strip() if len(row) > 4 and row[4].strip() else '0'
            personas_por_dia = row[5].strip() if len(row) > 5 and row[5].strip() else '0'
            personas_por_grupo = row[6].strip() if len(row) > 6 and row[6].strip() else '0'

            # For now, we'll create a single area "Producción" and add all groups under it
            # Check if area exists
            cursor.execute("SELECT AreaId FROM Areas WHERE NombreDeArea = 'Producción'")
            area = cursor.fetchone()

            if not area:
                cursor.execute("""
                    INSERT INTO Areas (NombreDeArea, Manning)
                    OUTPUT INSERTED.AreaId
                    VALUES ('Producción', 0.0)
                """)
                area_id = cursor.fetchone()[0]
                conn.commit()
                print(f"Created area: Producción (ID: {area_id})")
            else:
                area_id = area[0]

            # Check if group exists
            cursor.execute("SELECT GrupoId FROM Grupos WHERE NombreGrupo = ?", (grupo_name,))
            existing_grupo = cursor.fetchone()

            if not existing_grupo:
                # Create SAP identifier from group name (simple version)
                sap_id = grupo_name.replace(' ', '_').upper()[:20]

                cursor.execute("""
                    INSERT INTO Grupos (NombreGrupo, AreaId, IdentificadorSAP, Manning, PersonasPorDia, PersonasPorGrupo)
                    VALUES (?, ?, ?, ?, ?, ?)
                """, (grupo_name, area_id, sap_id, manning, personas_por_dia, personas_por_grupo))
                conn.commit()
                print(f"Created group: {grupo_name} (Manning: {manning})")

print("\n=== Processing Employee Data ===")

# Get all grupos mapped by name
cursor.execute("SELECT GrupoId, NombreGrupo FROM Grupos")
grupos_map = {row[1]: row[0] for row in cursor.fetchall()}

# Get default area
cursor.execute("SELECT AreaId FROM Areas WHERE NombreDeArea = 'Producción'")
default_area_id = cursor.fetchone()[0]

# Get the EmpleadoSindicalizado role ID
cursor.execute("SELECT RolId FROM Roles WHERE NombreRol = 'EmpleadoSindicalizado'")
empleado_role = cursor.fetchone()
if not empleado_role:
    print("WARNING: EmpleadoSindicalizado role not found!")
    empleado_role_id = None
else:
    empleado_role_id = empleado_role[0]

with open('New Data/Listado Octubre Sindicalizados.xlsx - Octubre.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    count = 0

    for row in reader:
        nomina = row['Nómina'].strip()
        nombre = row['Nombre'].strip()
        alta_str = row['Alta'].strip()
        ce_coste = row['Ce.coste'].strip()
        posicion = row['Posición'].strip()
        unidad_org = row['Unidad organizativa'].strip()
        encargado = row['Encargado para registro de tie'].strip()
        regla = row['Regla '].strip()
        turno = row['Turno'].strip()

        # Parse date (format: 1/4/2016)
        try:
            fecha_alta = datetime.strptime(alta_str, '%m/%d/%Y')
        except:
            try:
                fecha_alta = datetime.strptime(alta_str, '%d/%m/%Y')
            except:
                fecha_alta = datetime.now()

        # Try to match grupo from unidad organizativa
        grupo_id = None
        for grupo_name, gid in grupos_map.items():
            if grupo_name.upper() in unidad_org.upper():
                grupo_id = gid
                break

        # Check if user already exists
        cursor.execute("SELECT UserId FROM Users WHERE NumeroNomina = ?", (nomina,))
        existing_user = cursor.fetchone()

        if existing_user:
            print(f"Skipping existing user: {nomina} - {nombre}")
            continue

        # Create user
        cursor.execute("""
            INSERT INTO Users (
                NumeroNomina,
                Nombre,
                Maquina,
                FechaDeIngreso,
                AreaId,
                GrupoId
            )
            OUTPUT INSERTED.UserId
            VALUES (?, ?, ?, ?, ?, ?)
        """, (nomina, nombre, posicion, fecha_alta, default_area_id, grupo_id))

        user_id = cursor.fetchone()[0]

        # Assign role
        if empleado_role_id:
            cursor.execute("""
                INSERT INTO UserRoles (UserId, RolId)
                VALUES (?, ?)
            """, (user_id, empleado_role_id))

        # Create Empleado record
        cursor.execute("""
            INSERT INTO Empleados (
                UserId,
                NumeroNomina,
                NombreCompleto,
                CentroDeCosto,
                Posicion,
                UnidadOrganizativa,
                EncargadoRegistro
            )
            VALUES (?, ?, ?, ?, ?, ?, ?)
        """, (user_id, nomina, nombre, ce_coste, posicion, unidad_org, encargado))

        # Create Sindicalizado record
        cursor.execute("""
            INSERT INTO Sindicalizados (UserId, NumeroNomina, NombreCompleto)
            VALUES (?, ?, ?)
        """, (user_id, nomina, nombre))

        count += 1
        if count % 50 == 0:
            conn.commit()
            print(f"Processed {count} employees...")

    conn.commit()
    print(f"\nTotal employees created: {count}")

print("\n=== Database Population Complete! ===")
cursor.close()
conn.close()
