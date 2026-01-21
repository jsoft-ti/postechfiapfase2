# Arquitetura e Orquestração do Projeto

Este diretório contém os recursos para orquestrar a implantação de todos os microsserviços do projeto usando Kubernetes.

## Arquitetura

O projeto é composto pelos seguintes microsserviços:

-   **fcg-UsersAPI**: Gerencia os dados dos usuários.
-   **fcg-CatalogAPI**: Gerencia o catálogo de produtos.
-   **fcg-PaymentsAPI**: Processa os pagamentos.
-   **fcg-NotificationsAPI**: Envia notificações.

Esses serviços se comunicam entre si e com serviços externos, como:

-   **PostgreSQL**: Utilizado como banco de dados para `fcg-UsersAPI` e `fcg-CatalogAPI`.
-   **RabbitMQ**: Utilizado como um message broker para comunicação assíncrona entre os serviços.

## Orquestração

O diretório `fcg-Orchestration` contém os arquivos necessários para implantar e gerenciar toda a pilha de aplicativos usando Kubernetes.

-   **Diretório `k8s/`**: Contém os arquivos de manifesto do Kubernetes para cada serviço.
-   **`deploy.sh`**: Um script para construir as imagens Docker e aplicar os manifestos do Kubernetes.
-   **`delete.sh`**: Um script para excluir os recursos do Kubernetes.
-   **`docker-compose.yml`**: Usado para executar o contêiner do deployer.

### Manifestos do Kubernetes (`k8s/`)

-   **`00-namespace.yaml`**: Cria o namespace `fcg-system`.
-   **`01-postgres.yaml`**: Implanta uma instância do PostgreSQL e cria os bancos de dados necessários.
-   **`02-rabbitmq.yaml`**: Implanta uma instância do RabbitMQ com o plugin de gerenciamento.
-   **`03-catalog-api.yaml`**: Implanta a API de Catálogo.
-   **`04-notifications-api.yaml`**: Implanta a API de Notificações.
-   **`05-payments-api.yaml`**: Implanta a API de Pagamentos.
-   **`06-users-api.yaml`**: Implanta a API de Usuários.

## Execução

### Pré-requisitos

-   Docker instalado
-   Cluster Kubernetes em execução (como Minikube ou Kubernetes do Docker Desktop)
-   `kubectl` configurado para se conectar ao seu cluster

### Implantação

1.  Navegue até o diretório `fcg-Orchestration`.
2.  docker compose up --build
    