# 🎓 Sistema de Gestión Académica

Sistema web desarrollado en ASP.NET para la gestión académica de estudiantes, permitiendo la visualización de notas, materias y promedio acumulado (CUM).

## 📋 Descripción

Este proyecto es un sistema de gestión académica que permite a los estudiantes consultar su información académica de manera centralizada. El sistema cuenta con diferentes roles de usuario (Administrador, Docente y Estudiante), donde cada uno tiene permisos específicos según su función.

## ✨ Características Principales

- **Vista de Estudiante**: Los estudiantes pueden visualizar de manera segura:
  - Notas por materia
  - Materias inscritas
  - Promedio acumulado (CUM)
  - Historial académico

- **Panel de Docente**: Los docentes tienen la capacidad de:
  - Registrar y modificar notas de los estudiantes
  - Visualizar listado de estudiantes por materia
  - Gestionar calificaciones

- **Panel de Administrador**: Los administradores pueden:
  - Gestionar usuarios (estudiantes y docentes)
  - Administrar materias
  - Configurar periodos académicos
  - Generar reportes

## 🛠️ Tecnologías Utilizadas

- **Backend**: ASP.NET (Framework .NET)
- **Base de Datos**: MySQL
- **Frontend**: HTML, CSS, JavaScript
- **Lenguaje**: C#

## 📦 Requisitos Previos

Antes de ejecutar el proyecto, asegúrate de tener instalado:

- [.NET SDK](https://dotnet.microsoft.com/download) (versión compatible con tu proyecto)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/)
- Visual Studio 2019 o superior (recomendado) o Visual Studio Code

## 🚀 Instalación

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/m0nge/GestionAcademica.git
   cd GestionAcademica
   ```

2. **Configurar la base de datos**
   - Abre MySQL Workbench o tu gestor de base de datos preferido
   - Ejecuta el script SQL ubicado en `final.sql` para crear la base de datos y las tablas necesarias
   ```bash
   mysql -u tu_usuario -p < final.sql
   ```

3. **Configurar la cadena de conexión**
   - Abre el archivo `GestionAcademica.sln` en Visual Studio
   - Edita el archivo de configuración (Web.config o appsettings.json) con tus credenciales de MySQL:
   ```xml
   <connectionStrings>
     <add name="GestionAcademicaDB" 
          connectionString="Server=localhost;Database=gestionacademica;Uid=tu_usuario;Pwd=tu_contraseña;" 
          providerName="MySql.Data.MySqlClient" />
   </connectionStrings>
   ```

4. **Restaurar dependencias**
   ```bash
   dotnet restore
   ```

5. **Ejecutar el proyecto**
   ```bash
   dotnet run
   ```
   O presiona `F5` en Visual Studio para iniciar en modo debug.

## 👥 Roles y Permisos

### 🎓 Estudiante
- **Permisos**: Solo lectura
- **Acceso a**:
  - Visualización de notas
  - Consulta de materias inscritas
  - Visualización del CUM (Promedio acumulado)

### 👨‍🏫 Docente
- **Permisos**: Lectura y escritura de notas
- **Acceso a**:
  - Todo lo que puede ver un estudiante
  - Registro y edición de calificaciones
  - Gestión de asistencias

### 👨‍💼 Administrador
- **Permisos**: Control total del sistema
- **Acceso a**:
  - Gestión completa de usuarios
  - Administración de materias y periodos
  - Configuración del sistema
  - Generación de reportes

## 📁 Estructura del Proyecto

```
GestionAcademica/
├── .vs/                    # Configuración de Visual Studio
├── GestionAcademica/       # Proyecto principal ASP.NET
│   ├── Controllers/        # Controladores MVC
│   ├── Models/            # Modelos de datos
│   ├── Views/             # Vistas de la aplicación
│   └── Scripts/           # Scripts JavaScript
├── GestionAcademica.sln   # Solución de Visual Studio
└── final.sql              # Script de base de datos
```

## 🗄️ Base de Datos

El sistema utiliza MySQL como gestor de base de datos. El archivo `final.sql` contiene:
- Creación de tablas (Estudiantes, Docentes, Materias, Notas, etc.)
- Relaciones y llaves foráneas
- Procedimientos almacenados (si aplica)
- Datos de prueba (opcional)

## 🔐 Seguridad

- Autenticación mediante sistema de login
- Autorización basada en roles
- Los estudiantes solo tienen acceso de lectura a su información personal
- Validación de sesiones activas
- Protección contra inyección SQL mediante consultas parametrizadas

## 🤝 Contribuciones

Las contribuciones son bienvenidas. Por favor:
1. Haz un Fork del proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📝 Licencia

Este proyecto es de código abierto y está disponible bajo la [Licencia MIT](LICENSE).

## 👨‍💻 Autor

**m0nge**
- GitHub: [@m0nge](https://github.com/m0nge)

## 📞 Soporte

Si tienes alguna pregunta o problema, por favor abre un [issue](https://github.com/m0nge/GestionAcademica/issues) en el repositorio.

---

⭐ Si este proyecto te fue útil, no olvides darle una estrella en GitHub!
