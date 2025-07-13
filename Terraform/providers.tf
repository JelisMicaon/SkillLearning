# Define a versão do Terraform para garantir consistência
terraform {
  required_version = ">= 1.0"

  # Configura o backend remoto para armazenar o estado da infraestrutura.
  # Isso é ESSENCIAL para trabalho em equipe e para evitar perda de estado.
  # Você precisará criar um bucket S3 e uma tabela DynamoDB primeiro.
  # Ex: aws s3 mb s3://skilllearning-terraform-state-bucket --region us-east-1
  # Ex: aws dynamodb create-table --table-name skilllearning-terraform-lock --attribute-definitions AttributeName=LockID,AttributeType=S --key-schema AttributeName=LockID,KeyType=HASH --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 --region us-east-1
  backend "s3" {
    bucket         = "skilllearning-terraform-state-bucket" # SUBSTITUA PELO NOME DO SEU BUCKET
    key            = "global/eks/terraform.tfstate"
    region         = "us-east-1"
    dynamodb_table = "skilllearning-terraform-lock" # Tabela para controle de concorrência
    encrypt        = true
  }

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

# Configura o provedor AWS
provider "aws" {
  region = var.aws_region
}