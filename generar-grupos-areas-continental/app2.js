// Carga de variables de entorno
require('dotenv').config();

// Dependencias para SQL Server
// Si hay usuario/contraseña usamos el driver por defecto (tedious a través de mssql)
// Si NO hay usuario/contraseña, usamos autenticación integrada con msnodesqlv8
const hasSqlAuth = !!(process.env.SQL_USERNAME && process.env.SQL_PASSWORD);
const sql = hasSqlAuth ? require('mssql') : require('mssql/msnodesqlv8');

const server = process.env.SQL_SERVER;
const database = process.env.SQL_DATABASE;
const username = process.env.SQL_USERNAME;
const password = process.env.SQL_PASSWORD;
const schema = process.env.SQL_SCHEMA || 'dbo';
const encargadoRegistro = process.env.SQL_ENCARGADO || null;

if (!server || !database) {
  console.error('Faltan variables de entorno: SQL_SERVER, SQL_DATABASE (y opcionalmente SQL_USERNAME, SQL_PASSWORD)');
  process.exit(1);
}

// Configuración de conexión
const config = hasSqlAuth
  ? {
      server,
      database,
      user: username,
      password: password,
      options: {
        trustServerCertificate: true,
      },
      pool: { max: 10, min: 0, idleTimeoutMillis: 30000 },
    }
  : {
      driver: 'msnodesqlv8',
      connectionString:
        `Driver=${process.env.SQL_DRIVER || '{ODBC Driver 17 for SQL Server}'};` +
        `Server=${server};` +
        `Database=${database};` +
        `Trusted_Connection=Yes;` +
        `TrustServerCertificate=Yes;`,
      pool: { max: 10, min: 0, idleTimeoutMillis: 30000 },
    };

// Mapeo UnidadOrganizativa → Nombre General (idéntico a main.py)
const mapEquivalencias = {
  '80805 A 127 CONSTRUCCION LLANTAS RADIAL': 'Construcción',
  '80805 A 101 Corte de Hule y Compuestos': 'Banbury',
  '80805 A 111 PREPARACION DE MATERIALES': 'Prep. De Materiales',
  '80805 A 132 Vulcanizado': 'Vulcanización',
  '80805 A 141 ACABADO REPARACION INSPECCIO': 'Acabado',
  '80805 A 105 TUBULADO': 'Tubuladoras',
  '80805 A612 Taller de Moldes': 'Lab. De Moldes',
  '80805 A 609 Mantenimiento Electrico II': 'Mtto. B',
  '80805 A 607 MANTENIMIENTO AREA III': 'Mtto. C Vulca',
  '80805 A 605 MANTENIMIENTO AREA I': 'Mtto. A',
  '80805 A 611 Lava Moldes': 'Moldes Vulca',
  '80805 A 609 Mantenimiento Electrico': 'Mtto. A',
  '80805 A 606 Mantenimiento Area II': 'Mtto. B',
  '80805 A 609 Mantenimiento Electrico III': 'Mtto. C Vulca',
  '80805 A 601 Almacen Materias Primas': 'AMP',
  '80805 A 800 Relaciones Laborales MX': 'Sindicato',
  '80805 A_104 CALANDRIADO': 'Calandria',
  '80805 A 800 Instructores Tecnicos': 'Instructor Tecnico',
  '80805 A 790 Construccion de Bladders': 'Bladders',
  '80805 A 612 Aire Vapor Vacio Agua': 'Casa de Fuerza',
  '80805 A 610 Metrologia': 'Metrologia',
  '80805 A 703 Almacen de Producto Terminad': 'APT',
  '80805 A 102 Cementos': 'Cementos',
  '80805 PLT Vulcanizacion MX': 'Vulcanización MX'
};

