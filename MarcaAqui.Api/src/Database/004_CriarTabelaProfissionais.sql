-- Tabela de profissionais (barbeiros vinculados a uma barbearia)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Profissionais')
BEGIN
    CREATE TABLE Profissionais
    (
        id           INT IDENTITY(1,1) NOT NULL,
        usuario_id   INT NOT NULL,
        barbearia_id INT NOT NULL,

        CONSTRAINT PK_Profissionais            PRIMARY KEY (id),
        CONSTRAINT FK_Profissionais_Usuarios   FOREIGN KEY (usuario_id)
            REFERENCES Usuarios(id),
        CONSTRAINT FK_Profissionais_Barbearias FOREIGN KEY (barbearia_id)
            REFERENCES Barbearias(id)
    );
END;
