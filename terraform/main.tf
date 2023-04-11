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
  vpc_name    = "mimir"
}
