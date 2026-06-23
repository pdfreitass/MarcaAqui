-- Tabela de controlo de versão das migrations
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = '_Migration')
BEGIN
    CREATE TABLE _Migration
    (
        Numero     INT           NOT NULL,
        Nome       NVARCHAR(200) NOT NULL,
        AplicadaEm DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Migration PRIMARY KEY (Numero)
    );
END;
