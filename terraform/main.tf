# Terraform configuration and providers
terraform {
  required_version = ">= 1.6"
  
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.31"
    }
  }

  backend "s3" {
    bucket         = "codedaily-terraform-state"
    key            = "codedaily-api-test/terraform.tfstate"
    region         = "us-east-1"
    dynamodb_table = "terraform-state-lock"
    encrypt        = true
  }
}

# Variables
variable "region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

# Provider configuration
provider "aws" {
  region = var.region
}

# S3 Bucket for Templates
resource "aws_s3_bucket" "templates" {
  bucket = "codedaily-templates-test"
}

resource "aws_s3_bucket_versioning" "templates" {
  bucket = aws_s3_bucket.templates.id
  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "templates" {
  bucket = aws_s3_bucket.templates.id
  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

resource "aws_s3_bucket_public_access_block" "templates" {
  bucket = aws_s3_bucket.templates.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

# DynamoDB Table - Blogs
resource "aws_dynamodb_table" "blogs" {
  name         = "codedaily-blogs-test"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Status"
  range_key    = "PublishedDate"

  attribute {
    name = "Status"
    type = "S"
  }

  attribute {
    name = "PublishedDate"
    type = "S"
  }

  attribute {
    name = "Slug"
    type = "S"
  }

  attribute {
    name = "Author"
    type = "S"
  }

  attribute {
    name = "TagString"
    type = "S"
  }

  attribute {
    name = "Title"
    type = "S"
  }

  # GSI for slug lookup (single blog retrieval regardless of status)
  global_secondary_index {
    name               = "SlugIndex"
    hash_key           = "Slug"
    projection_type    = "ALL"
  }

  # LSI for searching within published blogs by tag
  local_secondary_index {
    name            = "PublishedTagIndex"
    range_key       = "TagString"
    projection_type = "ALL"
  }

  # LSI for searching within published blogs by title
  local_secondary_index {
    name            = "PublishedTitleIndex"
    range_key       = "Title"
    projection_type = "ALL"
  }

  # LSI for searching within draft blogs by author
  local_secondary_index {
    name            = "DraftAuthorIndex"
    range_key       = "Author"
    projection_type = "ALL"
  }

  # LSI for searching within draft blogs by title
  local_secondary_index {
    name            = "DraftTitleIndex"
    range_key       = "Title"
    projection_type = "ALL"
  }

  tags = {
    Project = "CodeDaily"
  }
}

# DynamoDB Table - Templates
resource "aws_dynamodb_table" "templates" {
  name         = "codedaily-templates-test"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "TemplateType"
  range_key    = "CreatedDate"

  attribute {
    name = "TemplateType"
    type = "S"
  }

  attribute {
    name = "CreatedDate"
    type = "S"
  }

  attribute {
    name = "Slug"
    type = "S"
  }

  global_secondary_index {
    name               = "SlugIndex"
    hash_key           = "Slug"
    projection_type    = "ALL"
  }

  tags = {
    Project = "CodeDaily"
  }
}

# IAM Role for Lambda
resource "aws_iam_role" "lambda_role" {
  name = "codedaily-api-lambda-role-test"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      }
    ]
  })

  tags = {
    Project = "CodeDaily"
  }
}

# IAM Policy for DynamoDB and S3 access
resource "aws_iam_policy" "lambda_policy" {
  name = "codedaily-api-lambda-policy-test"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "dynamodb:GetItem",
          "dynamodb:PutItem",
          "dynamodb:UpdateItem",
          "dynamodb:DeleteItem",
          "dynamodb:Query",
          "dynamodb:Scan"
        ]
        Resource = [
          aws_dynamodb_table.blogs.arn,
          "${aws_dynamodb_table.blogs.arn}/index/*",
          aws_dynamodb_table.templates.arn,
          "${aws_dynamodb_table.templates.arn}/index/*"
        ]
      },
      {
        Effect = "Allow"
        Action = [
          "s3:GetObject",
          "s3:PutObject",
          "s3:DeleteObject",
          "s3:ListBucket"
        ]
        Resource = [
          aws_s3_bucket.templates.arn,
          "${aws_s3_bucket.templates.arn}/*"
        ]
      }
    ]
  })

  tags = {
    Project = "CodeDaily"
  }
}

