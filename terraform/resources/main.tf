locals {
  table_name = "${var.app_name}-${var.environment}"
}


module "dynamodb_table" {
  source = "terraform-aws-modules/dynamodb-table/aws"

  name      = local.table_name
  hash_key  = "PK"
  range_key = "SK"

  attributes = [
    {
      name = "PK"
      type = "S"
    },
    {
      name = "SK"
      type = "S"
    },
    {
      name = "GSI1PK"
      type = "S"
    },
    {
      name = "GSI1SK"
      type = "S"
    }
  ]
  global_secondary_indexes = [
    {
      name            = "GSI1"
      hash_key        = "GSI1PK"
      range_key       = "GSI1SK"
      projection_type = "ALL"
    }
  ]

  tags = {
    Environment = var.environment
  }
}
