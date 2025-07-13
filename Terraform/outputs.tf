output "aws_region" {
  description = "Região da AWS onde os recursos foram criados."
  value       = var.aws_region
}

output "eks_cluster_endpoint" {
  description = "Endpoint do cluster EKS."
  value       = module.eks.cluster_endpoint
}

output "eks_cluster_name" {
  description = "Nome do cluster EKS."
  value       = module.eks.cluster_name
}

output "db_endpoint" {
  description = "Endpoint da instância RDS."
  value       = module.db.db_instance_endpoint
}

output "redis_primary_endpoint" {
  description = "Endpoint primário do ElastiCache Redis."
  value       = module.redis.primary_endpoint_address
}