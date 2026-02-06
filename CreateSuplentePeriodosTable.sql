IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SuplentePeriodos]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SuplentePeriodos](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UsuarioId] [int] NOT NULL,
        [SuplenteId] [int] NOT NULL,
        [Rol] [nvarchar](100) NOT NULL,
        [AreaId] [int] NULL,
        [GrupoId] [int] NULL,
        [FechaInicio] [datetime2](7) NOT NULL,
        [FechaFin] [datetime2](7) NOT NULL,
        [Comentarios] [nvarchar](500) NULL,
        [Activo] [bit] NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [CreatedBy] [nvarchar](100) NOT NULL,
     CONSTRAINT [PK_SuplentePeriodos] PRIMARY KEY CLUSTERED 
    (
        [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]

    ALTER TABLE [dbo].[SuplentePeriodos]  WITH CHECK ADD  CONSTRAINT [FK_SuplentePeriodos_Users_SuplenteId] FOREIGN KEY([SuplenteId])
    REFERENCES [dbo].[Users] ([Id])
    
    ALTER TABLE [dbo].[SuplentePeriodos] CHECK CONSTRAINT [FK_SuplentePeriodos_Users_SuplenteId]
    
    ALTER TABLE [dbo].[SuplentePeriodos]  WITH CHECK ADD  CONSTRAINT [FK_SuplentePeriodos_Users_UsuarioId] FOREIGN KEY([UsuarioId])
    REFERENCES [dbo].[Users] ([Id])
    
    ALTER TABLE [dbo].[SuplentePeriodos] CHECK CONSTRAINT [FK_SuplentePeriodos_Users_UsuarioId]

    -- Optional FKs if Area/Grupo tables exist and IDs match
    -- ALTER TABLE [dbo].[SuplentePeriodos]  WITH CHECK ADD  CONSTRAINT [FK_SuplentePeriodos_Areas_AreaId] FOREIGN KEY([AreaId])
    -- REFERENCES [dbo].[Areas] ([AreaId])
    
    -- ALTER TABLE [dbo].[SuplentePeriodos]  WITH CHECK ADD  CONSTRAINT [FK_SuplentePeriodos_Grupos_GrupoId] FOREIGN KEY([GrupoId])
    -- REFERENCES [dbo].[Grupos] ([GrupoId])
END
GO
