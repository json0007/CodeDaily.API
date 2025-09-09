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
    bucket         = "codedaily-terraform-state-east-2"
    key            = "codedaily-api/terraform.tfstate"
    region         = "us-east-2"
    dynamodb_table = "terraform-state-lock"
    encrypt        = true
  }
}

# Variables
variable "region" {
  description = "AWS region"
  type        = string
  default     = "us-east-2"
}

# Provider configuration
provider "aws" {
  region = var.region
}

# S3 Bucket for Templates
resource "aws_s3_bucket" "templates" {
  bucket = "codedaily-templates"
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

# DynamoDB Table - Single Table Design for Blogs
#
# Main Table Access Patterns:
# - PK = "BLOG#slug", SK = "BLOG#date" → Blog metadata
# - PK = "BLOG#slug", SK = "TAG#tagname" → Individual tag items for each blog
#
# GSI1 Access Patterns (Status):
# - GSI1_PK = "STATUS#published", GSI1_SK = "date" → All published blogs by date
# - GSI1_PK = "STATUS#draft", GSI1_SK = "date" → All draft blogs by date
#
# GSI2 Access Patterns (Author):
# - GSI2_PK = "AUTHOR#author-name", GSI2_SK = "date" → All blogs by specific author
#
# GSI3 Access Patterns (Tags):
# - GSI3_PK = "TAG#javascript", GSI3_SK = "BLOG#date" → All blogs with specific tag
#
resource "aws_dynamodb_table" "blogs" {
  name         = "codedaily-blogs"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "PK"    # Partition key - BLOG#slug
  range_key    = "SK"    # Sort key - BLOG#date or TAG#tagname

  # Base table attributes
  attribute {
    name = "PK"          # Primary partition key (BLOG#slug)
    type = "S"
  }

  attribute {
    name = "SK"          # Primary sort key (BLOG#date or TAG#tagname)
    type = "S"
  }

  # GSI1 attributes - for status-based queries (published/draft)
  attribute {
    name = "GSI1_PK"     # STATUS
    type = "S"
  }

  attribute {
    name = "GSI1_SK"     # PublishedDate or CreatedDate
    type = "S"
  }

  # GSI2 attributes - for author-based queries
  attribute {
    name = "GSI2_PK"     # AUTHOR#author-name
    type = "S"
  }

  attribute {
    name = "GSI2_SK"     # PublishedDate or CreatedDate for sorting
    type = "S"
  }

  # GSI3 attributes - for title-based queries
  attribute {
    name = "GSI3_PK"     # TITLE#title
    type = "S"
  }

  attribute {
    name = "GSI3_SK"     # PublishedDate or CreatedDate for sorting
    type = "S"
  }

  # GSI4 attributes - for tag-based queries
  attribute {
    name = "GSI4_PK"     # TAG#tagname
    type = "S"
  }

  attribute {
    name = "GSI4_SK"     # BLOG#date for sorting (from BlogTag items)
    type = "S"
  }

  # GSI1 - Query blogs by status (published/draft), sorted by date
  # Excludes Content to optimize for list queries
  global_secondary_index {
    name               = "StatusDateIndex"
    hash_key           = "GSI1_PK"     # STATUS#published
    range_key          = "GSI1_SK"     # PublishedDate or CreatedDate
    projection_type    = "INCLUDE"     # Include specific attributes (not Content)
    non_key_attributes = ["Slug", "Title", "Description", "Author", "Status", "CreatedDate", "PublishedDate", "Tags", "IsFeatured", "ReadTime"]
  }

  # GSI2 - Query blogs by author, sorted by date
  # Excludes Content to optimize for list queries
  global_secondary_index {
    name               = "AuthorDateIndex"
    hash_key           = "GSI2_PK"     # AUTHOR#john-doe
    range_key          = "GSI2_SK"     # PublishedDate or CreatedDate
    projection_type    = "INCLUDE"     # Include specific attributes (not Content)
    non_key_attributes = ["Slug", "Title", "Description", "Author", "Status", "CreatedDate", "PublishedDate", "Tags", "IsFeatured", "ReadTime"]
  }

  # GSI3 - Query blogs by author, sorted by date
  # Excludes Content to optimize for list queries
  global_secondary_index {
    name               = "TitleDateIndex"
    hash_key           = "GSI3_PK"     # TITLE#some-title
    range_key          = "GSI3_SK"     # PublishedDate or CreatedDate
    projection_type    = "INCLUDE"     # Include specific attributes (not Content)
    non_key_attributes = ["Slug", "Title", "Description", "Author", "Status", "CreatedDate", "PublishedDate", "Tags", "IsFeatured", "ReadTime"]
  }

  # GSI4 - Query blogs by tag, sorted by date
  # Only needs minimal attributes since BlogTag items are lightweight
  global_secondary_index {
    name               = "TagDateIndex"
    hash_key           = "GSI4_PK"     # TAG#javascript
    range_key          = "GSI4_SK"     # BLOG#date
    projection_type    = "KEYS_ONLY"   # BlogTag items only need the keys
  }

  tags = {
    Project = "CodeDaily"
  }
}

# IAM Role for Lambda
resource "aws_iam_role" "lambda_role" {
  name = "codedaily-api-lambda-role"

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
  name = "codedaily-api-lambda-policy"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "dynamodb:DescribeTable",
          "dynamodb:GetItem",
          "dynamodb:PutItem",
          "dynamodb:UpdateItem",
          "dynamodb:DeleteItem",
          "dynamodb:Query",
          "dynamodb:Scan"
        ]
        Resource = [
          aws_dynamodb_table.blogs.arn,
          "${aws_dynamodb_table.blogs.arn}/index/*"
        ]
      },
      {
        Effect = "Allow"
        Action = [
          "s3:GetObject",
          "s3:PutItem",
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
  function_name = "CodeDaily-API"
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
      TEMPLATES_BUCKET_NAME = aws_s3_bucket.templates.bucket
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
  name        = "codedaily-api"
  description = "CodeDaily API Gateway"

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

# Enable CORS for browser access
resource "aws_api_gateway_method" "options" {
  rest_api_id   = aws_api_gateway_rest_api.api.id
  resource_id   = aws_api_gateway_resource.proxy.id
  http_method   = "OPTIONS"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "options" {
  rest_api_id = aws_api_gateway_rest_api.api.id
  resource_id = aws_api_gateway_method.options.resource_id
  http_method = aws_api_gateway_method.options.http_method
  type        = "MOCK"
  
  request_templates = {
    "application/json" = "{\"statusCode\": 200}"
  }
}

resource "aws_api_gateway_method_response" "options" {
  rest_api_id = aws_api_gateway_rest_api.api.id
  resource_id = aws_api_gateway_resource.proxy.id
  http_method = aws_api_gateway_method.options.http_method
  status_code = "200"
  
  response_parameters = {
    "method.response.header.Access-Control-Allow-Headers" = true
    "method.response.header.Access-Control-Allow-Methods" = true
    "method.response.header.Access-Control-Allow-Origin"  = true
  }
}

resource "aws_api_gateway_integration_response" "options" {
  rest_api_id = aws_api_gateway_rest_api.api.id
  resource_id = aws_api_gateway_resource.proxy.id
  http_method = aws_api_gateway_method.options.http_method
  status_code = aws_api_gateway_method_response.options.status_code
  
  response_parameters = {
    "method.response.header.Access-Control-Allow-Headers" = "'Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token'"
    "method.response.header.Access-Control-Allow-Methods" = "'GET,OPTIONS,POST,PUT,DELETE'"
    "method.response.header.Access-Control-Allow-Origin"  = "'*'"
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

output "templates_bucket_name" {
  description = "Name of the S3 templates bucket"
  value       = aws_s3_bucket.templates.bucket
}