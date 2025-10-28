# 🏦 AccountBankChuSA

**AccountBankChuSA** é uma API desenvolvida em **.NET 6** com **PostgreSQL**, que simula as principais operações de um sistema bancário, como cadastro de clientes, abertura de contas, transferências (TED) e consulta de extratos.  

---

## 🚀 Como executar o projeto

### 🧩 Pré-requisitos
- [Docker](https://www.docker.com/get-started) instalado na máquina

---

### ⚙️ Passos para execução

1. **Clonar o repositório**
   ```bash
   git clone https://github.com/GVDAM/AccontBankChuSA.git
   ```

2. **Acessar o diretório raiz (onde está a solução `AccountsChu.sln`)**
   ```bash
   cd AccountsChu
   ```

3. **Subir os containers**
   ```bash
   docker-compose up --build
   ```

4. **Acessar a documentação da API (Swagger)**
   👉 [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html)

---

## 🗄️ Banco de Dados

| Configuração | Valor |
|---------------|-------|
| **Host**      | `localhost` |
| **Porta**     | `5433` |
| **Usuário**   | `postgres` |
| **Senha**     | `1q2w3e4r@#$` |

---

## 🧠 Lógica de Uso do Sistema

O serviço foi projetado para ser utilizado por diferentes times e operações internas do banco.  
Para garantir o uso correto, siga a sequência lógica abaixo:

1. **Cadastrar um cliente**  
   `POST api/v1/customer`  
   > Observação: um cliente pode existir sem necessariamente possuir uma conta aberta.

2. **Efetuar login**  
   `POST api/v1/customer/login`  
   > O retorno incluirá um **token JWT**, que deve ser utilizado em todas as requisições autenticadas.

3. **Abrir uma conta**  
   `POST api/v1/account`  
   > Necessário para realizar transferências e consultar extratos.

4. **Realizar transferências (TED)**  
   `POST api/v1/ted`  
   > É necessário que existam **duas contas ativas** para efetuar transferências entre elas.

5. **Consultar extrato**  
   `GET api/v1/extrato`  

---

## ✅ Observações Finais
- Utilize o **token** de autenticação em todas as rotas protegidas do módulo **Account**.  
- Caso ocorra algum erro na inicialização do banco ou migração, verifique se a porta `5433` já está em uso.  
- As variáveis de ambiente e credenciais podem ser ajustadas conforme necessário no `docker-compose.yml`.

---

**Desenvolvido com ❤️ em .NET 6 e PostgreSQL**
