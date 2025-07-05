# 🚀 SkillLearning API

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
[![Kafka](https://img.shields.io/badge/Apache%20Kafka-232323?style=for-the-badge&logo=apache-kafka&logoColor=white)](https://kafka.apache.org/)
[![Redis](https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white)](https://redis.io/)
[![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)](https://swagger.io/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## 🎯 Sobre o Projeto

**SkillLearning** é um backend robusto para uma plataforma de aprendizagem, construído com .NET para demonstrar a aplicação de uma arquitetura limpa, padrões de design avançados e uma stack de tecnologia moderna e escalável.

*Este projeto está em desenvolvimento contínuo como um portfólio vivo, refletindo a evolução de conhecimentos e a aplicação de novas tecnologias. (o README pode estar desatualizado perante alterações do projeto)*

---

## ✨ Arquitetura e Padrões de Design

A fundação do projeto é baseada em princípios que garantem um sistema manutenível, testável e desacoplado.

* **Arquitetura Limpa (Clean Architecture):** Separação de responsabilidades em camadas (`Domain`, `Application`, `Infrastructure`, `Presentation`), com o fluxo de dependências sempre apontando para o núcleo do negócio.
* **Domain-Driven Design (DDD) Tático:** As entidades (ex: `User`) são ricas em comportamento, encapsulando a lógica de negócio e evitando modelos anêmicos.
* **CQRS (Command and Query Responsibility Segregation):** Operações de escrita (Commands) e leitura (Queries) são segregadas, permitindo otimizações e lógicas independentes para cada fluxo.
* **Comunicação Assíncrona via Eventos:** Utilizando o padrão **Produtor/Consumidor** com **Apache Kafka** para desacoplar tarefas secundárias (como envio de e-mails), garantindo que a API permaneça ágil e resiliente.
* **Padrão Mediator:** Com o uso de `MediatR` para orquestrar as operações na camada de aplicação, mantendo os controllers limpos e focados em roteamento e validação.

---

## 🛠️ Stack de Tecnologias

| Categoria       | Tecnologia/Ferramenta                                  |
| --------------- | ------------------------------------------------------ |
| **Backend** | .NET 9, ASP.NET Core, C#                               |
| **Banco de Dados** | PostgreSQL, Entity Framework Core                      |
| **Mensageria** | Apache Kafka, Confluent.Kafka Client                 |
| **Cache** | Redis (Cache Distribuído)                              |
| **Autenticação**| JWT (JSON Web Tokens)                                  |
| **Testes** | xUnit, Moq, FluentAssertions                           |
| **Container** | Docker & Docker Compose                                |
| **CI/CD** | GitHub Actions                                         |
| **Documentação**| Swagger (OpenAPI)                                      |

---

## 🚀 Executando com Docker (Método Recomendado)

A maneira mais simples e consistente de executar todo o ambiente (API, Worker, Banco de Dados, Cache e Mensageria) é utilizando Docker.

### Pré-requisitos
* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Passos

1.  **Clone o Repositório**
    ```bash
    git clone [https://github.com/JelisMicaon/SkillLearning.git](https://github.com/JelisMicaon/SkillLearning.git)
    cd SkillLearning
    ```

2.  **Configure os Segredos Locais**
    Crie um arquivo chamado `.env` na raiz do projeto. Este arquivo **não será versionado** e guardará seus segredos. Copie o conteúdo abaixo e preencha com seus valores.
    ```dotenv
    # .env - Arquivo de segredos para o ambiente Docker

    # Banco de Dados
    POSTGRES_DB=AlgumNomeDeBanco
    POSTGRES_USERNAME=AlgumUsername
    POSTGRES_PASSWORD=AlgumaSenha

    # JWT
    JWT_KEY="AlgumaChaveGeradaPorVoce"

    # Configurações de E-mail
    EMAIL_SENDER_USER="SeuEmail@gmail.com"
    EMAIL_SENDER_PASSWORD="SuaSenha"

    # Chaves AWS
    AWS_ACCESS_KEY_ID=SeuAccess
    AWS_SECRET_ACCESS_KEY=SuaKey
    AWS_REGION=SuaRegiao
    ```

3.  **Inicie todos os Serviços**
    Com o Docker Desktop em execução, rode o seguinte comando na raiz do projeto:
    ```bash
    docker-compose up -d --build
    ```
    Este comando irá construir as imagens e iniciar todos os contêineres em segundo plano.

4.  **Aplique as Migrações do Banco de Dados**
    Aguarde alguns segundos para o container do PostgreSQL iniciar completamente e então execute:
    ```bash
    dotnet ef database update --project SkillLearning.Infrastructure
    ```

5.  **Acesse a Aplicação**
    * **API:** `http://localhost:5000`
    * **Documentação (Swagger):** `http://localhost:5000/swagger`
    * **Kafka UI:** `http://localhost:8080`
    * **Redis Insight:** `http://localhost:8081`

---

## 🧪 Rodando os Testes

Para executar a suíte de testes unitários, utilize o comando:

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
