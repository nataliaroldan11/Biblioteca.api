# Biblioteca API

API REST de una biblioteca desarrollada como prueba técnica junior. Permite crear, consultar, actualizar y eliminar libros, registrar usuarios e iniciar sesión con JWT. Las consultas de libros son públicas; las operaciones de escritura requieren autenticación.

## Tecnologías y organización

- C# y ASP.NET Core sobre .NET 10.
- Entity Framework Core 10 y SQL Server para persistencia.
- ASP.NET Core Identity para usuarios y hashes de contraseñas.
- JWT firmado con HS256 para autenticar solicitudes.
- xUnit, WebApplicationFactory y SQLite temporal para las pruebas.
- Postman para ejecutar el flujo completo de la API.

La solución aplica una separación por capas inspirada en Clean Architecture:

| Proyecto | Responsabilidad |
| --- | --- |
| Biblioteca.Domain | Entidad Book y contrato IBookRepository. |
| Biblioteca.Application | Casos de uso de libros y autenticación, DTOs y contratos IIdentityService e ITokenService. |
| Biblioteca.Infrastructure | Implementaciones con EF Core, contexto, migraciones, Identity y emisión de JWT. |
| Biblioteca.Api | Controladores HTTP, configuración y registro de dependencias. |
| Biblioteca.Application.Tests | Pruebas unitarias de creación de libros, registro y login. |
| Biblioteca.Api.Tests | Pruebas de integración HTTP con una base temporal. |

Application depende de Domain. Infrastructure depende de Domain y Application porque implementa sus contratos. Api conecta los servicios. Domain no depende de EF Core, Identity ni ASP.NET Core.

Ejemplo del recorrido de una solicitud para crear un libro:

```text
Postman -> validación del JWT -> BooksController -> CreateBookUseCase
        -> IBookRepository / BookRepository -> ApplicationDbContext -> SQL Server
```

## Requisitos

- SDK de .NET 10 y Visual Studio con soporte para ese SDK y desarrollo de ASP.NET.
- Una instancia de SQL Server disponible y una cuenta con permisos para aplicar las migraciones en una base de desarrollo.
- Postman para las comprobaciones manuales.

Los paquetes NuGet se restauran al compilar la solución. SQL Server debe instalarse o iniciarse por separado; este repositorio no incluye un servidor ni un archivo Docker Compose.

## Configuración y ejecución en Visual Studio

### 1. Abrir la solución

Abre `Bibliotecata.slnx` y establece `Biblioteca.Api` como proyecto de inicio. El nombre del archivo de solución conserva el nombre original del proyecto.

### 2. Configurar los secretos locales

En Biblioteca.Api, abre **Administrar secretos de usuario** desde el menú contextual. Para una instalación nueva, completa esta plantilla con tus propios datos:

```json
{
  "ConnectionStrings:Biblioteca": "Server=localhost,1433;Database=BibliotecaDb;User Id=USUARIO_SQL;Password=TU_PASSWORD_SQL;Encrypt=True;TrustServerCertificate=True;",
  "Jwt:Key": "REEMPLAZAR_CLAVE"
}
```

La dirección de SQL Server debe coincidir con tu instancia. El ejemplo utiliza autenticación SQL; sustituye usuario y contraseña. `TrustServerCertificate=True` corresponde al entorno local de desarrollo.

Sustituye `REEMPLAZAR_CLAVE` por una clave aleatoria de al menos 32 bytes. Esta clave pertenece a la API; las contraseñas de los usuarios se eligen al registrarse. El siguiente comando opcional genera 64 bytes aleatorios en Base64 desde PowerShell o la consola del administrador de paquetes, también en versiones que no ofrecen el método estático `Fill`:

```powershell
$libraryJwtBytes = New-Object byte[] 64
$libraryJwtGenerator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$libraryJwtGenerator.GetBytes($libraryJwtBytes)
$libraryJwtGenerator.Dispose()
[Convert]::ToBase64String($libraryJwtBytes)
```

Copia el resultado únicamente al valor de `Jwt:Key` en los secretos locales. Si ya tienes los secretos configurados en tu equipo, conserva esos valores.

