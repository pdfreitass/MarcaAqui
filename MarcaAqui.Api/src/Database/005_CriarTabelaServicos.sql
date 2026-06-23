-- Tabela de serviços oferecidos por cada barbearia
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Servicos')
BEGIN
    CREATE TABLE Servicos
    (
        id               INT            IDENTITY(1,1) NOT NULL,
        barbearia_id     INT            NOT NULL,
        nome             NVARCHAR(150)  NOT NULL,
        duracao_minutos  INT            NOT NULL,
        preco            DECIMAL(10,2)  NOT NULL DEFAULT 0,

        CONSTRAINT PK_Servicos            PRIMARY KEY (id),
        CONSTRAINT FK_Servicos_Barbearias FOREIGN KEY (barbearia_id)
            REFERENCES Barbearias(id)
    );
END;
