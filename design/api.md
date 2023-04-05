# Mimir API design

## Overview

| Endpoint                                     | Request                                                 | Response                   |
|----------------------------------------------|---------------------------------------------------------|----------------------------|
| `POST /v1/conversations`                     | [CreateConversationRequest](#createconversationrequest) | CreateConversationResponse |
| `GET /v1/conversations/{id:string}/messages` | N/A                                                     | [Message](#message)[]      |

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

### Message

| Field   | Type                | Comment |
|---------|---------------------|---------|
| Role    | 'user'\|'assistant' |         |
| Content | string              |         |
