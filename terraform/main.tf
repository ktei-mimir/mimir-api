provider "aws" {
  region  = "ap-southeast-2"
  profile = "ktei2008"
}

terraform {
  backend "s3" {
    bucket  = "terraform-state-504224764639"
    key     = "mimir/infra.tfstate"
    region  = "ap-southeast-2"
    profile = "ktei2008"
  }
}

module "vpc" {
  source      = "./vpc"
  environment = "prod"
  vpc_name    = "disasterdev"
}

module "ecs" {
  source               = "./ecs"
  app_name             = "mimir"
  region               = "ap-southeast-2"
  environment          = "prod"
  vpc_id               = module.vpc.vpc_id
  subnets              = module.vpc.public_subnets
  task_definition_file = "./ecs/task-definition.json"
}

module "resources" {
  source      = "./resources"
  app_name    = "mimir"
  environment = "prod"
}
