drop TABLE LoginAttempts;
drop table Sessions;
drop table PetPhotos;
drop table pets;
drop table users;

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,      -- Chave primária auto-incremental.
    UserId UNIQUEIDENTIFIER NOT NULL UNIQUE, -- UserId com chave única.
    Username NVARCHAR(50) NOT NULL UNIQUE,  -- Username obrigatório e único.
    Email NVARCHAR(255) NOT NULL UNIQUE,    -- Email obrigatório e único.
    PasswordHash NVARCHAR(MAX) NOT NULL,    -- Hash da senha obrigatória.
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE() -- Data de criação padrão como o horário atual (UTC).
);

CREATE TABLE Pets (
    PetId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- Identificador único do pet
    UserId UNIQUEIDENTIFIER NOT NULL, -- Referência ao dono do pet
    Name NVARCHAR(100) NOT NULL, -- Nome do pet
    Species NVARCHAR(50) NOT NULL, -- Espécie do pet (ex.: cachorro, gato)
	gender NVARCHAR(5) NOT NULL,
	Photo NVARCHAR(300) NOT NULL, -- Caminho no sistema de arquivos onde a foto está armazenada
    Breed NVARCHAR(100) NULL, -- Raça (opcional)
    ColorPri NVARCHAR(50) NOT NULL, -- Cor Primaria
	ColorSec NVARCHAR(50) NULL, -- Cor Secundaria (opcional)
    Size NVARCHAR(20) NOT NULL, -- Porte (pequeno, médio, grande)
    BirthDate DATE NULL, -- Data de nascimento
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(), -- Data de criação
    FOREIGN KEY (UserId) REFERENCES Users (UserId) -- Chave estrangeira para a tabela Users
);

CREATE TABLE PetPhotos (
    PhotoId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- Identificador único da foto
    PetId UNIQUEIDENTIFIER NOT NULL, -- Identificador do pet
    FilePath NVARCHAR(300) NOT NULL, -- Caminho no sistema de arquivos onde a foto está armazenada
    UploadedAt DATETIME NOT NULL DEFAULT GETDATE(), -- Data e hora do upload
    FOREIGN KEY (PetId) REFERENCES Pets (PetId) -- Chave estrangeira para a tabela Pets
);

CREATE TABLE Sessions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    SessionToken NVARCHAR(128) NOT NULL UNIQUE,
    ExpiresAt DATETIME NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE TABLE LoginAttempts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NULL,
    Username NVARCHAR(100) NOT NULL,
    Success BIT NOT NULL,
    AttemptedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE SET NULL
);


CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Sessions_SessionToken ON Sessions(SessionToken);
