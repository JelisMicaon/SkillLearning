variable "aws_region" {
  description = "Região da AWS para provisionar os recursos."
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "Nome do projeto, usado para nomear recursos."
  type        = string
  default     = "skilllearning"
}

variable "vpc_cidr_block" {
  description = "Bloco CIDR para a VPC."
  type        = string
  default     = "10.0.0.0/16"
}

variable "db_instance_class" {
  description = "Classe de instância para o RDS PostgreSQL."
  type        = string
  default     = "db.t3.micro"
}

variable "db_username" {
  description = "Usuário master para o banco de dados."
  type        = string
  sensitive   = true # Marca a variável como sensível para não ser exibida nos logs
}

variable "db_password" {
  description = "Senha master para o banco de dados."
  type        = string
  sensitive   = true
}

variable "redis_node_type" {
  description = "Tipo de nó para o cluster ElastiCache Redis."
  type        = string
  default     = "cache.t3.micro"
}