variable "app_name" {
  type = string
}

variable "region" {
  type = string
}

variable "environment" {
  type = string
}

variable "vpc_id" {
  type = string
}

variable "subnets" {
  type    = list(string)
  default = []
}

variable "task_definition_file" {
  type = string
}
