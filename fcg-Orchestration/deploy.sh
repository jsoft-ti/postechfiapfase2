#!/bin/bash

# Define image names
CATALOG_IMAGE="fcg-catalog-api:latest"
NOTIFICATIONS_IMAGE="fcg-notifications-api:latest"
PAYMENTS_IMAGE="fcg-payments-api:latest"
USERS_IMAGE="fcg-users-api:latest"

# Build images
echo "Building Catalog API..."
docker build -t $CATALOG_IMAGE -f ../fcg-CatalogAPI/fcg-CatalogAPI/fcg-CatalogAPI/Dockerfile ../fcg-CatalogAPI/fcg-CatalogAPI

echo "Building Notifications API..."
docker build -t $NOTIFICATIONS_IMAGE -f ../fcg-NotificationsAPI/fcg-NotificationsAPI/fcg-NotificationsAPI/Dockerfile ../fcg-NotificationsAPI/fcg-NotificationsAPI

echo "Building Payments API..."
docker build -t $PAYMENTS_IMAGE -f ../fcg-PaymentsAPI/fcg-PaymentsAPI/fcg-PaymentsAPI/Dockerfile ../fcg-PaymentsAPI/fcg-PaymentsAPI

echo "Building Users API..."
docker build -t $USERS_IMAGE -f ../fcg-UsersAPI/fcg-UsersAPI/fcg-UsersAPI/Dockerfile ../fcg-UsersAPI/fcg-UsersAPI

# Apply Kubernetes manifests
echo "Applying Kubernetes manifests..."
kubectl delete service postgres -n fcg-system --ignore-not-found
kubectl delete deployment postgres -n fcg-system --ignore-not-found
kubectl delete pvc postgres-pvc -n fcg-system --ignore-not-found
kubectl apply -f k8s/00-namespace.yaml
kubectl apply -f k8s/01-postgres.yaml
kubectl apply -f k8s/02-rabbitmq.yaml
kubectl apply -f k8s/03-catalog-api.yaml
kubectl apply -f k8s/04-notifications-api.yaml
kubectl apply -f k8s/05-payments-api.yaml
kubectl apply -f k8s/06-users-api.yaml

echo "Deployment complete! Check status with: kubectl get pods -n fcg-system"
