IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE TABLE [Clientes] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(100) NOT NULL,
        [CNPJ] nvarchar(18) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [Logradouro] nvarchar(200) NOT NULL,
        [Numero] nvarchar(10) NOT NULL,
        [Complemento] nvarchar(50) NOT NULL,
        [CEP] nvarchar(9) NOT NULL,
        [Estado] nvarchar(2) NOT NULL,
        [Cidade] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE TABLE [Funcionalidades] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(100) NOT NULL,
        [Controller] nvarchar(100) NOT NULL,
        [Action] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Funcionalidades] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE TABLE [Perfis] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Perfis] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE TABLE [Unidades] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(50) NOT NULL,
        [CPF] nvarchar(14) NOT NULL,
        [Apelido] nvarchar(10) NOT NULL,
        [Celular] nvarchar(15) NOT NULL,
        [Status] int NOT NULL,
        [Carro] nvarchar(20) NOT NULL,
        [Placa] nvarchar(8) NOT NULL,
        [IMEI] nvarchar(50) NOT NULL,
        [PercentualCorrida] decimal(5,2) NOT NULL,
        CONSTRAINT [PK_Unidades] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE TABLE [CentrosCusto] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(10) NOT NULL,
        [Descricao] nvarchar(50) NOT NULL,
        [ClienteId] int NOT NULL,
        CONSTRAINT [PK_CentrosCusto] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CentrosCusto_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE TABLE [Trechos] (
        [Id] int NOT NULL IDENTITY,
        [ClienteId] int NOT NULL,
        [NomeTrecho] nvarchar(15) NOT NULL,
        [TrechoInicio] nvarchar(100) NOT NULL,
        [TrechoTermino] nvarchar(100) NOT NULL,
        [Valor] decimal(10,2) NOT NULL,
        [Status] int NOT NULL,
        CONSTRAINT [PK_Trechos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Trechos_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE TABLE [UsuariosAutorizados] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(50) NOT NULL,
        [Funcional] nvarchar(15) NOT NULL,
        [TipoSolicitante] int NOT NULL,
        [Status] int NOT NULL,
        [Telefone1] nvarchar(20) NOT NULL,
        [Telefone2] nvarchar(20) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [ClienteId] int NOT NULL,
        CONSTRAINT [PK_UsuariosAutorizados] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UsuariosAutorizados_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE TABLE [PerfilFuncionalidades] (
        [Id] int NOT NULL IDENTITY,
        [PerfilId] int NOT NULL,
        [FuncionalidadeId] int NOT NULL,
        [PodeConsultar] bit NOT NULL,
        [PodeEditar] bit NOT NULL,
        CONSTRAINT [PK_PerfilFuncionalidades] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PerfilFuncionalidades_Funcionalidades_FuncionalidadeId] FOREIGN KEY ([FuncionalidadeId]) REFERENCES [Funcionalidades] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PerfilFuncionalidades_Perfis_PerfilId] FOREIGN KEY ([PerfilId]) REFERENCES [Perfis] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(50) NOT NULL,
        [Login] nvarchar(10) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [PerfilId] int NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Usuarios_Perfis_PerfilId] FOREIGN KEY ([PerfilId]) REFERENCES [Perfis] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE TABLE [Corridas] (
        [Id] int NOT NULL IDENTITY,
        [ClienteId] int NOT NULL,
        [SolicitanteId] int NOT NULL,
        [TipoTarifa] int NOT NULL,
        [EnderecoInicial] nvarchar(100) NOT NULL,
        [EnderecoFinal] nvarchar(100) NOT NULL,
        [KmInicial] nvarchar(4) NOT NULL,
        [KmFinal] nvarchar(4) NOT NULL,
        [TrechoId] int NULL,
        [DataHoraAgendamento] datetime2 NOT NULL,
        [UnidadeId] int NOT NULL,
        [Valor] decimal(10,2) NOT NULL,
        [Observacao] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [CentroCustoId] int NULL,
        CONSTRAINT [PK_Corridas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Corridas_CentrosCusto_CentroCustoId] FOREIGN KEY ([CentroCustoId]) REFERENCES [CentrosCusto] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Corridas_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Corridas_Trechos_TrechoId] FOREIGN KEY ([TrechoId]) REFERENCES [Trechos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Corridas_Unidades_UnidadeId] FOREIGN KEY ([UnidadeId]) REFERENCES [Unidades] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Corridas_UsuariosAutorizados_SolicitanteId] FOREIGN KEY ([SolicitanteId]) REFERENCES [UsuariosAutorizados] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Action', N'Controller', N'Nome') AND [object_id] = OBJECT_ID(N'[Funcionalidades]'))
        SET IDENTITY_INSERT [Funcionalidades] ON;
    EXEC(N'INSERT INTO [Funcionalidades] ([Id], [Action], [Controller], [Nome])
    VALUES (1, N''Index'', N''Clientes'', N''Clientes''),
    (2, N''Index'', N''Unidades'', N''Unidades''),
    (3, N''Index'', N''Trechos'', N''Trechos''),
    (4, N''Index'', N''Corridas'', N''Corridas''),
    (5, N''Index'', N''Relatorios'', N''Relatórios''),
    (6, N''Index'', N''Perfis'', N''Perfis''),
    (7, N''Index'', N''Usuarios'', N''Usuários'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Action', N'Controller', N'Nome') AND [object_id] = OBJECT_ID(N'[Funcionalidades]'))
        SET IDENTITY_INSERT [Funcionalidades] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nome') AND [object_id] = OBJECT_ID(N'[Perfis]'))
        SET IDENTITY_INSERT [Perfis] ON;
    EXEC(N'INSERT INTO [Perfis] ([Id], [Nome])
    VALUES (1, N''Administrador'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nome') AND [object_id] = OBJECT_ID(N'[Perfis]'))
        SET IDENTITY_INSERT [Perfis] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'FuncionalidadeId', N'PerfilId', N'PodeConsultar', N'PodeEditar') AND [object_id] = OBJECT_ID(N'[PerfilFuncionalidades]'))
        SET IDENTITY_INSERT [PerfilFuncionalidades] ON;
    EXEC(N'INSERT INTO [PerfilFuncionalidades] ([Id], [FuncionalidadeId], [PerfilId], [PodeConsultar], [PodeEditar])
    VALUES (1, 1, 1, CAST(1 AS bit), CAST(1 AS bit)),
    (2, 2, 1, CAST(1 AS bit), CAST(1 AS bit)),
    (3, 3, 1, CAST(1 AS bit), CAST(1 AS bit)),
    (4, 4, 1, CAST(1 AS bit), CAST(1 AS bit)),
    (5, 5, 1, CAST(1 AS bit), CAST(1 AS bit)),
    (6, 6, 1, CAST(1 AS bit), CAST(1 AS bit)),
    (7, 7, 1, CAST(1 AS bit), CAST(1 AS bit))');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'FuncionalidadeId', N'PerfilId', N'PodeConsultar', N'PodeEditar') AND [object_id] = OBJECT_ID(N'[PerfilFuncionalidades]'))
        SET IDENTITY_INSERT [PerfilFuncionalidades] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Email', N'Login', N'Nome', N'PerfilId') AND [object_id] = OBJECT_ID(N'[Usuarios]'))
        SET IDENTITY_INSERT [Usuarios] ON;
    EXEC(N'INSERT INTO [Usuarios] ([Id], [Email], [Login], [Nome], [PerfilId])
    VALUES (1, N''admin@frotataxi.com'', N''admin'', N''Administrador'', 1)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Email', N'Login', N'Nome', N'PerfilId') AND [object_id] = OBJECT_ID(N'[Usuarios]'))
        SET IDENTITY_INSERT [Usuarios] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CentrosCusto_ClienteId] ON [CentrosCusto] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Corridas_CentroCustoId] ON [Corridas] ([CentroCustoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Corridas_ClienteId] ON [Corridas] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Corridas_SolicitanteId] ON [Corridas] ([SolicitanteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Corridas_TrechoId] ON [Corridas] ([TrechoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Corridas_UnidadeId] ON [Corridas] ([UnidadeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PerfilFuncionalidades_FuncionalidadeId] ON [PerfilFuncionalidades] ([FuncionalidadeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PerfilFuncionalidades_PerfilId] ON [PerfilFuncionalidades] ([PerfilId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Trechos_ClienteId] ON [Trechos] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Usuarios_PerfilId] ON [Usuarios] ([PerfilId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UsuariosAutorizados_ClienteId] ON [UsuariosAutorizados] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902210210_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250902210210_InitialCreate', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902221016_UpdateCorridaDataTypes'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Corridas]') AND [c].[name] = N'Observacao');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Corridas] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Corridas] ALTER COLUMN [Observacao] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902221016_UpdateCorridaDataTypes'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Corridas]') AND [c].[name] = N'KmInicial');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Corridas] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Corridas] ALTER COLUMN [KmInicial] decimal(10,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902221016_UpdateCorridaDataTypes'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Corridas]') AND [c].[name] = N'KmFinal');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Corridas] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Corridas] ALTER COLUMN [KmFinal] decimal(10,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902221016_UpdateCorridaDataTypes'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Corridas]') AND [c].[name] = N'EnderecoInicial');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Corridas] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Corridas] ALTER COLUMN [EnderecoInicial] nvarchar(200) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902221016_UpdateCorridaDataTypes'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Corridas]') AND [c].[name] = N'EnderecoFinal');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Corridas] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Corridas] ALTER COLUMN [EnderecoFinal] nvarchar(200) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902221016_UpdateCorridaDataTypes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250902221016_UpdateCorridaDataTypes', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250904012815_UpdateUsuarioModel'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Usuarios]') AND [c].[name] = N'Login');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Usuarios] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Usuarios] ALTER COLUMN [Login] nvarchar(20) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250904012815_UpdateUsuarioModel'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [Ativo] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250904012815_UpdateUsuarioModel'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [DataCriacao] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250904012815_UpdateUsuarioModel'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [Senha] nvarchar(100) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250904012815_UpdateUsuarioModel'
)
BEGIN
    EXEC(N'UPDATE [Usuarios] SET [Ativo] = CAST(1 AS bit), [DataCriacao] = ''2025-01-01T00:00:00.0000000'', [Senha] = N''8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250904012815_UpdateUsuarioModel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250904012815_UpdateUsuarioModel', N'9.0.0');
END;

COMMIT;
GO

