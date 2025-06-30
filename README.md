# 🚀 SkillLearning: Plataforma de Aprendizagem de Habilidades (Backend)

[![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Kafka](https://img.shields.io/badge/Apache%20Kafka-232323?style=for-the-badge&logo=apache-kafka&logoColor=white)](https://kafka.apache.org/)
[![Redis](https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white)](https://redis.io/)
[![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)](https://swagger.io/)
[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

---

## 🎯 Sobre o Projeto

O **SkillLearning** é o backend robusto e escalável para uma plataforma de aprendizagem de habilidades, desenvolvida com foco em alta performance e manutenibilidade. Este projeto demonstra a construção de uma API com recursos de autenticação, gestão de usuários e um sistema de notificação assíncrona, utilizando as melhores práticas do mercado e uma arquitetura limpa.

**Status do Projeto**: Este projeto está em desenvolvimento contínuo. Estou aplicando novos conhecimentos e tecnologias para aprimorá-lo constantemente, e ele não está em sua versão final.

## ✨ Funcionalidades Principais

* **Sistema de Autenticação Completo**:
    * 🔐 Registro e Login de usuários com validação de credenciais.
    * 🔑 Geração e validação de JWT (JSON Web Tokens) para acesso seguro à API.
    * 🔒 Hashing de senhas seguro utilizando BCrypt para proteção de dados sensíveis.
* **Gestão de Usuários**:
    * Cadastro, recuperação e diferenciação de informações de usuário (com perfis de `Admin` e `User`).
* **Notificações Assíncronas**:
    * 📧 Envio de e-mails de notificação (ex: após login) de forma assíncrona via Apache Kafka, garantindo uma experiência de usuário fluida e o desacoplamento de serviços.
* **Caching Inteligente**:
    * 🚀 Utilização de Redis para caching de dados frequentemente acessados, otimizando o tempo de resposta da API e reduzindo a carga no banco de dados.
* **Validação de Requisições Robusta**:
    * ✅ Validação de dados de entrada com FluentValidation, garantindo a integridade dos dados e um feedback claro para o cliente da API.
* **Persistência de Dados Confiável**:
    * 🗄️ Integração com PostgreSQL utilizando Entity Framework Core, com migrações automáticas aplicadas no startup para facilitar o desenvolvimento.
* **API Documentada e Testável**:
    * 📖 Documentação interativa da API via Swagger/OpenAPI, que permite explorar e testar os endpoints facilmente.

## 🛠️ Tecnologias e Ferramentas

Este projeto demonstra proficiência e experiência prática nas seguintes tecnologias e padrões:

* **Backend**:
    * [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0): Framework moderno e de alta performance para construir aplicações robustas.
    * [C#](https://docs.microsoft.com/en-us/dotnet/csharp/): Linguagem principal de desenvolvimento, com foco em código limpo e orientado a objetos.
    * [ASP.NET Core Web API](https://docs.microsoft.com/en-us/aspnet/core/web-api/): Para construção da API RESTful, expondo recursos de forma clara e eficiente.
* **Banco de Dados**:
    * [PostgreSQL](https://www.postgresql.org/): Banco de dados relacional poderoso, escalável e de código aberto.
    * [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/): ORM (Object-Relational Mapper) para interação simplificada com o banco de dados.
* **Mensageria**:
    * [Apache Kafka](https://kafka.apache.org/): Plataforma de streaming de eventos distribuída, utilizada para comunicação assíncrona e desacoplamento de microserviços.
    * [Confluent.Kafka](https://github.com/confluentinc/confluent-kafka-dotnet): Cliente .NET para interagir com clusters Kafka.
* **Cache**:
    * [Redis](https://redis.io/): Servidor de estrutura de dados em memória, utilizado para implementar um cache distribuído de alta performance.
* **Autenticação**:
    * [JWT (JSON Web Tokens)](https://jwt.io/): Padrão aberto para criação de tokens de acesso seguros, garantindo a autenticação e autorização na API.
* **Validação**:
    * [FluentValidation](https://fluentvalidation.net/): Biblioteca para construir regras de validação claras e separadas da lógica de negócio.
* **Padrões e Ferramentas**:
    * [MediatR](https://github.com/jbogard/MediatR): Implementação do padrão Mediator, facilitando a comunicação entre componentes in-process e a aplicação do CQS (Command Query Separation).
    * [Swagger/OpenAPI](https://swagger.io/): Para documentação automática e teste interativo da API, melhorando a experiência do desenvolvedor.
* **Testes**:
    * [xUnit](https://xunit.net/): Framework de testes unitários e de integração amplamente utilizado na comunidade .NET.
    * [Moq](https://github.com/moq/moq4): Biblioteca de mocking para facilitar a criação de testes unitários.
* **Containerização**:
    * [Docker](https://www.docker.com/): Utilizado para isolar e empacotar a aplicação e seus serviços dependentes (PostgreSQL, Kafka, Redis), facilitando o desenvolvimento e implantação.

## 📐 Arquitetura do Projeto

O **SkillLearning** foi projetado seguindo os princípios da **Clean Architecture** (também conhecida como Onion Architecture ou Hexagonal Architecture). Essa abordagem garante uma forte separação de preocupações, alta testabilidade, e facilita a manutenção e escalabilidade do sistema.

A estrutura do projeto é organizada em camadas bem definidas:

* **`SkillLearning.Domain`**: O núcleo da aplicação. Contém as entidades de negócio (`User`), enums (`UserRole`), eventos de domínio (`UserLoginEvent`, `UserRegisteredEvent`) e exceções customizadas. Esta camada é completamente independente de qualquer tecnologia externa.
* **`SkillLearning.Application`**: Responsável pela lógica de aplicação e orquestração das operações. Aqui são definidos os Comandos e Queries (utilizando MediatR), os DTOs, as interfaces para serviços externos (`IUserRepository`, `IEmailSender`, `ICacheService`, `IEventPublisher`) e os comportamentos de pipeline (como `ValidationBehavior` com FluentValidation). Depende apenas da camada de `Domain`.
* **`SkillLearning.Infrastructure`**: Implementa as interfaces definidas na camada de `Application`. Contém a lógica de persistência de dados (Entity Framework Core e PostgreSQL), a implementação dos serviços de autenticação (JWT), cache (Redis), mensageria (Kafka Producer) e envio de e-mails (SMTP). Esta camada depende da `Application` e `Domain`.
* **`SkillLearning.Api`**: A camada de apresentação, implementada como uma ASP.NET Core Web API. É a porta de entrada para a aplicação, contendo os controladores RESTful, configurações de startup (`Program.cs` com setup de banco de dados, autenticação e Swagger) e tratamento de exceções. Depende das camadas `Application` e `Infrastructure`.
* **`SkillLearning.Workers.EmailSender`**: Um projeto de worker separado que atua como um serviço de fundo. Ele consome eventos de login do Apache Kafka e, em seguida, utiliza o serviço de e-mail para enviar notificações. Isso demonstra um padrão de comunicação assíncrona e desacoplada, melhorando a resiliência e a performance da aplicação principal. Depende da `Application` e `Infrastructure`.

Essa arquitetura permite que a lógica de negócio principal (`Domain` e `Application`) permaneça isolada e testável, e que a aplicação seja adaptável a futuras mudanças tecnológicas ou de infraestrutura sem impactar o domínio central.

## 🚀 Como Rodar o Projeto

Para configurar e rodar o SkillLearning em seu ambiente de desenvolvimento local, siga as instruções abaixo:

### Pré-requisitos

Certifique-se de ter as seguintes ferramentas instaladas em sua máquina:

* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) ou versão superior.
* [Docker Desktop](https://www.docker.com/products/docker-desktop) (altamente recomendado para gerenciar PostgreSQL, Kafka e Redis).
* Um cliente de banco de dados para PostgreSQL (ex: [DBeaver](https://dbeaver.io/) ou [pgAdmin](https://www.pgadmin.org/)) para visualizar os dados.

### Configuração e Execução

1.  **Clone o Repositório:**

    ```bash
    git clone [https://github.com/seu-usuario/SkillLearning.git](https://github.com/seu-usuario/SkillLearning.git)
    cd SkillLearning
    ```
    (Substitua `seu-usuario` pelo seu usuário do GitHub e `SkillLearning` pelo nome do seu repositório, se diferente.)

2.  **Inicie os Serviços com Docker Compose:**
    O projeto utiliza Docker Compose para facilmente configurar e iniciar o PostgreSQL, Apache Kafka e Redis.

    ```bash
    docker-compose up -d
    ```
    Este comando iniciará os containers necessários em segundo plano. Verifique o status dos containers com `docker-compose ps` para garantir que estão rodando.

3.  **Restaure as Dependências do .NET:**

    ```bash
    dotnet restore
    ```
    Este comando baixa todas as dependências NuGet para todos os projetos da solução.

4.  **Aplique as Migrações do Banco de Dados:**
    As migrações do Entity Framework Core para o PostgreSQL serão aplicadas automaticamente na inicialização do `SkillLearning.Api` se o banco de dados ainda não existir. Para desenvolvimento ou aplicação manual:

    ```bash
    dotnet ef database update --project SkillLearning.Infrastructure
    ```

5.  **Configure as Variáveis de Ambiente ou `appsettings`:**
    Revise os arquivos `appsettings.Development.json` localizados em `SkillLearning.Api` e `SkillLearning.Workers.EmailSender`.
    Certifique-se de que as configurações para `ConnectionStrings` (PostgreSQL), `Kafka`, `Redis`, `Jwt` e `EmailSettings` estão corretas e ajustadas para o seu ambiente local.

    **Atenção para as `EmailSettings`:**
    No `SkillLearning.Workers.EmailSender/appsettings.Development.json`, a senha SMTP (`SmtpPassword`) está configurada como um "App Password" de exemplo do Gmail (`rbpu netf pblj nikb`). Se você pretende testar o envio de e-mails, **é crucial que você gere um "App Password" para sua própria conta Google** e o substitua aqui, pois senhas de aplicativos são mais seguras que senhas normais para acesso via SMTP. Você pode gerar uma em [Google Account Security](https://myaccount.google.com/security).

    **Nota sobre JWT Key:**
    No `SkillLearning.Api/appsettings.Development.json`, a chave JWT (`Jwt:Key`) está definida como `ChaveDeDesenvolvimentoPadraoParaTestesLocaisSemDockerProducao`. Para ambientes de produção, uma chave mais segura e gerenciada por variáveis de ambiente ou segredos deve ser utilizada.

6.  **Execute o Projeto:**
    Para ver o projeto em funcionamento completo, inicie a API e o Worker de E-mail. Você pode fazer isso de duas maneiras:

    * **Via Terminal (Recomendado para desenvolvimento rápido):**
        Abra dois terminais na raiz do projeto e execute:
        ```bash
        # Terminal 1 (para a API)
        dotnet run --project SkillLearning.Api

        # Terminal 2 (para o Worker de E-mail)
        dotnet run --project SkillLearning.Workers.EmailSender
        ```

    * **Via Visual Studio / VS Code:**
        Abra a solução (`SkillLearning.sln`) em seu IDE. Configure múltiplos projetos de startup para iniciar `SkillLearning.Api` e `SkillLearning.Workers.EmailSender` simultaneamente.

7.  **Acesse a API:**
    Uma vez que a API esteja rodando, ela estará disponível em `https://localhost:7140` (ou a porta configurada no seu `launchSettings.json`).
    A documentação interativa do Swagger para testar os endpoints estará acessível em:
    `https://localhost:7140/swagger`

## 🧪 Rodando os Testes

Para executar todos os testes unitários e de integração do projeto:

```bash
dotnet test SkillLearning.Tests
```

## 📄 Licença

Este projeto está licenciado sob a [Licença MIT](LICENSE). Consulte o arquivo `LICENSE` na raiz do repositório para mais detalhes sobre os termos de uso.

## 📬 Contato

Se você tiver alguma dúvida, sugestão, feedback ou apenas quiser conversar sobre o projeto e programação, sinta-se à vontade para entrar em contato:

* **Jonas Maciel**
* **LinkedIn**: [Jonas Maciel](https://www.linkedin.com/in/jonas-maciell)
* **GitHub**: [Jelis Micaon](https://github.com/JelisMicaon)
* **Email**: jonasmacielwork@gmail.com
