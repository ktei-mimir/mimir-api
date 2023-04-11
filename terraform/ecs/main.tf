locals {
  cluster_name  = "${var.app_name}-apps-${var.environment}"
  ecr_repo_name = "${var.app_name}-${var.environment}"

  user_data = <<-EOT
    #!/bin/bash
    cat <<'EOF' >> /etc/ecs/ecs.config
    ECS_CLUSTER=${local.cluster_name}
    ECS_LOGLEVEL=debug
    EOF
  EOT

  instance_type = "t4g.nano"

  region = split(":", data.aws_caller_identity.current.arn)[3]

  tags = {
    Name       = local.cluster_name
    Repository = "https://github.com/terraform-aws-modules/terraform-aws-ecs"
  }
}

resource "aws_ecs_service" "service" {
  name            = "${var.app_name}-${var.environment}"
  cluster         = local.cluster_name
  task_definition = aws_ecs_task_definition.task.arn
  desired_count   = 1

  ordered_placement_strategy {
    type  = "binpack"
    field = "memory"
  }
}

resource "aws_ecs_task_definition" "task" {
  family                = "${var.app_name}-${var.environment}"
  task_role_arn         = aws_iam_role.task_role.arn
  execution_role_arn    = aws_iam_role.task_execution_role.arn
  cpu                   = 2048
  memory                = 256
  container_definitions = file(var.task_definition_file)
}

resource "aws_iam_role" "task_execution_role" {
  name               = "${var.app_name}-${var.environment}-task-execution"
  assume_role_policy = data.aws_iam_policy_document.ecs_task_assume_policy.json
}

resource "aws_iam_role_policy_attachment" "task_execution_role" {
  role       = aws_iam_role.task_execution_role.id
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

resource "aws_iam_policy" "task_execution_role" {
  name   = "${var.app_name}-${var.environment}-task-execution-policy"
  policy = data.aws_iam_policy_document.task_execution_role_policy.json
}

data "aws_iam_policy_document" "task_execution_role_policy" {
  statement {
    actions = ["ssm:GetParameters", "ssm:GetParameter"]
    resources = [
      "*"
    ]
  }
}

resource "aws_iam_role" "task_role" {
  name               = "${var.app_name}-${var.environment}-task"
  assume_role_policy = data.aws_iam_policy_document.ecs_task_assume_policy.json
}

resource "aws_iam_role_policy_attachment" "task_role" {
  role       = aws_iam_role.task_role.id
  policy_arn = aws_iam_policy.task_role.arn
}

resource "aws_iam_policy" "task_role" {
  name   = "${var.app_name}-${var.environment}-task_role_policy"
  policy = data.aws_iam_policy_document.task_role_policy.json
}

data "aws_iam_policy_document" "task_role_policy" {
  statement {
    actions = [
      "logs:CreateLogGroup",
      "logs:CreateLogStream",
      "logs:PutLogEvents"
    ]
    resources = [
      "arn:aws:logs:${local.region}:${data.aws_caller_identity.current.account_id}:log-group:${var.app_name}-${var.environment}",
      "arn:aws:logs:${local.region}:${data.aws_caller_identity.current.account_id}:log-group:${var.app_name}-${var.environment}:*",
    ]
  }
}

data "aws_iam_policy_document" "ecs_task_assume_policy" {
  statement {

    actions = [
      "sts:AssumeRole"
    ]

    principals {
      type        = "Service"
      identifiers = ["ecs-tasks.amazonaws.com"]
    }

    effect = "Allow"
  }
}

module "ecs" {
  source = "terraform-aws-modules/ecs/aws"

  cluster_name = local.cluster_name

  cluster_configuration = {
    execute_command_configuration = {
      logging = "OVERRIDE"
      log_configuration = {
        cloud_watch_log_group_name = "/aws/ecs/${local.cluster_name}"
      }
    }
  }
  autoscaling_capacity_providers = {
    one = {
      auto_scaling_group_arn         = module.autoscaling.autoscaling_group_arn
      managed_termination_protection = "ENABLED"
    }
  }
}

module "autoscaling" {
  source  = "terraform-aws-modules/autoscaling/aws"
  version = "~> 6.5"

  name          = "${local.cluster_name}-one"
  instance_type = local.instance_type

  use_mixed_instances_policy = true
  mixed_instances_policy = {
    instances_distribution = {
      on_demand_percentage_above_base_capacity = 0
      spot_instance_pools                      = 2
      spot_allocation_strategy                 = "lowest-price"
    }
    override = [
      {
        instance_type = local.instance_type
      }
    ]
  }
  image_id = jsondecode(data.aws_ssm_parameter.ecs_optimized_ami.value)["image_id"]

  security_groups = [module.autoscaling_sg.security_group_id]
  network_interfaces = [{
    associate_public_ip_address = true
  }]
  user_data                       = base64encode(local.user_data)
  ignore_desired_capacity_changes = true

  create_iam_instance_profile = true
  iam_role_name               = local.cluster_name
  iam_role_description        = "ECS role for ${local.cluster_name}"
  iam_role_policies = {
    AmazonEC2ContainerServiceforEC2Role = "arn:aws:iam::aws:policy/service-role/AmazonEC2ContainerServiceforEC2Role"
    AmazonSSMManagedInstanceCore        = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
  }

  vpc_zone_identifier = var.subnets
  health_check_type   = "EC2"
  min_size            = 0
  max_size            = 1
  desired_capacity    = 1

  # https://github.com/hashicorp/terraform-provider-aws/issues/12582
  autoscaling_group_tags = {
    AmazonECSManaged = true
  }

  # Required for  managed_termination_protection = "ENABLED"
  protect_from_scale_in = true

  tags = local.tags
}

module "autoscaling_sg" {
  source  = "terraform-aws-modules/security-group/aws"
  version = "~> 4.0"

  name        = local.cluster_name
  description = "Autoscaling group security group"
  vpc_id      = var.vpc_id

  ingress_cidr_blocks = ["0.0.0.0/0"]
  ingress_rules       = ["https-443-tcp"]

  egress_rules = ["all-all"]

  tags = local.tags
}

resource "aws_ecr_repository" "images_repo" {
  name = local.ecr_repo_name
}

resource "aws_ecr_lifecycle_policy" "images_repo_policy" {
  repository = aws_ecr_repository.images_repo.name

  policy = <<EOF
{
    "rules": [
        {
            "rulePriority": 1,
            "description": "Keep last 5 images",
            "selection": {
                "tagStatus": "any",
                "countType": "imageCountMoreThan",
                "countNumber": 5
            },
            "action": {
                "type": "expire"
            }
        }
    ]
}
EOF
}

# Supporting resources
data "aws_caller_identity" "current" {}

# https://docs.aws.amazon.com/AmazonECS/latest/developerguide/ecs-optimized_AMI.html#ecs-optimized-ami-linux
data "aws_ssm_parameter" "ecs_optimized_ami" {
  name = "/aws/service/ecs/optimized-ami/amazon-linux-2/arm64/recommended"
}
