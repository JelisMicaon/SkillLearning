output "instance_public_ip" {
  description = "IP público da instância EC2."
  value       = aws_instance.app_server.public_ip
}

output "ssh_command" {
  description = "Comando para conectar via SSH."
  value       = "ssh -i ~/.ssh/id_rsa ubuntu@${aws_instance.app_server.public_ip}"
}

output "rds_endpoint" {
  description = "RDS instance endpoint."
  value       = aws_db_instance.postgres.endpoint
}