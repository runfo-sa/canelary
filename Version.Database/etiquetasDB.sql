SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE SCHEMA etiquetas
GO

CREATE TABLE [etiquetas].[Etiquetas]
(
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](256) NOT NULL,
	[descripcion] [varchar](max) NULL,
	[idEtiqueta] [int] NOT NULL,
	[version] [int] NOT NULL,
	[fecha] [datetime] NULL,
    CONSTRAINT [PK_Etiquetas] PRIMARY KEY CLUSTERED 
    (
        [id] ASC
    ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [etiquetas].[Etiquetas] ADD CONSTRAINT [DF_Etiquetas_fecha]  DEFAULT (getdate()) FOR [fecha]
GO

CREATE TABLE [etiquetas].[DefinicionEtiquetas]
(
	[id] [int] IDENTITY(1,1) NOT NULL,
	[idEtiqueta] [int] NOT NULL,
	[version] [int] NOT NULL,
    CONSTRAINT [PK_DefinicionEtiquetas] PRIMARY KEY CLUSTERED
    (
        [id] ASC,
        [idEtiqueta] ASC
    ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [etiquetas].[FormatoEtiquetas]
(
	[idEtiqueta] [int] NOT NULL,
	[idLinea] [int] NOT NULL,
    [version] [int] NOT NULL,
	[comandos] [nvarchar](max) NOT NULL,
	[comentarios] [varchar](max) NULL,
    [habilitada] [bit] NULL,
    PRIMARY KEY CLUSTERED
    (
        [idEtiqueta] ASC,
        [idLinea] ASC,
        [version] ASC
    ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [etiquetas].[FormatoEtiquetas] ADD DEFAULT ((1)) FOR [habilitada]
GO

CREATE TABLE [etiquetas].[Configuracion]
(
    [id] [int] IDENTITY(1,1) NOT NULL,
    [nombre] [varchar](64) NOT NULL,
    [valor] [nvarchar](64) NULL
    CONSTRAINT [PK_Configuracion] PRIMARY KEY CLUSTERED 
    (
        [id] ASC
    ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
)
GO

INSERT INTO [etiquetas].[Configuracion]
VALUES
('start_var', '[@'),
('end_var', '@]')
GO

CREATE TYPE [etiquetas].[Diccionario] AS TABLE
(
    [key] NVARCHAR(MAX) NOT NULL,
    [value] NVARCHAR(MAX) NULL
)
GO

CREATE or ALTER VIEW [etiquetas].[ListarEtiquetas] AS
    SELECT DISTINCT nombre
    FROM [etiquetas].[Etiquetas]
GO

CREATE or ALTER PROC [etiquetas].[GenerarCodigo](@idEtiqueta int, @version int = -1) as
BEGIN
    SET NOCOUNT ON

    DECLARE @_version INT
    DECLARE @codigo NVARCHAR(MAX)

    IF (@version = -1)
    BEGIN
        SELECT @_version = [version]
        FROM [etiquetas].[DefinicionEtiquetas]
        WHERE idEtiqueta = @idEtiqueta
    END
    ELSE
    BEGIN
        SET @_version = @version
    END

    SELECT @codigo = coalesce(@codigo + CHAR(13)+CHAR(10) + comandos, comandos)
	FROM [etiquetas].[FormatoEtiquetas]
	WHERE idEtiqueta = @idEtiqueta and [version] = @_version and habilitada = 1
	ORDER BY idLinea

    SELECT @codigo [codigo]
END
GO

CREATE or ALTER PROC [etiquetas].[CargarVariables](@idEtiqueta int, @variables [etiquetas].[Diccionario] READONLY) as
BEGIN
    SET NOCOUNT ON

    DECLARE @start NVARCHAR(MAX)
    DECLARE @end NVARCHAR(MAX)

    SELECT @start = valor
    FROM [etiquetas].[Configuracion]
    WHERE nombre = 'start_var'

    SELECT @end = valor
    FROM [etiquetas].[Configuracion]
    WHERE nombre = 'end_var'

    DECLARE @codigo_tmp TABLE(codigo NVARCHAR(MAX))
    INSERT INTO @codigo_tmp
    EXEC [etiquetas].[GenerarCodigo] @idEtiqueta

    DECLARE @codigo NVARCHAR(MAX)
    SELECT TOP(1) @codigo = codigo
    FROM @codigo_tmp

    SELECT @codigo = replace(@codigo, @start + [key] + @end, isnull([value],'')) 
    FROM @variables

    SELECT @codigo as [codigo]
END
GO

CREATE or ALTER PROC [etiquetas].[ActualizarEtiqueta](@idEtiqueta int, @codigo NVARCHAR(MAX), @descripcion VARCHAR(MAX) = NULL) as
BEGIN
    -- Tabla temporal, no la creamos como variable para poder utilizar identity.
    CREATE TABLE #etiquetas_tmp (idLinea int identity, idEtiqueta int, comandos nvarchar(max))
    SET IDENTITY_INSERT #etiquetas_tmp OFF

    -- Separamos el codigo por salto de linea, limpiamos la cadena y la ingresamos en la tabla temporal.
    INSERT INTO #etiquetas_tmp(idEtiqueta, comandos)
    SELECT @idEtiqueta, TRIM(REPLACE(A.[value], CHAR(13), ''))
    FROM STRING_SPLIT(@codigo, char(10)) as A

    DELETE
    FROM #etiquetas_tmp
    WHERE comandos = ''

    -- Obtener la ultima version de esta etiqueta
    DECLARE @version INT
    
    SELECT @version = MAX([version]) + 1
    FROM [etiquetas].[Etiquetas]
    WHERE idEtiqueta = @idEtiqueta

    -- Guardar etiqueta en la base de datos.
    INSERT INTO [etiquetas].[FormatoEtiquetas]([idEtiqueta], [idLinea], [version], [comandos])
    SELECT idEtiqueta, idLinea, @version as [version], comandos
    FROM #etiquetas_tmp

    INSERT INTO [etiquetas].[Etiquetas]
    SELECT TOP(1) nombre, COALESCE(@descripcion, descripcion), idEtiqueta, @version, GETDATE()
    FROM [etiquetas].[Etiquetas]
    WHERE idEtiqueta = @idEtiqueta

    UPDATE A SET A.[version] = @version
    FROM [etiquetas].[DefinicionEtiquetas] as A
    WHERE A.[idEtiqueta] = @idEtiqueta

    DROP TABLE #etiquetas_tmp
END
GO

CREATE or ALTER PROC [etiquetas].[CrearFormato](@idEtiqueta int, @codigo NVARCHAR(MAX)) as
BEGIN
    -- Tabla temporal, no la creamos como variable para poder utilizar identity.
    CREATE TABLE #etiquetas_tmp (idLinea int identity, idEtiqueta int, comandos nvarchar(max))
    SET IDENTITY_INSERT #etiquetas_tmp OFF

    -- Separamos el codigo por salto de linea, limpiamos la cadena y la ingresamos en la tabla temporal.
    INSERT INTO #etiquetas_tmp(idEtiqueta, comandos)
    SELECT @idEtiqueta, TRIM(REPLACE(A.[value], CHAR(13), ''))
    FROM STRING_SPLIT(@codigo, char(10)) as A

    -- Guardar etiqueta en la base de datos.
    INSERT INTO [etiquetas].[FormatoEtiquetas]([idEtiqueta], [idLinea], [version], [comandos])
    SELECT idEtiqueta, idLinea, 1 as [version], comandos
    FROM #etiquetas_tmp

    DROP TABLE #etiquetas_tmp
END
GO

CREATE or ALTER PROC [etiquetas].[CrearEtiqueta](@nombre VARCHAR(MAX), @codigo NVARCHAR(MAX), @descripcion VARCHAR(MAX) = '') as
BEGIN
    DECLARE @idEtiqueta INT

    IF (@nombre in (SELECT nombre FROM [etiquetas].[Etiquetas]))
    BEGIN
        RAISERROR('Ya existe una etiqueta con el mismo nombre', 11, 1)
        RETURN;
    END

    SELECT @idEtiqueta = MAX([idEtiqueta]) + 1
    FROM [etiquetas].[Etiquetas]

    INSERT INTO [etiquetas].[Etiquetas]
    VALUES (@nombre, @descripcion, @idEtiqueta, 1, getdate())

    INSERT INTO [etiquetas].[DefinicionEtiquetas]
    VALUES (@idEtiqueta, 1)

    EXEC [etiquetas].[CrearFormato] @idEtiqueta = @idEtiqueta, @codigo = @codigo
END
GO