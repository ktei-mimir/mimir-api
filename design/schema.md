# DynamoDB table schema

## Index design

### Primary key

| PK                            | SK                       |
|-------------------------------|--------------------------|
| CONVERSATION#[ConversationId] | CONVERSATION#[CreatedAt] |
| CONVERSATION#[ConversationId] | MESSAGE#[CreatedAt]      |

| GSI1PK                  | GSI1SK      |
|-------------------------|-------------|
| [CONVERSATION\|MESSAGE] | [CreatedAt] |
