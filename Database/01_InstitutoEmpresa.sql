/* Base SQL Server para la versión Blazor.
   Si ya tenés las tablas creadas, compará nombres/tipos antes de ejecutar.
*/
IF DB_ID(N'InstitutoEmpresa') IS NULL
    CREATE DATABASE InstitutoEmpresa;
GO
USE InstitutoEmpresa;
GO

IF OBJECT_ID(N'dbo.departamento', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.departamento (
        id_Departamento INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_departamento PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.localidad', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.localidad (
        CodPostal INT NOT NULL CONSTRAINT PK_localidad PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Provincia NVARCHAR(100) NOT NULL CONSTRAINT DF_localidad_Provincia DEFAULT N'Buenos Aires'
    );
END
GO

IF OBJECT_ID(N'dbo.empleados', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.empleados (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_empleados PRIMARY KEY,
        dni NVARCHAR(20) NOT NULL,
        nombre NVARCHAR(100) NOT NULL,
        apellido NVARCHAR(100) NOT NULL,
        telefono NVARCHAR(50) NULL,
        Direccion NVARCHAR(200) NULL,
        id_Departamento INT NOT NULL,
        CodPostal INT NOT NULL,
        CONSTRAINT UQ_empleados_dni UNIQUE (dni),
        CONSTRAINT FK_empleados_departamento FOREIGN KEY (id_Departamento) REFERENCES dbo.departamento(id_Departamento),
        CONSTRAINT FK_empleados_localidad FOREIGN KEY (CodPostal) REFERENCES dbo.localidad(CodPostal)
    );
END
GO

IF OBJECT_ID(N'dbo.usuario', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.usuario (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_usuario PRIMARY KEY,
        nombre NVARCHAR(100) NOT NULL,
        email NVARCHAR(200) NOT NULL,
        contrasenia NVARCHAR(500) NOT NULL,
        CONSTRAINT UQ_usuario_email UNIQUE (email),
        CONSTRAINT UQ_usuario_nombre UNIQUE (nombre)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.departamento)
BEGIN
    INSERT INTO dbo.departamento (Nombre) VALUES
    (N'Ventas'),(N'Producción'),(N'Marketing'),(N'Administración'),(N'Gerencia');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.localidad)
BEGIN
    INSERT INTO dbo.localidad (CodPostal, Nombre, Provincia) VALUES
    (1865,N'San Vicente',N'Buenos Aires'),
    (1984,N'Domselaar',N'Buenos Aires'),
    (1980,N'Brandsen',N'Buenos Aires'),
    (1804,N'Canning',N'Buenos Aires'),
    (1842,N'Monte Grande',N'Buenos Aires');
END
GO
