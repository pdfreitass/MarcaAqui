-- Tabela de utilizadores (login compartilhado entre clientes e barbeiros)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Usuarios')
BEGIN
    CREATE TABLE Usuarios
    (
        id         INT            IDENTITY(1,1) NOT NULL,
        nome       NVARCHAR(100)  NOT NULL,
        email      NVARCHAR(200)  NOT NULL,
        senha_hash NVARCHAR(255)  NOT NULL,
        tipo       NVARCHAR(20)   NOT NULL,  -- 'cliente' ou 'barbeiro'
        criado_em  DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Usuarios      PRIMARY KEY (id),
        CONSTRAINT UQ_UsuariosEmail UNIQUE (email)
    );
END;
