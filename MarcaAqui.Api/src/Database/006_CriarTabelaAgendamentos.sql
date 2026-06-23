-- Tabela de agendamentos (marcações feitas pelos clientes)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Agendamentos')
BEGIN
    CREATE TABLE Agendamentos
    (
        id               INT           IDENTITY(1,1) NOT NULL,
        cliente_id       INT           NOT NULL,
        profissional_id  INT           NOT NULL,
        servico_id       INT           NOT NULL,
        data_hora_inicio DATETIME2     NOT NULL,
        data_hora_fim    DATETIME2     NOT NULL,
        status           NVARCHAR(20)  NOT NULL DEFAULT 'confirmado',  -- 'confirmado', 'cancelado', 'concluido'

        CONSTRAINT PK_Agendamentos             PRIMARY KEY (id),
        CONSTRAINT FK_Agendamentos_Clientes     FOREIGN KEY (cliente_id)
            REFERENCES Clientes(id),
        CONSTRAINT FK_Agendamentos_Profissionais FOREIGN KEY (profissional_id)
            REFERENCES Profissionais(id),
        CONSTRAINT FK_Agendamentos_Servicos     FOREIGN KEY (servico_id)
            REFERENCES Servicos(id)
    );
END;
