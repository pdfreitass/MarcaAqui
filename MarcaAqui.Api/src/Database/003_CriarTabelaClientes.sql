-- Tabela de clientes (perfil vinculado a um utilizador do tipo 'cliente')
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Clientes')
BEGIN
    CREATE TABLE Clientes
    (
        id         INT           IDENTITY(1,1) NOT NULL,
        usuario_id INT           NOT NULL,
        telefone   NVARCHAR(20)  NULL,

        CONSTRAINT PK_Clientes          PRIMARY KEY (id),
        CONSTRAINT UQ_ClientesUsuario   UNIQUE (usuario_id),
        CONSTRAINT FK_Clientes_Usuarios FOREIGN KEY (usuario_id)
            REFERENCES Usuarios(id)
    );
END;
