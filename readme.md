# Petiverso API

## Visão Geral
A **Petiverso API** é uma API desenvolvida em **.NET** para gerenciar usuários e seus pets. Ela oferece funcionalidades de autenticação, registro de usuários, gerenciamento de pets e upload de fotos.

## Tecnologias Utilizadas
- **ASP.NET Core**
- **Autenticação baseada em Cookies**
- **JWT (opcional, dependendo da implementação do serviço de usuário)**
- **Microsoft.AspNetCore.Authentication**
- **Microsoft.AspNetCore.Mvc**

---

## Endpoints

### Autenticação (`/api/auth`)

#### **Registro de Usuário**
- **Endpoint:** `POST /api/auth/register`
- **Descrição:** Registra um novo usuário.
- **Body (JSON):**
  ```json
  {
    "username": "string",
    "password": "string",
    "email": "string"
  }
  ```
- **Respostas:**
  - `200 OK`: Usuário registrado com sucesso.
  - `400 Bad Request`: Dados inválidos ou campos obrigatórios ausentes.
  - `409 Conflict`: Usuário já existe.

#### **Login de Usuário**
- **Endpoint:** `POST /api/auth/login`
- **Descrição:** Autentica um usuário e inicia uma sessão.
- **Body (JSON):**
  ```json
  {
    "username": "string",
    "password": "string"
  }
  ```
- **Respostas:**
  - `200 OK`: Login bem-sucedido.
  - `400 Bad Request`: Campos obrigatórios ausentes.
  - `401 Unauthorized`: Credenciais inválidas.

#### **Logout**
- **Endpoint:** `POST /api/auth/logout`
- **Descrição:** Encerra a sessão do usuário.
- **Respostas:**
  - `200 OK`: Logout bem-sucedido.
  - `400 Bad Request`: Sessão não encontrada.

#### **Verificação de Autenticação**
- **Endpoint:** `GET /api/auth/authenticated`
- **Descrição:** Verifica se o usuário está autenticado.
- **Respostas:**
  - `200 OK`: Retorna `authenticated: true/false` e o nome do usuário autenticado (se houver).

#### **Detalhes do Usuário**
- **Endpoint:** `GET /api/auth/user/{userId}`
- **Descrição:** Retorna os detalhes do usuário.
- **Parâmetros:**
  - `userId` (GUID) - Identificador único do usuário.
- **Respostas:**
  - `200 OK`: Retorna os detalhes do usuário.
  - `404 Not Found`: Usuário não encontrado.

---

### Pets (`/api/pets`)

#### **Adicionar um Pet**
- **Endpoint:** `POST /api/pets`
- **Descrição:** Adiciona um pet para o usuário autenticado.
- **Body (JSON):**
  ```json
  {
    "name": "string",
    "age": 0,
    "species": "string"
  }
  ```
- **Respostas:**
  - `200 OK`: Pet adicionado com sucesso.
  - `401 Unauthorized`: Usuário não autenticado.

#### **Listar Pets do Usuário**
- **Endpoint:** `GET /api/pets`
- **Descrição:** Retorna a lista de pets do usuário autenticado.
- **Respostas:**
  - `200 OK`: Lista de pets.
  - `401 Unauthorized`: Usuário não autenticado.

#### **Upload de Foto para um Pet**
- **Endpoint:** `POST /api/pets/{petId}/photos`
- **Descrição:** Faz upload de uma foto para um pet específico.
- **Parâmetros:**
  - `petId` (GUID) - Identificador único do pet.
- **Body:** Form-data contendo a imagem (`photoFile`).
- **Respostas:**
  - `200 OK`: Foto adicionada com sucesso.
  - `400 Bad Request`: Foto inválida.
  - `401 Unauthorized`: Usuário não autenticado.
  - `403 Forbidden`: O pet não pertence ao usuário.

#### **Listar Fotos de um Pet**
- **Endpoint:** `GET /api/pets/{petId}/photos`
- **Descrição:** Retorna todas as fotos associadas a um pet.
- **Parâmetros:**
  - `petId` (GUID) - Identificador único do pet.
- **Respostas:**
  - `200 OK`: Lista de fotos.
  - `404 Not Found`: Pet não encontrado ou sem fotos.

#### **Deletar uma Foto**
- **Endpoint:** `DELETE /api/pets/photos/{photoId}`
- **Descrição:** Remove uma foto específica do sistema.
- **Parâmetros:**
  - `photoId` (GUID) - Identificador único da foto.
- **Respostas:**
  - `200 OK`: Foto deletada com sucesso.
  - `404 Not Found`: Foto não encontrada.

---

## Autenticação e Segurança
A API utiliza **Cookies** para autenticação, garantindo que apenas usuários autenticados possam acessar determinados endpoints. A sessão expira após **8 horas** de inatividade.

## Como Rodar o Projeto
1. Clone o repositório:
   ```sh
   git clone https://github.com/seu-repositorio/petiverso-api.git
   ```
2. Acesse a pasta do projeto:
   ```sh
   cd petiverso-api
   ```
3. Configure as variáveis de ambiente no `appsettings.json`.
4. Execute a aplicação:
   ```sh
   dotnet run
   ```
5. A API estará disponível em `http://localhost:5067`.

## Para publicar no IIS
1. Após publicar, alterar modo de autenticação basica para `Desabilitada`


## Contato
Caso tenha dúvidas ou sugestões, entre em contato com o desenvolvedor através do e-mail **sergio.ltnj@gmail.com**.

---

📌 **Licença:** MIT License

