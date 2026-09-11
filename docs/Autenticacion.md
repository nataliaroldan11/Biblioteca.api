# Biblioteca: registro, JWT y endpoints protegidos

## Qué se agregó

- POST /api/auth/register: recibe email y password. Identity valida y almacena el usuario con un hash de contraseña. Devuelve 201 y el identificador. Datos inválidos o correo duplicado devuelven 400.
- POST /api/auth/login: devuelve accessToken, expiresAtUtc y tokenType. Credenciales incorrectas o cuenta bloqueada devuelven 401.
- POST, PUT y DELETE de /api/books requieren Authorization: Bearer <token>.
- Los GET de libros siguen siendo públicos.
- Los JWT se firman con HS256 y vencen en 30 minutos. Se comprueban emisor, destinatario, firma, algoritmo y vencimiento, con 30 segundos de tolerancia.
- Tras 5 contraseñas incorrectas, Identity bloquea el login de esa cuenta durante 5 minutos.

Para esta prueba básica, cualquier usuario registrado puede modificar cualquier libro. No se agregaron roles ni permisos por propietario. La contraseña no va en el JWT; un JWT firmado no es un contenedor para secretos.

## Archivos que debes entender para sustentarlo

1. Application/Authentication: DTOs y contrato IAuthService. Esta capa no depende de Identity.
2. Infrastructure/Authentication/IdentityAuthService: usa UserManager para registrar usuarios, comprobar contraseñas y controlar intentos fallidos.
3. Infrastructure/Authentication/JwtTokenService: emite el token con identificador, correo, identificador del token y vencimiento.
4. Api/Authentication/AuthenticationConfiguration: registra la autenticación y establece qué tokens acepta.
5. Api/Controllers/AuthController: traduce el resultado del servicio a respuestas HTTP.
6. Api/Controllers/BooksController: [Authorize] en las tres operaciones de escritura.
7. Program.cs: conecta los servicios y llama UseAuthentication antes de UseAuthorization.

Infrastructure ahora referencia Application porque implementa IAuthService. Domain sigue independiente.

## Configuración local

La API tiene un UserSecretsId. En Visual Studio, Biblioteca.Api → Administrar secretos de usuario contiene:

- Jwt:Key: clave aleatoria generada localmente. No debe copiarse a Postman ni compartirse.
- ConnectionStrings:Biblioteca: conexión existente a SQL Server, trasladada fuera del proyecto.

appsettings.Development.json contiene solamente configuración no secreta: registros, emisor, audiencia y duración del JWT. User Secrets es para desarrollo y no cifra sus archivos. Un entorno desplegado debe proporcionar estos valores mediante su configuración de secretos.

## Prueba con Postman

1. Inicia SQL Server y Biblioteca.Api con el perfil https. La dirección configurada es https://localhost:7149.
2. Importa Biblioteca-JWT.postman_collection.json. Es una colección nueva llamada Biblioteca - JWT y CRUD.
3. Ejecuta 01 Registrar usuario y 02 Login y guardar JWT. La colección genera un correo de prueba único y usa Biblioteca-Demo!2026 como contraseña de demostración.
4. Ejecuta las siguientes peticiones en orden. El JWT y el identificador del libro se guardan automáticamente como variables de colección.
5. El resultado esperado se incluye en el nombre o en Test Results. 401 sin token, 400 con título inválido y 404 después de eliminar son respuestas correctas.
6. El flujo elimina únicamente el libro que creó. La cuenta de demostración permanece en la base; cada ejecución de registro crea otra cuenta.

Para enviar peticiones manualmente: Authorization → Bearer Token → pega solo accessToken. No pegues Jwt:Key. Al vencer el token, vuelve a ejecutar Login.

Las colecciones anteriores necesitan agregar el token a POST, PUT y DELETE. No es un fallo del CRUD si ahora responden 401 sin autenticar.

## Pruebas automatizadas

Ejecuta dotnet test Bibliotecata.slnx, o Ejecutar todas en Visual Studio.

- Biblioteca.Application.Tests: los 3 casos de creación existentes.
- Biblioteca.Api.Tests: 14 casos de integración HTTP con SQLite temporal; validan registro, hash persistido, duplicados, credenciales inválidas, bloqueo, JWT inválidos, lecturas públicas y CRUD con JWT.

Las pruebas de integración no usan ni borran BibliotecaDb. Verifican el contrato HTTP y el comportamiento con una base relacional temporal; no sustituyen la comprobación de las migraciones específicas de SQL Server.

## Alcance pendiente de la prueba completa

El registro y la autenticación no completan por sí solos la entrega: todavía debemos revisar reglas como ISBN único, mejorar la documentación global y preparar el historial Git. Los endpoints del ejemplo WeatherForecast aún pertenecen a la plantilla y se pueden retirar al preparar la entrega.