async function main() {
  let pool;
  let transaction;
  try {
    pool = await sql.connect(config);
    transaction = new sql.Transaction(pool);
    await transaction.begin();

    // Lectura de empleados
    const reqRead = new sql.Request(transaction);
    let empleadosRes;
    try {
      empleadosRes = await reqRead.query(
        `SELECT Nomina, Nombre, FechaAlta, CentroCoste, Posicion, UnidadOrganizativa, EncargadoRegistro, Rol FROM [${schema}].[Empleados]`
      );
    } catch (readErr) {
      // Si columnas no existen, listar columnas reales para diagnóstico
      if (readErr?.code === 207 || (readErr?.original && readErr.original.code === 207)) {
        const diagReq = new sql.Request(transaction);
        diagReq.input('schema', sql.NVarChar, schema);
        const cols = await diagReq.query(
          `SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = 'Empleados' ORDER BY ORDINAL_POSITION`
        );
        console.error('Las columnas esperadas no existen en [${schema}].[Empleados]. Columnas disponibles:', cols.recordset.map(r => r.COLUMN_NAME));
      }
      throw readErr;
    }
    const empleados = empleadosRes.recordset || [];

    if (empleados.length === 0) {
      console.log('No hay empleados que procesar.');
      await transaction.commit();
      return;
    }

    // Agregar NombreGeneral a cada fila
    for (const row of empleados) {
      const uo = row.UnidadOrganizativa;
      row.NombreGeneral = uo ? mapEquivalencias[uo] || null : null;
    }

    // 4. Insertar Areas únicas (diferenciadas por UnidadOrganizativa + EncargadoRegistro)
    const areaIdMap = new Map(); // `${UnidadOrganizativa}|${EncargadoRegistro}` -> AreaId
    const areasVistas = new Set();
    
    // Primero, agrupar por UnidadOrganizativa para contar cuántos EncargadoRegistro diferentes hay
    const encargadosPorUO = new Map(); // UnidadOrganizativa -> Set de EncargadoRegistro
    for (const row of empleados) {
      const uo = row.UnidadOrganizativa;
      const encargado = row.EncargadoRegistro;
      if (!uo) continue;
      
      if (!encargadosPorUO.has(uo)) {
        encargadosPorUO.set(uo, new Set());
      }
      encargadosPorUO.get(uo).add(encargado);
    }
    
    // Crear mapeo de sufijos para cada UnidadOrganizativa
    const sufijosPorUO = new Map(); // UnidadOrganizativa -> Map(EncargadoRegistro -> sufijo)
    for (const [uo, encargados] of encargadosPorUO) {
      const encargadosArray = Array.from(encargados).sort(); // Ordenar para consistencia
      const sufijoMap = new Map();
      
      if (encargadosArray.length === 1) {
        // Si solo hay un encargado, no agregar sufijo
        sufijoMap.set(encargadosArray[0], '');
      } else {
        // Si hay múltiples encargados, agregar sufijos numéricos
        encargadosArray.forEach((encargado, index) => {
          sufijoMap.set(encargado, ` ${index + 1}`);
        });
      }
      
      sufijosPorUO.set(uo, sufijoMap);
    }
    
    // Ahora insertar las áreas con los nombres diferenciados
    for (const row of empleados) {
      const uo = row.UnidadOrganizativa;
      const encargado = row.EncargadoRegistro;
      if (!uo) continue;
      
      const key = `${uo}|${encargado}`;
      if (areasVistas.has(key)) continue;
      areasVistas.add(key);

      // Obtener el nombre base y el sufijo
      const nombreBase = mapEquivalencias[uo] || null;
      const sufijo = sufijosPorUO.get(uo)?.get(encargado) || '';
      const nombreGeneral = nombreBase ? `${nombreBase}${sufijo}` : null;

      const reqArea = new sql.Request(transaction);
      reqArea.input('uo', sql.VarChar, uo);
      reqArea.input('ng', sql.VarChar, nombreGeneral);
      reqArea.input('encargado', sql.VarChar, encargado);
      reqArea.input('manning', sql.Int, 0); // Valor por defecto para Manning
      reqArea.input('jefeId', sql.Int, null); // JefeId puede ser NULL
      reqArea.input('jefeSuplenteId', sql.Int, null); // JefeSuplenteId puede ser NULL
      const areaInsert = await reqArea.query(
        `INSERT INTO [${schema}].[Areas] (UnidadOrganizativaSap, NombreGeneral, JefeId, JefeSuplenteId, Manning, EncargadoRegistro) VALUES (@uo, @ng, @jefeId, @jefeSuplenteId, @manning, @encargado); SELECT SCOPE_IDENTITY() AS id;`
      );
      const areaId = areaInsert.recordset && areaInsert.recordset[0] && areaInsert.recordset[0].id;
      areaIdMap.set(key, areaId);
    }

    // 5. Insertar Grupos únicos por (UnidadOrganizativa, EncargadoRegistro, Rol)
    const grupoIdMap = new Map(); // `${uo}|${encargado}|${rol}` -> GrupoId
    const gruposVistos = new Set();
    for (const row of empleados) {
      const uo = row.UnidadOrganizativa;
      const encargado = row.EncargadoRegistro;
      const rol = row.Rol;
      if (!uo) continue;
      const gkey = `${uo}|${encargado}|${rol}`;
      if (gruposVistos.has(gkey)) continue;
      gruposVistos.add(gkey);

      const areaKey = `${uo}|${encargado}`;
      const areaId = areaIdMap.get(areaKey);
      const reqGrupo = new sql.Request(transaction);
      reqGrupo.input('areaId', sql.Int, areaId);
      reqGrupo.input('rol', sql.VarChar, rol);
      const grupoInsert = await reqGrupo.query(
        `INSERT INTO [${schema}].[Grupos] (AreaId, Rol) VALUES (@areaId, @rol); SELECT SCOPE_IDENTITY() AS id;`
      );
      const grupoId = grupoInsert.recordset && grupoInsert.recordset[0] && grupoInsert.recordset[0].id;
      grupoIdMap.set(gkey, grupoId);
    }

    

    // // 6. Insertar en Sindicalizados desde Empleados (evita duplicados por Nomina)
    // for (const row of empleados) {
    //   const reqSind = new sql.Request(transaction);
    //   reqSind.input('Nomina', sql.Int, row.Nomina ?? null);
    //   reqSind.input('Nombre', sql.NVarChar, row.Nombre ?? null);
    //   reqSind.input('FechaAlta', sql.Date, row.FechaAlta ?? null);
    //   reqSind.input('CentroCoste', sql.Int, row.CentroCoste ?? null);
    //   reqSind.input('Posicion', sql.NVarChar, row.Posicion ?? null);
    //   reqSind.input('Encargado', sql.NVarChar, encargadoRegistro);
    //   await reqSind.query(
    //     `INSERT INTO [${schema}].[Sindicalizados] (Nomina, Nombre, FechaAlta, CentroCoste, Posicion, EncargadoRegistro)
    //      SELECT @Nomina, @Nombre, @FechaAlta, @CentroCoste, @Posicion, @Encargado
    //      WHERE NOT EXISTS (SELECT 1 FROM [${schema}].[Sindicalizados] WHERE Nomina = @Nomina);`
    //   );
    // }

    // // 7. Insertar SindicalizadosPorGrupo (con SindicalizadoId obtenido por Nomina) evitando duplicados
    // for (const row of empleados) {
    //   const uo = row.UnidadOrganizativa;
    //   const encargado = row.EncargadoRegistro;
    //   const rol = row.Rol;
    //   const gkey = `${uo}|${encargado}|${rol}`;
    //   const grupoId = grupoIdMap.get(gkey);
    //   const reqSPG = new sql.Request(transaction);
    //   reqSPG.input('Nomina', sql.Int, row.Nomina ?? null);
    //   reqSPG.input('gId', sql.Int, grupoId);
    //   await reqSPG.query(
    //     `INSERT INTO [${schema}].[SindicalizadosPorGrupo] (SindicalizadoId, GrupoId)
    //      SELECT s.SindicalizadoId, @gId
    //      FROM [${schema}].[Sindicalizados] s
    //      WHERE s.Nomina = @Nomina
    //        AND NOT EXISTS (
    //          SELECT 1 FROM [${schema}].[SindicalizadosPorGrupo]
    //          WHERE SindicalizadoId = s.SindicalizadoId AND GrupoId = @gId
    //        );`
    //   );
    // }

    await transaction.commit();
    console.log('Carga completada.');
  } catch (err) {
    try {
      if (transaction && transaction._acquiredConnection) {
        await transaction.rollback();
      }
    } catch (_) {}
    console.error('Error durante la carga:', {
      message: err?.message,
      code: err?.code,
      original: err?.originalError || err?.precedingErrors || err,
    });
    process.exitCode = 1;
  } finally {
    try {
      await sql.close();
    } catch (_) {}
  }
}

main();
