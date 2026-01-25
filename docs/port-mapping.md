| Microservices | Local Env   | Aspire/Docker Env | Docker Inside |
|---------------|-------------|-------------------|---------------|
| Catalog       | 5000 - 5050 | 6000 - 6060       | 8080 - 8081   |
| Basket        | 5001 - 5051 | 6001 - 6061       | 8080 - 8081   |
| Discount      | 5002 - 5052 | 6002 - 6062       | 8080 - 8081   |
| Ordering      | 5003 - 5053 | 6003 - 6063       | 8080 - 8081   |
| Gateway       | 5004 - 5054 | 6004 - 6064       | 8080 - 8081   |
| Merchant      | 5005 - 5055 | 6005 - 6065       | 8080 - 8081   |
| User          | 5006 - 5066 | 6006 - 6066       | 8080 - 8081   |
| WebApp        | 3000 - xxxx | 80 + 4001         | 4000          |
| BFF           | 4000 - xxxx | 5100 - 5150       | 8080 - 8081   |


| Others        | Local Env | Docker Env      | Docker Inside |
|---------------|-----------|-----------------|---------------|
| CatalogDb     | 5432      | 5432            | 5432          |
| BasketDb      | 5433      | 5433            | 5433          |
| DiscountDb    | Sqlite    | Sqlite          | Sqlite        |
| OrderingDb    | 5435      | 5435            | 5435          |
| MerchantDb    | 5436      | 5436            | 5436          |
| UserDb        | 5437      | 5437            | 5437          |
| RabbitMQ      | 5672      | 5672            | 5672          |
| RabbitMQ Mgmt | 15672     | 15672           | 15672         |
| Minio         | 9000      | 9000            | 9000          |
| Minio Console | 9001      | 9001            | 9001          |
