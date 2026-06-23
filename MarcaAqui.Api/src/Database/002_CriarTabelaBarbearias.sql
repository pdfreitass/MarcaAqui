-- Tabela de barbearias (uma por barbeiro-dono)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Barbearias')
BEGIN
    CREATE TABLE Barbearias
    (
        id              INT            IDENTITY(1,1) NOT NULL,
        nome            NVARCHAR(150)  NOT NULL,
        endereco        NVARCHAR(300)  NULL,
        telefone        NVARCHAR(20)   NULL,
        usuario_dono_id INT            NOT NULL,

        CONSTRAINT PK_Barbearias           PRIMARY KEY (id),
        CONSTRAINT FK_Barbearias_Usuarios  FOREIGN KEY (usuario_dono_id)
            REFERENCES Usuarios(id)
    );
END;
