- Order registra na base de dados e cria o evento - Created = 1,
- Quando PAYMENT receber uma ordem, ao registar na base de dados, criar um evento para ORDER saber que está processando - Processing = 2,
- Quando PAYMENT estiver OK, enviar para order - Completed = 3,

- Criar um projeto focado em delivery, aonde entregadores irão se registrar para fazer entregas.



    
    
    ReadyForDelivery = 4,
    DeliveryInProgress = 5,
    Delivered = 6,
    Finalized = 7,
    Cancelled = 90,
    CancelledByCustomer = 91,



    ------
    Criar sistema de OTP? Simulando push notification?