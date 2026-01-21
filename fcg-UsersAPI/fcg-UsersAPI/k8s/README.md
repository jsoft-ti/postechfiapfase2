# Kubernetes Deployment

Este diretório contém os manifestos Kubernetes para implantar a `fcg-UsersAPI` e o banco de dados PostgreSQL.

## Pré-requisitos

- Kubernetes Cluster (Minikube, Docker Desktop, Kind, etc.)
- kubectl configurado
- Docker

## Passos para Deployment

### 1. Build da Imagem Docker

Antes de aplicar os manifestos, você precisa construir a imagem Docker da API.

Execute o comando abaixo na raiz do repositório (um nível acima desta pasta):

```bash
docker build -t fcg-usersapi:latest -f fcg-UsersAPI/Dockerfile .
```

> **Nota para usuários Minikube:** Se estiver usando Minikube, certifique-se de apontar seu ambiente Docker para o Minikube antes de buildar:
> ```bash
> eval $(minikube docker-env)
> docker build -t fcg-usersapi:latest -f fcg-UsersAPI/Dockerfile .
> ```
> Ou carregue a imagem: `minikube image load fcg-usersapi:latest`

### 2. Aplicar Manifestos

Aplique todos os arquivos de configuração nesta pasta:

```bash
kubectl apply -f k8s/
```

### 3. Verificar Status

Verifique se os pods estão rodando:

```bash
kubectl get pods
kubectl get services
```

### 4. Acessar a API

Se estiver usando Docker Desktop ou um Cloud Provider com LoadBalancer, o serviço `fcg-usersapi-service` estará exposto na porta 80 (localhost).

Se estiver usando Minikube, execute:
```bash
minikube service fcg-usersapi-service
```

## Detalhes da Configuração

- **PostgreSQL**:
  - Usuário: `fcguser`
  - Senha: `fcgpass` (definida em `postgres-secret.yaml`)
  - Banco: `fcgdb`
  - Porta Interna: 5432
  - Porta do Service: 5431 (para compatibilidade com `appsettings.json`)

- **API**:
  - Replicas: 1
  - Porta Interna: 8081
  - Porta Externa: 80
