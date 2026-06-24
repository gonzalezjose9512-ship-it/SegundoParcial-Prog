// ================================================================================
// EXAMEN PARCIAL: PERSISTENCIA Y ALGORITMIA AVANZADA EN .NET
// ================================================================================
// 
// TemÃ¡tica: Simulador de Trayectoria de un Dron Automatizado con ADO.NET y PostgreSQL
// DuraciÃ³n: 3 horas.
// Modalidad: PrÃ¡ctico individual - Remoto.
// 
// --------------------------------------------------------------------------------
// 1. EXPLICACIÃ“N DEL PROBLEMA: ESCANEO DEL TERRENO ALCANZABLE
// --------------------------------------------------------------------------------
// 
// El objetivo de este ejercicio es programar el sistema de navegaciÃ³n de un Dron
// de InspecciÃ³n. El dron opera dentro de un terreno cuadrado dividido en parcelas
// de tamaÃ±o variable (N x N). Para cumplir su directiva, el dron debe despegar
// desde una parcela inicial y recorrer todas las parcelas que pueda ALCANZAR con
// su patrÃ³n de movimiento, pisando cada una exactamente una vez (ver la condiciÃ³n
// de Ã©xito mÃ¡s abajo para el detalle de quÃ© parcelas cuentan como alcanzables).
// 
// ConvenciÃ³n de coordenadas: en todo este enunciado X = FILA e Y = COLUMNA, ambas
// en el rango [0, N-1]. Por ejemplo, la coordenada (5,3) es la fila 5, columna 3.
// 
// Reglas de navegaciÃ³n del dron:
// 
// * EL PATRÃ“N DE VUELO: El dron se desplaza con un movimiento fijo en forma de
//   "L". En cada salto, debe avanzar exactamente 2 parcelas en lÃ­nea recta (ya sea
//   horizontal o vertical) y luego exactamente 1 parcela en perpendicular a esa
//   direcciÃ³n. Desde una posiciÃ³n central interna, el dron tiene hasta 8 posibles
//   destinos de salto.
// 
//   Ejemplo de los 8 destinos desde una parcela central D = (fila 3, columna 3) en
//   un terreno de 7x7. Los asteriscos (*) marcan las parcelas alcanzables en un
//   Ãºnico salto:
// 
//         col: 0  1  2  3  4  5  6
//             -----------------------
//        f0 |  .  .  .  .  .  .  .
//        f1 |  .  .  *  .  *  .  .
//        f2 |  .  *  .  .  .  *  .
//        f3 |  .  .  .  D  .  .  .
//        f4 |  .  *  .  .  .  *  .
//        f5 |  .  .  *  .  *  .  .
//        f6 |  .  .  .  .  .  .  .
// 
//   Esos 8 destinos se obtienen combinando el avance recto (2) con el perpendicular (1).
//   Partiendo de D = (fila 3, columna 3), cada salto es:
// 
//      Avance recto VERTICAL (2 filas) + 1 columna perpendicular:
//        1) 2 arriba   + 1 a la izquierda  -> (1, 2)
//        2) 2 arriba   + 1 a la derecha    -> (1, 4)
//        3) 2 abajo    + 1 a la izquierda  -> (5, 2)
//        4) 2 abajo    + 1 a la derecha    -> (5, 4)
// 
//      Avance recto HORIZONTAL (2 columnas) + 1 fila perpendicular:
//        5) 1 arriba   + 2 a la izquierda  -> (2, 1)
//        6) 1 arriba   + 2 a la derecha    -> (2, 5)
//        7) 1 abajo    + 2 a la izquierda  -> (4, 1)
//        8) 1 abajo    + 2 a la derecha    -> (4, 5)
// 
//   Si el dron estuviera sobre un borde o una esquina, algunos de esos 8 destinos
//   caerÃ­an fuera del terreno y no estarÃ­an disponibles (de ahÃ­ el "hasta 8").
// 
// * RESTRICCIÃ“N DE BATERÃA: El dron no puede volver a pisar una parcela que ya
//   visitÃ³ en la misma simulaciÃ³n. Las parcelas ya pisadas quedan bloqueadas.
// 
// * ESTRATEGIA DE EXPLORACIÃ“N RECURSIVA: El algoritmo debe avanzar paso a paso de
//   forma recursiva. Si el dron se mete en un callejÃ³n sin salida (quedan parcelas
//   libres pero ninguna es alcanzable con su patrÃ³n de movimiento), el programa
//   debe deshacer el Ãºltimo paso intentado (Backtracking), marcar esa parcela
//   nuevamente como "no visitada" y evaluar la siguiente alternativa disponible.
// 
// * CONDICIÃ“N DE Ã‰XITO: La simulaciÃ³n es exitosa cuando el dron recorre TODAS las
//   parcelas ALCANZABLES desde su posiciÃ³n de despegue, cada una exactamente una
//   vez. Se entiende por "alcanzables" todas las parcelas a las que el dron puede
//   llegar encadenando saltos vÃ¡lidos de su patrÃ³n de movimiento (la "isla"
//   conectada que parte del despegue). Si esa isla tiene M parcelas, el recorrido
//   es exitoso al registrar el movimiento nÃºmero M - 1 (contando desde el paso
//   inicial 0). En terrenos amplios la isla abarca todo el mapa (M = N x N) y el
//   dron cubre el terreno completo; en terrenos chicos pueden quedar parcelas que
//   el patrÃ³n nunca puede pisar, y esas se excluyen del objetivo.
// 
// --------------------------------------------------------------------------------
// 2. REQUERIMIENTOS TÃ‰CNICOS OBLIGATORIOS
// --------------------------------------------------------------------------------
// 
// 1. TIPO DE PROYECTO: AplicaciÃ³n de Consola (.NET 8 o superior).
// 
// 2. ACCESO A DATOS (ADO.NET SÃNCRONO): Uso exclusivo del driver oficial de
//    PostgreSQL: "Npgsql". Se deberÃ¡n gestionar manualmente las conexiones
//    (NpgsqlConnection), comandos (NpgsqlCommand) y parÃ¡metros utilizando los
//    mÃ©todos bloqueantes estÃ¡ndar del proveedor. Queda terminantemente prohibido
//    el uso de ORMs (EF Core, Dapper, etc.) y de modificadores "async/await".
// 
// 3. CONFIGURACIÃ“N EXTERNA (PARAMETRIZACIÃ“N): Queda estrictamente prohibido
//    escribir la cadena de conexiÃ³n de la base de datos fija (hardcodeada) en el
//    cÃ³digo C#. Se debe implementar de forma obligatoria el uso de un archivo
//    "appsettings.json" junto con los paquetes de configuraciÃ³n de .NET
//    (Microsoft.Extensions.Configuration y Microsoft.Extensions.Configuration.Json)
//    para leer las credenciales del motor de manera dinÃ¡mica.
// 
// 4. RESTRICCIÃ“N DE SINTAXIS EN PERSISTENCIA: Para evaluar el control de flujos
//    lÃ³gicos tradicionales, queda prohibido el uso de bucles "for" o "foreach"
//    para recorrer e insertar la secuencia de movimientos en la base de datos.
//    Se debe utilizar una estructura de control alternativa ("while" o "do-while"),
//    donde el avance por la secuencia lo controla el programador con una variable
//    contador (Ã­ndice) que Ã©l mismo inicializa y va incrementando en cada vuelta,
//    en lugar de dejar que el bucle gestione el recorrido automÃ¡ticamente.
//    Por ejemplo:
//        int i = 0;                       // el Ã­ndice arranca en 0
//        while (i < cantidadMovimientos)  // la condiciÃ³n se evalÃºa a mano
//        {
//            InsertarMovimiento(secuencia[i]);
//            i++;                         // se avanza el Ã­ndice manualmente
//        }
// 
// 5. VALIDACIÃ“N DINÃMICA: El tamaÃ±o del terreno (N) y las coordenadas de inicio
//    deben ser validados en tiempo de ejecuciÃ³n antes de iniciar la recursiÃ³n.
//    N debe ser un entero >= 1, y cada coordenada de despegue debe caer en el
//    rango [0, N-1]. Nota: que las coordenadas sean vÃ¡lidas no garantiza que exista
//    recorrido; hay tamaÃ±os (p. ej. N = 4) en los que el dron alcanza todas las
//    parcelas pero no existe ruta que las cubra sin repetir, y el programa debe
//    informarlo como "sin soluciÃ³n" (no es un error).
// 
// --------------------------------------------------------------------------------
// 3. CONSIGNAS A DESARROLLAR
// --------------------------------------------------------------------------------
// 
// PARTE A: DISEÃ‘O E INTERPRETACIÃ“N DE LA BASE DE DATOS (15%)
// El alumno deberÃ¡ interpretar el siguiente requerimiento funcional y traducirlo
// a un diseÃ±o relacional fÃ­sico en PostgreSQL (creando los scripts DDL
// correspondientes). El sistema requiere modelar una relaciÃ³n Uno a Muchos entre:
// 
// 1. tb_master_control: Almacena la cabecera de cada ejecuciÃ³n exitosa. Debe
//    contener un identificador Ãºnico autonumÃ©rico entero (PK), una marca de tiempo
//    con la fecha del sistema, el tamaÃ±o del terreno N, y las coordenadas X e Y
//    de despegue.
// 2. tb_det_log: Almacena el rastro de la secuencia calculada de forma atÃ³mica.
//    Debe poseer su propio ID autonumÃ©rico entero (PK), un campo entero que actÃºe
//    como Llave ForÃ¡nea (FK) vinculada estrictamente a la estructura principal, un
//    campo entero para la etiqueta del paso actual (ver regla de la Parte D), y
//    dos campos enteros para registrar las coordenadas X e Y de la parcela pisada.
// 
// PARTE B: ALGORITMO DE VUELO RECURSIVO (40%)
// Implementa la lÃ³gica de resoluciÃ³n en una clase independiente:
// * El mÃ©todo recursivo no debe usar constantes para el tamaÃ±o del terreno; debe
//   recibir y adaptarse al tamaÃ±o N ingresado por el usuario.
// * Debe rellenar la matriz dinÃ¡mica de forma recursiva aplicando estrictamente
//   el patrÃ³n de desplazamiento 2x1 especificado y evaluando los caminos
//   alternativos cuando una ruta falle.
// * ORDEN DE EXPLORACIÃ“N OBLIGATORIO (heurÃ­stica de menor grado): en cada paso, los
//   destinos candidatos NO se prueban en un orden fijo, sino ordenados por su
//   "grado" ascendente, es decir, probando primero el destino que deja MENOS
//   salidas libres disponibles. Esta heurÃ­stica es OBLIGATORIA en toda entrega.
//   JustificaciÃ³n: con un orden fijo, el backtracking todavÃ­a resuelve terrenos
//   chicos (N <= 7) en milisegundos, pero se vuelve inviable a partir de N = 8 (un
//   8x8 no termina en tiempo razonable); exigirla siempre garantiza que la soluciÃ³n
//   funcione para cualquier N que se evalÃºe. El grado se explica en detalle en la
//   secciÃ³n "EJEMPLO DE REFERENCIA" al final.
// * Antes de la recursiÃ³n, el algoritmo debe determinar cuÃ¡ntas parcelas son
//   alcanzables desde el despegue (el objetivo a cubrir) recorriendo el grafo de
//   movimientos. Las parcelas no alcanzables quedan fuera del objetivo.
// * Nota: aunque el dron pueda alcanzar un conjunto de parcelas, no siempre existe
//   una ruta Ãºnica que las visite todas sin repetir (por ejemplo, en un terreno
//   4x4). El algoritmo debe ser capaz de retornar "false" si agota todas las
//   posibilidades de exploraciÃ³n sin lograr cubrir las parcelas alcanzables.
// 
// PARTE C: CONFIGURACIÃ“N E INFRAESTRUCTURA (10%)
// * Crear el archivo "appsettings.json" en la raÃ­z del proyecto, configurando
//   correctamente la secciÃ³n "ConnectionStrings". Asegurar que las propiedades
//   del archivo estÃ©n seteadas en "Copy to Output Directory = Copy if newer".
// * En el inicio de la aplicaciÃ³n (Program.cs), inicializar el ConfigurationBuilder
//   para extraer la cadena de conexiÃ³n antes de realizar cualquier interacciÃ³n
//   con PostgreSQL.
// 
// PARTE D: PERSISTENCIA DINÃMICA CON OFUSCACIÃ“N DE DATOS (20%)
// Una vez que la recursiÃ³n determine que el dron completÃ³ el recorrido, se debe
// invocar al mÃ©todo de guardado protegido por una transacciÃ³n (NpgsqlTransaction):
// 1. Insertar la cabecera en "tb_master_control" y recuperar el ID generado
//    automÃ¡ticamente utilizando la clÃ¡usula "RETURNING id" mediante el mÃ©todo
//    ".ExecuteScalar()".
// 2. Insertar los movimientos uno a uno en "tb_det_log" mediante un bucle "while"
//    controlado manualmente (respetando la restricciÃ³n de no usar for/foreach).
// 3. REGLA DE OFUSCACIÃ“N: Por motivos de auditorÃ­a de sistemas, el campo destinado
//    a registrar la etiqueta del paso actual NO debe guardarse de forma lineal.
//    DeberÃ¡s aplicar la siguiente lÃ³gica antes de ejecutar el ".ExecuteNonQuery()":
//    * Si el nÃºmero de movimiento actual de la simulaciÃ³n es PAR, se debe guardar
//      en la base de datos multiplicado por 2 (ej: el paso 4 se guarda como 8).
//    * Si el nÃºmero de movimiento actual es IMPAR, se debe guardar como un
//      NÃšMERO NEGATIVO (ej: el paso 3 se guarda como -3).
// 
// PARTE E: INTERFAZ DE CONSOLA Y REPORTE CON INGENIERÃA INVERSA (15%)
// El mÃ©todo Main debe gestionar el siguiente flujo dinÃ¡mico:
// 1. Cargar la configuraciÃ³n del sistema desde el archivo JSON.
// 2. Solicitar al usuario la dimensiÃ³n del espacio N (entero >= 1) y la posiciÃ³n
//    inicial de la unidad mÃ³vil, validando que las coordenadas se encuentren dentro
//    del rango vÃ¡lido [0, N-1].
// 3. Mostrar en la consola la matriz del recorrido calculada de forma numÃ©rica
//    ordinaria (0, 1, 2...). Las parcelas que el dron no haya podido pisar (no
//    alcanzables) se muestran con un punto ".". Si no hay soluciÃ³n, mostrar un
//    mensaje explicativo.
// 4. Si fue exitoso, guardar en PostgreSQL e informar el ID generado.
// 5. REPORTE INVERSO CON RECONSTRUCCIÃ“N: Realizar una consulta final utilizando
//    ".ExecuteReader()" (NpgsqlDataReader) para traer los ÃšLTIMOS 5 REGISTROS
//    vinculados a esa simulaciÃ³n especÃ­fica desde "tb_det_log", ordenados de forma
//    descendente por su ID (ORDER BY id DESC LIMIT 5). El programa deberÃ¡ aplicar
//    ingenierÃ­a inversa matemÃ¡tica "en caliente" mientras lee los registros para
//    revertir la transformaciÃ³n de la Parte D y mostrar el nÃºmero de paso REAL
//    (sin ofuscar) de cada uno de esos 5 registros. Es decir, se muestran los
//    Ãºltimos 5 pasos del recorrido ya reconstruidos a su valor original.
//    La regla de reconstrucciÃ³n (inversa de la Parte D) es: si el valor guardado es
//    NEGATIVO, el paso real era IMPAR y se recupera cambiÃ¡ndole el signo; si el
//    valor guardado es >= 0, el paso real era PAR y se recupera dividiÃ©ndolo por 2.
// 
// --------------------------------------------------------------------------------
// 4. CRITERIOS DE EVALUACIÃ“N ESTRICTOS
// --------------------------------------------------------------------------------
// * FIDELIDAD DEL MOVIMIENTO: El cÃ³digo debe implementar los vectores del patrÃ³n
//   2x1 especificado: 2 parcelas en recto + 1 perpendicular, con sus 8 destinos
//   posibles. Usar otro vector de desplazamiento se considerarÃ¡ desaprobado.
// * VALIDACIÃ“N DE LA SOLUCIÃ“N (NOTA PARA LA CORRECCIÃ“N): El recorrido NO es Ãºnico,
//   asÃ­ que la matriz del alumno puede no coincidir con la del ejemplo y aun asÃ­ ser
//   correcta. La correcciÃ³n debe validar PROPIEDADES, no la coincidencia exacta:
//   (a) cada parcela alcanzable se pisa exactamente una vez; (b) todo salto entre
//   pasos consecutivos respeta el patrÃ³n 2x1; (c) la cantidad de pasos coincide con
//   las parcelas alcanzables; (d) la ofuscaciÃ³n al guardar y la reconstrucciÃ³n al
//   leer son correctas.
// * DISEÃ‘O E INTEGRIDAD DE DATOS: Correcta interpretaciÃ³n de los nombres abstractos
//   de las tablas/columnas y su correspondiente vinculaciÃ³n por Clave ForÃ¡nea.
// * CUMPLIMIENTO DE RESTRICCIONES Y OFUSCACIÃ“N: No utilizaciÃ³n de bucles for/foreach
//   en la persistencia. Correcta implementaciÃ³n de la transformaciÃ³n de datos al
//   guardar (Parte D) y al recuperar/reconstruir (Parte E).
// * USO CORRECTO DE ADO.NET SÃNCRONO: Uso estricto de bloques "using" para
//   liberar recursos, parametrizaciÃ³n adecuada de consultas (evitar concatenaciÃ³n
//   de strings) y gestiÃ³n completa de transacciones (Commit/Rollback).
// * DEFENSA OBLIGATORIA: El docente se reserva el derecho de solicitar una
//   explicaciÃ³n sincrÃ³nica de lÃ­neas de cÃ³digo especÃ­ficas a los alumnos de
//   manera aleatoria para validar la autorÃ­a del examen.
// 
// --------------------------------------------------------------------------------
// EJEMPLO DE REFERENCIA: EJECUCIÃ“N PASO A PASO DEL ALGORITMO (N=8)
// --------------------------------------------------------------------------------
// Para ilustrar CÃ“MO ejecuta el algoritmo, simulamos un terreno de 8x8 (N=8) con
// despegue en la coordenada (0,0). En cada iteraciÃ³n el dron:
//   1) mira sus hasta 8 destinos posibles;
//   2) descarta los que caen fuera del terreno y los que YA PISÃ“ (quedan bloqueados);
//   3) entre los que siguen LIBRES prueba primero el de menor "grado" (ver abajo)
//      y avanza de forma recursiva.
// 
// Â¿QUÃ‰ SIGNIFICA EL "GRADO" DE UNA PARCELA CANDIDATA?
// El grado de una parcela candidata es la cantidad de parcelas LIBRES a las que el
// dron podrÃ­a saltar DESDE esa candidata en el siguiente movimiento (sus "salidas"
// disponibles). Se calcula contando solo las parcelas que en ese momento siguen sin
// pisar y dentro del terreno, por lo que el grado de una misma parcela va bajando a
// medida que sus vecinas se van bloqueando.
// 
//   - Grado alto  = la candidata tiene muchas salidas (es "cÃ³moda", flexible).
//   - Grado bajo  = la candidata tiene pocas salidas; una de grado 1 tiene una sola
//                   salida, y de grado 0 serÃ­a un callejÃ³n sin salida.
// 
// ESTRATEGIA (por quÃ© se elige el MENOR grado primero):
// Conviene visitar PRIMERO las parcelas mÃ¡s "encerradas" (las de menor grado), antes
// de que sus pocas salidas se bloqueen y queden aisladas para siempre. Si en cambio
// se dejaran para el final, es muy probable que terminen inalcanzables y el dron
// tenga que retroceder una y otra vez. Elegir el menor grado primero reduce
// muchÃ­simo el backtracking. Es Ãºnicamente el ORDEN en que se prueban los candidatos:
// si esa primera opciÃ³n luego falla, el backtracking igual prueba las demÃ¡s.
// (Ejemplo concreto: en el PASO 5 la esquina (7,7) tiene grado 1 â€”una Ãºnica salidaâ€”
// y por eso se elige antes que destinos de grado 5 Ã³ 7).
// 
// La etiqueta de cada parcela es su orden de pisada (0 = despegue) y el punto "."
// marca una parcela todavÃ­a libre. AsÃ­ se ve cÃ³mo el mapa se va bloqueando paso a
// paso (traza real generada por este proyecto):
// 
// PASO 0: el dron estÃ¡ en (0,0). Al ser una esquina, de sus 8 destinos teÃ³ricos
//   solo 2 caen dentro del terreno (de ahÃ­ el "hasta 8"):
//      -> ( 2, 1) LIBRE (grado 5)              <== ELEGIDO (menor grado)
//      -> ( 2,-1) FUERA DEL TERRENO (descartado: columna -1)
//      -> (-2, 1) FUERA DEL TERRENO (descartado: fila -2)
//      -> (-2,-1) FUERA DEL TERRENO (descartado: fila y columna negativas)
//      -> ( 1, 2) LIBRE (grado 5)
//      -> ( 1,-2) FUERA DEL TERRENO (descartado: columna -2)
//      -> (-1, 2) FUERA DEL TERRENO (descartado: fila -1)
//      -> (-1,-2) FUERA DEL TERRENO (descartado: fila y columna negativas)
//   Salta a (2,1) = 2 abajo + 1 derecha.
//   Terreno tras registrar el paso 0 ('.' = libre):
//         0   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
// 
// PASO 1: el dron estÃ¡ en (2,1).
//   Candidatos libres desde (2,1) [se descartan las ya pisadas]:
//      -> (4,2) LIBRE (grado 7)
//      -> (4,0) LIBRE (grado 3)   <== ELEGIDO (menor grado)
//      -> (0,2) LIBRE (grado 3)
//      -> (0,0) BLOQUEADA (ya pisada)
//      -> (3,3) LIBRE (grado 7)
//      -> (1,3) LIBRE (grado 5)
//   Salta a (4,0) = 2 abajo + 1 izquierda.
//   Terreno tras registrar el paso 1 ('.' = libre):
//         0   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   1   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
// 
// PASO 2: el dron estÃ¡ en (4,0).
//   Candidatos libres desde (4,0) [se descartan las ya pisadas]:
//      -> (6,1) LIBRE (grado 3)   <== ELEGIDO (menor grado)
//      -> (2,1) BLOQUEADA (ya pisada)
//      -> (5,2) LIBRE (grado 7)
//      -> (3,2) LIBRE (grado 7)
//   Salta a (6,1) = 2 abajo + 1 derecha.
//   Terreno tras registrar el paso 2 ('.' = libre):
//         0   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   1   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         2   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
// 
// PASO 3: el dron estÃ¡ en (6,1).
//   Candidatos libres desde (6,1) [se descartan las ya pisadas]:
//      -> (4,2) LIBRE (grado 6)
//      -> (4,0) BLOQUEADA (ya pisada)
//      -> (7,3) LIBRE (grado 3)   <== ELEGIDO (menor grado)
//      -> (5,3) LIBRE (grado 7)
//   Salta a (7,3) = 1 abajo + 2 derecha.
//   Terreno tras registrar el paso 3 ('.' = libre):
//         0   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   1   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         2   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   3   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
// 
// PASO 4: el dron estÃ¡ en (7,3).
//   Candidatos libres desde (7,3) [se descartan las ya pisadas]:
//      -> (5,4) LIBRE (grado 7)
//      -> (5,2) LIBRE (grado 6)
//      -> (6,5) LIBRE (grado 5)   <== ELEGIDO (menor grado)
//      -> (6,1) BLOQUEADA (ya pisada)
//   Salta a (6,5) = 1 arriba + 2 derecha.
//   Terreno tras registrar el paso 4 ('.' = libre):
//         0   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   1   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         2   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   3   .   .   .   .   .   .
//         .   .   .   4   .   .   .   .
// 
// PASO 5: el dron estÃ¡ en (6,5).
//   Candidatos libres desde (6,5) [se descartan las ya pisadas]:
//      -> (4,6) LIBRE (grado 5)
//      -> (4,4) LIBRE (grado 7)
//      -> (7,7) LIBRE (grado 1)   <== ELEGIDO (menor grado)
//      -> (7,3) BLOQUEADA (ya pisada)
//      -> (5,7) LIBRE (grado 3)
//      -> (5,3) LIBRE (grado 6)
//   Salta a (7,7) = 1 abajo + 2 derecha.
//   Terreno tras registrar el paso 5 ('.' = libre):
//         0   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   1   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         2   .   .   .   .   .   .   .
//         .   .   .   .   .   .   .   .
//         .   3   .   .   .   5   .   .
//         .   .   .   4   .   .   .   .
// 
// ... (el algoritmo sigue saltando y bloqueando parcelas) ...
// 
// Terreno al llegar al paso 15:
//         0   .  14   .   .   .  12   .
//        15   .   .   .  13   .   .   .
//         .   1   .   .   .   .   .  11
//         .   .   .   .   .   .   .   .
//         2   .   .   .   .   .  10   .
//         .   .   .   .   .   .   7   .
//         .   3   .   .   .   5   .   9
//         .   .   .   4   .   8   .   6
// 
// Terreno al llegar al paso 31:
//         0   .  14  31   .  27  12  29
//        15   .   .   .  13  30   .  26
//         .   1   .   .   .   .  28  11
//         .  16   .   .   .   .  25   .
//         2   .   .   .   .   .  10   .
//        17   .  19   .   .   .   7  24
//        20   3   .   .  22   5   .   9
//         .  18  21   4   .   8  23   6
// 
// Terreno al llegar al paso 47:
//         0   .  14  31   .  27  12  29
//        15  32   .   .  13  30   .  26
//         .   1   .  43   .   .  28  11
//        33  16   .   .   .  46  25   .
//         2   .  44  47  42   .  10  39
//        17  34  19   .  45  40   7  24
//        20   3  36  41  22   5  38   9
//        35  18  21   4  37   8  23   6
// 
// Terreno FINAL al llegar al paso 63 (terreno 8x8 cubierto por completo):
//         0  49  14  31  60  27  12  29
//        15  32  63  54  13  30  59  26
//        50   1  48  43  56  61  28  11
//        33  16  55  62  53  46  25  58
//         2  51  44  47  42  57  10  39
//        17  34  19  52  45  40   7  24
//        20   3  36  41  22   5  38   9
//        35  18  21   4  37   8  23   6
// 
// *(Nota sobre el BACKTRACKING: si en alguna iteraciÃ³n el dron se quedara SIN
// candidatos libres antes de completar el recorrido, el algoritmo deshace el Ãºltimo
// paso â€”vuelve a marcar esa parcela como libre "."â€” y prueba la siguiente
// alternativa de la parcela anterior. En esta simulaciÃ³n, elegir siempre el destino
// de menor grado evitÃ³ los callejones, por lo que llegÃ³ al paso 63 sin retroceder.
// El recorrido no es Ãºnico: otra elecciÃ³n de candidatos darÃ­a otra matriz igualmente
// vÃ¡lida. El programa dibuja esta matriz numÃ©rica en pantalla antes de procesar el
// guardado con ofuscaciÃ³n en PostgreSQL).*
// 
// --------------------------------------------------------------------------------
// MATRICES DE EJEMPLO PARA DISTINTOS TAMAÃ‘OS DE N
// --------------------------------------------------------------------------------
// Resultados reales generados por este proyecto (0 = parcela de despegue; los
// nÃºmeros indican el orden de pisada).
// 
// N = 6, despegue (0,0) -> terreno completo (36 parcelas, pasos 0..35):
//     0    9   30   19    6   11
//    31   18    7   10   29   20
//     8    1   24   35   12    5
//    17   32   15   26   21   28
//     2   25   34   23    4   13
//    33   16    3   14   27   22
// 
// N = 7, despegue (5,3) -> terreno completo (49 parcelas, pasos 0..48):
//    20    9   30   39   18    7    4
//    29   40   19    8    5   36   17
//    10   21   38   31   44    3    6
//    41   28   43   48   37   16   35
//    22   11   24   27   32   45    2
//    25   42   13    0   47   34   15
//    12   23   26   33   14    1   46
// 
// --------------------------------------------------------------------------------
// CASOS ESPERADOS (VERIFICACIÃ“N RÃPIDA)
// --------------------------------------------------------------------------------
// Entradas (N, X, Y) y el resultado que debe producir el programa:
// 
//   N = 1, (0,0)  -> Ã‰XITO: cubre 1 de 1 parcela (solo el paso 0).
//   N = 2, (0,0)  -> Ã‰XITO: cubre 1 de 4 (no entra ningÃºn salto; el resto queda ".").
//   N = 3, (0,0)  -> Ã‰XITO: cubre 8 de 9 (la parcela central es inalcanzable):
//         0   5   2
//         3   .   7
//         6   1   4
//   N = 4, (0,0)  -> SIN SOLUCIÃ“N: alcanza las 16 parcelas, pero no existe ruta que
//                    las cubra sin repetir; el programa debe informarlo (no es error).
//   N = 6, (0,0) y N = 7, (5,3) -> Ã‰XITO: terreno completo (ver matrices arriba).
// 
// Recordatorio: por empates de "grado", la matriz concreta puede variar entre
// soluciones correctas; se validan las PROPIEDADES (ver criterios de evaluaciÃ³n),
// no la coincidencia exacta con estas matrices.
// 
// --------------------------------------------------------------------------------
// 5. SISTEMA DE CALIFICACIÃ“N Y APROBACIÃ“N
// --------------------------------------------------------------------------------
// 
// DISTRIBUCIÃ“N DEL PUNTAJE (sobre 100 puntos):
// 
//   Parte A - DiseÃ±o e interpretaciÃ³n de la base de datos ......... 15
//   Parte B - Algoritmo de vuelo recursivo ....................... 40
//   Parte C - ConfiguraciÃ³n e infraestructura .................... 10
//   Parte D - Persistencia dinÃ¡mica con ofuscaciÃ³n ............... 20
//   Parte E - Interfaz de consola y reporte con ing. inversa ..... 15
//                                                           TOTAL = 100
// 
// Cada parte se corrige con PUNTAJE PARCIAL segÃºn las propiedades cumplidas (no es
// "todo o nada"). Detalle orientativo de lo que se valora en cada una:
// 
//   A (20): tablas con PK autonumÃ©rica, tipos correctos, FK bien vinculada (1 a N),
//           nombres de columnas coherentes con su funciÃ³n.
//   B (25): movimiento 2x1 fiel (8 destinos), recursiÃ³n + backtracking correcto,
//           orden por menor grado, pre-cuenta de alcanzables, retorno "false" al no
//           haber ruta, adaptaciÃ³n dinÃ¡mica a N (sin constantes).
//   C (15): appsettings.json con ConnectionStrings, "Copy if newer",
//           ConfigurationBuilder leyendo la cadena antes de tocar la base.
//   D (25): transacciÃ³n (Commit/Rollback), RETURNING id con ExecuteScalar, inserciÃ³n
//           con while/Ã­ndice manual, ofuscaciÃ³n correcta (par x2 / impar negativo).
//   E (15): validaciÃ³n de N y coordenadas, dibujo de la matriz, ExecuteReader de los
//           Ãºltimos 5 (ORDER BY id DESC) y reconstrucciÃ³n inversa correcta.
// 
// CONDICIÃ“N DE APROBACIÃ“N:
//   Se aprueba con 60 puntos o mÃ¡s (60/100), siempre que NO se incurra en ninguna
//   condiciÃ³n eliminatoria. AdemÃ¡s, la aprobaciÃ³n queda SUJETA a una defensa
//   INDIVIDUAL y OBLIGATORIA del cÃ³digo realizado, que se llevarÃ¡ a cabo el dÃ­a
//   LUNES. En esa instancia el alumno deberÃ¡ explicar su propio cÃ³digo; si no logra
//   justificar la autorÃ­a o no se presenta a la defensa, el examen se considera
//   desaprobado sin importar el puntaje obtenido.
// 
// CONDICIONES ELIMINATORIAS (desaprueban el examen sin importar el puntaje logrado):
//   - Implementar un vector de movimiento distinto del patrÃ³n 2x1 especificado.
//   - Usar un ORM (EF Core, Dapper, etc.) o modificadores async/await en el acceso
//     a datos (se exige ADO.NET sÃ­ncrono).
//   - No poder explicar el propio cÃ³digo en la defensa (autorÃ­a no validada).
//   - Entregar un proyecto que no compila.
// 
// PENALIZACIONES (restan dentro de la parte correspondiente, no eliminan):
//   - Cadena de conexiÃ³n hardcodeada en vez de leerla del JSON (resta en C).
//   - Uso de "for"/"foreach" para recorrer e insertar la secuencia (resta en D).
//   - Concatenar strings en las consultas en lugar de parametrizar (resta en D/E).
//   - No liberar recursos con bloques "using" o no gestionar Commit/Rollback (resta en D).
// 
// NOTA DE CORRECCIÃ“N: como el recorrido no es Ãºnico, la correcciÃ³n de la Parte B/E
// se hace por PROPIEDADES (recorrido vÃ¡lido, cobertura de alcanzables, ofuscaciÃ³n y
// reconstrucciÃ³n correctas), nunca por coincidencia exacta con las matrices de
// ejemplo de este enunciado.
// 
// --------------------------------------------------------------------------------
// ENTREGA DEL PARCIAL
// --------------------------------------------------------------------------------
// * TODOS los alumnos deberÃ¡n ENVIAR el parcial para tener derecho a recuperaciÃ³n.
// * Fecha y hora lÃ­mite de envÃ­o: HOY, 24 de junio, hasta las 22:00 hs.
// * El alumno que no llegue a terminar debe enviarlo igualmente en el estado en que
//   se encuentre (entrega parcial).
// * El cÃ³digo debe subirse a un repositorio de GitHub y notificarse vÃ­a email.
// 
// ================================================================================
// FIN DEL DOCUMENTO
// ================================================================================