El proyecto incluye un UserSecretsId y lee User Secrets en Development. Estos valores se guardan fuera del repositorio. User Secrets está destinado al desarrollo y no cifra su archivo. La configuración pública del JWT está en `Biblioteca.Api/appsettings.Development.json`: emisor, audiencia y duración de 30 minutos.

### 3. Aplicar las migraciones

Con SQL Server iniciado, abre en Visual Studio **Herramientas > Administrador de paquetes NuGet > Consola del Administrador de paquetes** y ejecuta:

```powershell
Update-Database -Project Biblioteca.Infrastructure -StartupProject Biblioteca.Api -Args '--environment Development'
```

Este comando aplica las migraciones pendientes: `initialcreate` para Books y `AddIdentityUsers` para las tablas de Identity. En una base ya actualizada no crea de nuevo las tablas. No necesitas generar otra migración para instalar el proyecto. Referencia: [herramientas de EF Core en Visual Studio](https://learn.microsoft.com/en-us/ef/core/cli/powershell).

### 4. Iniciar la API

Selecciona el perfil **https** y pulsa F5. La consola debe mostrar `https://localhost:7149` y `http://localhost:5149`. El perfil no abre el navegador automáticamente.

Para un equipo nuevo, se puede confiar en el certificado HTTPS de desarrollo con `dotnet dev-certs https --trust`, aceptando la confirmación de Windows. La API se prueba mediante sus rutas; la raíz `/` no contiene una página de inicio.

## Endpoints

| Método | Ruta | Acceso | Respuestas principales |
| --- | --- | --- | --- |
| POST | /api/auth/register | Público | 201; 400 por datos inválidos o correo duplicado. |
| POST | /api/auth/login | Público | 200 con JWT; 401 por credenciales incorrectas o bloqueo. |
| GET | /api/books | Público | 200 con una lista, que puede estar vacía. |
| GET | /api/books/{id} | Público | 200; 404 si no existe. |
| POST | /api/books | JWT | 201 con id y cabecera Location; 400; 401. |
| PUT | /api/books/{id} | JWT | 204; 400; 401; 404. |
| DELETE | /api/books/{id} | JWT | 204; 401; 404. |

Registro y login reciben un objeto JSON con `email` y `password`. La contraseña de registro debe tener al menos ocho caracteres, incluyendo mayúscula, minúscula, número y símbolo, según la configuración de Identity utilizada. Login devuelve `accessToken`, `expiresAtUtc` y `tokenType`.

POST y PUT de libros reciben:

```json
{
  "title": "Cien años de soledad",
  "author": "Gabriel García Márquez",
  "isbn": "9788497592208",
  "publicationYear": 1967
}
```

Envía `Content-Type: application/json`. El identificador se genera en SQL Server; para consultar, actualizar o eliminar utiliza el id recibido al crear el libro.

## Casos de uso de autenticación

`AuthController` recibe las solicitudes HTTP y ejecuta `RegisterUserUseCase` o `LoginUseCase`, situados en `Biblioteca.Application/UseCases/Authentication`.

- `RegisterUserUseCase` normaliza el correo y solicita el registro mediante `IIdentityService`. Conserva la contraseña recibida y propaga los errores del registro.
- `LoginUseCase` solicita autenticar las credenciales mediante `IIdentityService`. Si recibe una identidad válida, pide un token a `ITokenService`; si recibe null, no emite ningún token.
- `IdentityAuthService`, en Infrastructure, implementa `IIdentityService` con `UserManager<IdentityUser>`. Conserva la validación, el hash y el bloqueo de Identity. Devuelve `AuthenticatedUser`, un tipo propio de Application.
- `JwtTokenService`, en Infrastructure, implementa `ITokenService` y crea el JWT a partir de esa identidad. No depende de `IdentityUser`.

Application coordina las operaciones mediante interfaces y no depende de Identity ni de la biblioteca JWT. El contrato anterior IAuthService fue sustituido por estos dos contratos. No hay cambios de esquema de base de datos ni de los endpoints. Las cuatro pruebas unitarias de autenticación comprueban normalización, errores de registro, rechazo sin emisión de token y emisión para la identidad autenticada.

## Autenticación y Postman

Importa `postman/Biblioteca-JWT.postman_collection.json`. La colección se llama **Biblioteca - JWT y CRUD** y utiliza `https://localhost:7149` como baseUrl.

1. Ejecuta **01 Registrar usuario**: genera un correo de prueba y usa una contraseña de demostración definida en las variables de la colección.
2. Ejecuta **02 Login y guardar JWT**: guarda automáticamente el token en `accessToken`.
3. Ejecuta las peticiones 03 a 12 en orden. Verifican acceso sin token, creación, consulta, actualización, validación, eliminación y contraseña incorrecta.
4. Revisa **Test Results** en cada petición. Los códigos 400, 401 y 404 son resultados esperados en los casos que intentan provocar esos errores.

Para usar tu propio usuario, escribe el mismo correo y contraseña directamente en los cuerpos de registro y login. Escribe el correo sin llaves. `{{demoEmail}}` representa una variable de Postman, no la forma de escribir un correo literal. Para que la última prueba compruebe la contraseña incorrecta de tu cuenta, utiliza también tu correo en la petición 12.

Las peticiones protegidas envían `Authorization: Bearer {{accessToken}}`. Si haces una petición manual, elige Authorization > Bearer Token y pega el accessToken. Al vencer el token, repite el login. El flujo de la colección elimina el libro que acaba de crear; la cuenta de prueba permanece en la base de datos.

Identity guarda hashes de contraseñas y bloquea el acceso durante cinco minutos después de cinco contraseñas incorrectas para una cuenta existente. La API valida firma, emisor, audiencia y vencimiento del JWT, con una tolerancia de 30 segundos. Todos los usuarios registrados pueden modificar cualquier libro; no hay roles ni permisos por propietario.

## Pruebas automatizadas

En Visual Studio abre **Prueba > Explorador de pruebas > Ejecutar todas las pruebas**. También puedes ejecutar desde la carpeta de la solución:

```text
dotnet test Bibliotecata.slnx
```

La suite contiene 21 casos: siete unitarios en Application.Tests y catorce de integración en Api.Tests. Las pruebas unitarias utilizan un repositorio de prueba. Las de integración levantan la API y utilizan SQLite en memoria con configuración JWT exclusiva de pruebas; no necesitan iniciar manualmente la API ni SQL Server.

Se comprueba la creación válida y el título vacío; registro y hash persistido; correo duplicado; credenciales inválidas; bloqueo temporal; consultas públicas; operaciones protegidas; JWT vencido o con firma, emisor o audiencia incorrectos; y el recorrido de creación, actualización y eliminación con JWT.

SQLite permite comprobar la integración HTTP con persistencia relacional temporal. Las migraciones específicas de SQL Server se comprueban además al aplicar las migraciones y ejecutar la colección contra SQL Server.

## Alcance actual y mejoras posibles

- El CRUD, la persistencia, el registro y la autenticación están implementados. Es una API; no incluye interfaz web.
- La validación de libros comprueba que el título no esté vacío. No se ha implementado unicidad ni validación del formato de ISBN, límites de longitud, paginación o validaciones completas de autor y año.
- No hay renovación de tokens, recuperación de contraseñas, confirmación de correo ni roles.
- La plantilla WeatherForecast todavía está en la solución. OpenAPI está desactivado en la configuración actual; la colección de Postman documenta y ejecuta las peticiones.
- Las dos advertencias CS8981 corresponden al nombre en minúsculas de la migración existente `initialcreate` y su archivo generado.

## Entrega y control de versiones

Incluye los proyectos, las migraciones, este README y la colección de Postman. `.gitignore` excluye los resultados de compilación, los archivos de Visual Studio y los secretos locales.

Cada persona que ejecute el proyecto debe configurar sus propios secretos y aplicar las migraciones. El repositorio no contiene las credenciales, los usuarios ni los registros de libros de la base local del autor.
