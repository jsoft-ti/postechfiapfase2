#!/bin/bash

echo "Deleting Kubernetes resources..."
kubectl delete -f k8s/06-users-api.yaml
kubectl delete -f k8s/05-payments-api.yaml
kubectl delete -f k8s/04-notifications-api.yaml
kubectl delete -f k8s/03-catalog-api.yaml
kubectl delete -f k8s/02-rabbitmq.yaml
kubectl delete -f k8s/01-postgres.yaml
kubectl delete -f k8s/00-namespace.yaml

echo "Deletion complete!"