using System.Text;
using Microsoft.Extensions.Configuration;
using ParcialGonzalezJose.Data;
using ParcialGonzalezJose.Models;
using ParcialGonzalezJose.Services;

namespace ParcialGonzalezJose;

internal static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        IConfiguration configuration;

        try
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();
        }
        catch (Exception exception)
        {
            Console.WriteLine("No se pudo cargar appsettings.json.");
            Console.WriteLine(exception.Message);
            return;
        }

        string? connectionString = configuration.GetConnectionString("PostgreSql");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("La cadena ConnectionStrings:PostgreSql no esta configurada.");
            return;
        }

        Console.WriteLine("Simulador de trayectoria de dron automatizado");
        Console.WriteLine();

        int size = ReadInteger(
            "Ingrese la dimension del terreno N (entero >= 1): ",
            value => value >= 1,
            "N debe ser un entero mayor o igual a 1.");

        int startX = ReadInteger(
            $"Ingrese X / fila de despegue [0, {size - 1}]: ",
            value => value >= 0 && value < size,
            $"X debe estar en el rango [0, {size - 1}].");

        int startY = ReadInteger(
            $"Ingrese Y / columna de despegue [0, {size - 1}]: ",
            value => value >= 0 && value < size,
            $"Y debe estar en el rango [0, {size - 1}].");

        var solver = new DroneRouteSolver();
        DroneRouteResult result = solver.Resolve(size, new Coordinate(startX, startY));

        Console.WriteLine();
        Console.WriteLine($"Parcelas alcanzables desde el despegue: {result.ReachableCount} de {size * size}.");

        if (!result.Success)
        {
            Console.WriteLine("Sin solucion: el dron alcanza esas parcelas, pero no existe una ruta que las pise todas exactamente una vez sin repetir.");
            return;
        }

        PrintBoard(result.Board, result.Reachable);

        var repository = new DroneRunRepository(connectionString);

        try
        {
            int masterId = repository.SaveSuccessfulRun(size, startX, startY, result.Steps);

            Console.WriteLine();
            Console.WriteLine($"Recorrido guardado correctamente. ID generado: {masterId}");

            IReadOnlyList<PersistedStepReport> report = repository.GetLastFiveReconstructedSteps(masterId);
            PrintReconstructedReport(report);
        }
        catch (Exception exception)
        {
            Console.WriteLine();
            Console.WriteLine("El recorrido fue calculado, pero hubo un problema al guardar o consultar PostgreSQL.");
            Console.WriteLine(exception.Message);
        }
    }

    private static int ReadInteger(string prompt, Func<int, bool> isValid, string errorMessage)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (input is null)
            {
                Console.WriteLine();
                Console.WriteLine("No se recibio entrada por consola. Ejecute dotnet run desde una terminal interactiva.");
                Environment.Exit(1);
                return 0;
            }

            if (int.TryParse(input, out int value) && isValid(value))
            {
                return value;
            }

            Console.WriteLine(errorMessage);
        }
    }

    private static void PrintBoard(int[,] board, bool[,] reachable)
    {
        int size = board.GetLength(0);
        int cellWidth = Math.Max(2, ((size * size) - 1).ToString().Length + 1);

        Console.WriteLine();
        Console.WriteLine("Matriz del recorrido:");

        int x = 0;

        while (x < size)
        {
            int y = 0;

            while (y < size)
            {
                if (!reachable[x, y] || board[x, y] < 0)
                {
                    Console.Write(".".PadLeft(cellWidth));
                }
                else
                {
                    Console.Write(board[x, y].ToString().PadLeft(cellWidth));
                }

                y++;
            }

            Console.WriteLine();
            x++;
        }
    }

    private static void PrintReconstructedReport(IReadOnlyList<PersistedStepReport> report)
    {
        Console.WriteLine();
        Console.WriteLine("Ultimos 5 movimientos reconstruidos desde tb_det_log:");

        if (report.Count == 0)
        {
            Console.WriteLine("No se encontraron movimientos para esta simulacion.");
            return;
        }

        int i = 0;

        while (i < report.Count)
        {
            PersistedStepReport item = report[i];
            Console.WriteLine(
                $"Log ID {item.LogId}: paso real {item.RealStep} en ({item.X},{item.Y}) - valor guardado {item.StoredStep}");
            i++;
        }
    }
}
