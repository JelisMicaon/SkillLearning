terraform {
  required_version = ">= 1.0"

  backend "s3" {
    bucket         = "skilllearning-terraform-state-bucket"
    key            = "global/eks/terraform.tfstate"
    region         = "us-east-1"
    dynamodb_table = "skilllearning-terraform-lock"
    encrypt        = true
  }

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = var.aws_region
}