# Attach policies to Lambda role
resource "aws_iam_role_policy_attachment" "lambda_policy" {
  role       = aws_iam_role.lambda_role.name
  policy_arn = aws_iam_policy.lambda_policy.arn
}

resource "aws_iam_role_policy_attachment" "lambda_basic_execution" {
  role       = aws_iam_role.lambda_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

# Lambda Function
resource "aws_lambda_function" "api" {
  function_name = "CodeDaily-API-Test"
  role          = aws_iam_role.lambda_role.arn
  handler       = "CodeDaily.API::CodeDaily.API.LambdaEntryPoint::FunctionHandlerAsync"
  runtime       = "dotnet8"
  timeout       = 30
  memory_size   = 512

  # Placeholder zip - will be updated by CI/CD
  filename         = "placeholder.zip"
  source_code_hash = data.archive_file.placeholder.output_base64sha256

  environment {
    variables = {
      BLOGS_TABLE_NAME      = aws_dynamodb_table.blogs.name
      TEMPLATES_TABLE_NAME  = aws_dynamodb_table.templates.name
      TEMPLATES_BUCKET_NAME = aws_s3_bucket.templates.bucket
      AWS_REGION            = var.region
    }
  }

  depends_on = [
    aws_iam_role_policy_attachment.lambda_policy,
    aws_iam_role_policy_attachment.lambda_basic_execution
  ]

  tags = {
    Project = "CodeDaily"
  }
}

# Placeholder zip file for initial deployment
data "archive_file" "placeholder" {
  type        = "zip"
  output_path = "placeholder.zip"
  source {
    content  = "placeholder"
    filename = "placeholder.txt"
  }
}

# API Gateway
resource "aws_api_gateway_rest_api" "api" {
  name        = "codedaily-api-test"
  description = "CodeDaily API Gateway - Test"

  endpoint_configuration {
    types = ["REGIONAL"]
  }

  tags = {
    Project = "CodeDaily"
  }
}

resource "aws_api_gateway_resource" "proxy" {
  rest_api_id = aws_api_gateway_rest_api.api.id
  parent_id   = aws_api_gateway_rest_api.api.root_resource_id
  path_part   = "{proxy+}"
}

resource "aws_api_gateway_method" "proxy" {
  rest_api_id   = aws_api_gateway_rest_api.api.id
  resource_id   = aws_api_gateway_resource.proxy.id
  http_method   = "ANY"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "lambda" {
  rest_api_id = aws_api_gateway_rest_api.api.id
  resource_id = aws_api_gateway_method.proxy.resource_id
  http_method = aws_api_gateway_method.proxy.http_method

  integration_http_method = "POST"
  type                   = "AWS_PROXY"
  uri                    = aws_lambda_function.api.invoke_arn
}

resource "aws_api_gateway_method" "proxy_root" {
  rest_api_id   = aws_api_gateway_rest_api.api.id
  resource_id   = aws_api_gateway_rest_api.api.root_resource_id
  http_method   = "ANY"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "lambda_root" {
  rest_api_id = aws_api_gateway_rest_api.api.id
  resource_id = aws_api_gateway_method.proxy_root.resource_id
  http_method = aws_api_gateway_method.proxy_root.http_method

  integration_http_method = "POST"
  type                   = "AWS_PROXY"
  uri                    = aws_lambda_function.api.invoke_arn
}

resource "aws_api_gateway_deployment" "api" {
  depends_on = [
    aws_api_gateway_integration.lambda,
    aws_api_gateway_integration.lambda_root,
  ]

  rest_api_id = aws_api_gateway_rest_api.api.id
  stage_name  = "prod"

  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_lambda_permission" "api_gw" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.api.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_api_gateway_rest_api.api.execution_arn}/*/*"
}

# Outputs
output "lambda_function_name" {
  description = "Name of the Lambda function"
  value       = aws_lambda_function.api.function_name
}

output "api_gateway_url" {
  description = "Base URL for API Gateway"
  value       = "${aws_api_gateway_deployment.api.invoke_url}"
}

output "blogs_table_name" {
  description = "Name of the blogs DynamoDB table"
  value       = aws_dynamodb_table.blogs.name
}

output "templates_table_name" {
  description = "Name of the templates DynamoDB table"
  value       = aws_dynamodb_table.templates.name
}

output "templates_bucket_name" {
  description = "Name of the S3 templates bucket"
  value       = aws_s3_bucket.templates.bucket
}