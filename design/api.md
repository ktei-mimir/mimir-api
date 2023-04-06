# Mimir API design

## Overview

| Endpoint                                      | Request                                                 | Response                              |
|-----------------------------------------------|---------------------------------------------------------|---------------------------------------|
| `POST /v1/conversations`                      | [CreateConversationRequest](#createconversationrequest) | CreateConversationResponse            |
| `GET /v1/conversations`                       | N/A                                                     | [ConversationDto](#conversationdto)[] |
| `GET /v1/conversations/{id:string}/messages`  | N/A                                                     | [MessageDto](#messagedto)[]           |
| `POST /v1/conversations/{id:string}/messages` | [CreateMessageRequest](#createmessagerequest)           | [MessageDto](#messagedto)             |

## Types

### CreateConversationRequest

| Field    | Type     | Comment |
|----------|----------|---------|
| Messages | string[] |         |

### CreateConversationResponse

| Field       | Type                                            | Comment                         |
|-------------|-------------------------------------------------|---------------------------------|
| Id          | string                                          |                                 |
| Choices     | [ChatCompletionChoice](#chatcompletionchoice)[] |                                 |
| TotalTokens | int                                             | total number of tokens consumed |

### ChatCompletionChoice

| Field        | Type                  | Comment |
|--------------|-----------------------|---------|
| Message      | [Message](#message)[] |         |
| FinishReason | string                |         |
| Index        | int                   |         |

### ConversationDto

| Field | Type   | Comment |
|-------|--------|---------|
| Id    | string |         |
| Title | string |         |

### Message

| Field   | Type                | Comment |
|---------|---------------------|---------|
| Role    | 'user'\|'assistant' |         |
| Content | string              |         |

### CreateMessageRequest
| Field          | Type   | Comment |
|----------------|--------|---------|
| ConversationId | string |         |
| Content        | string |         |

### MessageDto

| Field   | Type                | Comment |
|---------|---------------------|---------|
| Role    | 'user'\|'assistant' |         |
| Content | string              |         |