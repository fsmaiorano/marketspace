| Microservices | Local Env   | Docker Env  | Docker Inside |
|---------------|-------------|-------------|---------------|
| Catalog       | 5000 - 5050 | 6000 - 6060 | 8080 - 8081   |
| Basket        | 5001 - 5051 | 6001 - 6061 | 8080 - 8081   |
| Discount      | 5002 - 5052 | 6002 - 6062 | 8080 - 8081   |
| Ordering      | 5003 - 5053 | 6003 - 6063 | 8080 - 8081   |
| Gateway       | 5004 - 5054 | 6004 - 6064 | 8080 - 8081   |
| Merchant      | 5005 - 5055 | 6005 - 6065 | 8080 - 8081   |
| keycloak      | xxxx - xxxx | 7005 - xxxx | 8080 - xxxx   |
| WebApp        | 3001 - 3041 | 4001 - 4041 | 8080 - xxxx   |
| BFF           | 4001 - 4041 | 5001 - 5051 | 8080 - xxxx   |


| Others        | Local Env | Docker Env      | Docker Inside |
|---------------|-----------|-----------------|---------------|
| CatalogDb     | 5432      | 5432            | 5432          |
| BasketDb      | 5433      | 5433            | 5433          |
| DiscountDb    | Sqlite    | Sqlite          | Sqlite        |
| OrderingDb    | 5435      | 5435            | 5435          |
| MerchantDb    | 5436      | 5436            | 5436          |
| KeycloakDb    | 5434      | 5434            | 5434          |
| Redis         | 6379      | 6379            | 6379          |
| RabbitMQ      | 5672      | 5672            | 5672          |
| Minio         | 9000      | 9000            | 9000          |
| Minio Console | 9001      | 9001            | 9001          |
| Grafana       | 3000      | 3000            | 3000          |
| Prometheus    | 9090      | 9090            | 9090          |
| Jaeger        | 16686     | 16686           | 16686         |
| Loki Api      | 3100      | 3100            | 3100          |