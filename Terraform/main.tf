# ------------------------------------------------------------------------------------------------
# MÓDULO DE REDE (VPC)
# Cria a fundação da nossa rede: VPC, subnets públicas e privadas, internet gateway, etc.
# ------------------------------------------------------------------------------------------------
module "vpc" {
  source  = "terraform-aws-modules/vpc/aws"
  version = "5.5.2"

  name = "${var.project_name}-vpc"
  cidr = var.vpc_cidr_block

  azs             = ["${var.aws_region}a", "${var.aws_region}b", "${var.aws_region}c"]
  private_subnets = ["10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24"]
  public_subnets  = ["10.0.101.0/24", "10.0.102.0/24", "10.0.103.0/24"]

  enable_nat_gateway = true
  single_nat_gateway = true # Economiza custos em ambientes de dev/teste

  tags = {
    Terraform   = "true"
    Environment = "dev"
    Project     = var.project_name
  }
}

# ------------------------------------------------------------------------------------------------
# MÓDULO DO CLUSTER KUBERNETES (EKS)
# Provisiona um cluster EKS gerenciado.
# ------------------------------------------------------------------------------------------------
module "eks" {
  source  = "terraform-aws-modules/eks/aws"
  version = "20.8.4"

  cluster_name    = "${var.project_name}-cluster"
  cluster_version = "1.29"

  vpc_id     = module.vpc.vpc_id
  subnet_ids = module.vpc.private_subnets

  eks_managed_node_groups = {
    general_purpose = {
      min_size     = 1
      max_size     = 3
      desired_size = 2

      instance_types = ["t3.medium"]
    }
  }

  tags = {
    Terraform   = "true"
    Environment = "dev"
    Project     = var.project_name
  }
}

# ------------------------------------------------------------------------------------------------
# GRUPO DE SEGURANÇA PARA BANCO DE DADOS E CACHE
# Controla quem pode acessar o RDS e o ElastiCache.
# ------------------------------------------------------------------------------------------------
resource "aws_security_group" "data_services" {
  name        = "${var.project_name}-data-services-sg"
  description = "Permite acesso do cluster EKS ao RDS e ElastiCache"
  vpc_id      = module.vpc.vpc_id

  # Permite todo o tráfego vindo do Security Group do Cluster EKS
  ingress {
    from_port       = 0
    to_port         = 0
    protocol        = "-1"
    security_groups = [module.eks.node_security_group_id]
  }

  # Permite todo o tráfego de saída
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.project_name}-data-services-sg"
  }
}

# ------------------------------------------------------------------------------------------------
# MÓDULO DE BANCO DE DADOS (RDS PostgreSQL)
# ------------------------------------------------------------------------------------------------
module "db" {
  source  = "terraform-aws-modules/rds/aws"
  version = "6.6.0"

  identifier = "${var.project_name}-db"

  engine            = "postgres"
  engine_version    = "15"
  instance_class    = var.db_instance_class
  allocated_storage = 20

  db_name  = "${var.project_name}db"
  username = var.db_username
  password = var.db_password
  port     = "5432"

  multi_az               = false # Defina como true para produção real
  vpc_security_group_ids = [aws_security_group.data_services.id]
  subnet_ids             = module.vpc.private_subnets

  maintenance_window      = "Mon:00:00-Mon:03:00"
  backup_window           = "03:00-06:00"
  skip_final_snapshot     = true
  db_subnet_group_use_name_prefix = true

  tags = {
    Terraform   = "true"
    Environment = "dev"
    Project     = var.project_name
  }
}

# ------------------------------------------------------------------------------------------------
# MÓDULO DE CACHE (ElastiCache Redis)
# ------------------------------------------------------------------------------------------------
module "redis" {
  source  = "terraform-aws-modules/elasticache/aws"
  version = "1.6.0"

  name = "${var.project_name}-redis"

  engine               = "redis"
  family               = "redis7"
  replication_group_id = "${var.project_name}-redis-rg"
  node_type            = var.redis_node_type
  num_cache_nodes      = 1
  port                 = 6379

  vpc_id             = module.vpc.vpc_id
  subnet_ids         = module.vpc.private_subnets
  security_group_ids = [aws_security_group.data_services.id]

  tags = {
    Terraform   = "true"
    Environment = "dev"
    Project     = var.project_name
  }
}