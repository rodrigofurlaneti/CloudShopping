| Tabela | Linha dump | Colunas | Referencia por FK |
|---|---:|---:|---|
| addresses | 27 | 13 | customers, addresstypes |
| addresstypes | 65 | 5 |  |
| cartitems | 93 | 8 | carts, products |
| carts | 126 | 6 | customers |
| companies | 155 | 7 | customers |
| contacts | 184 | 9 | customers |
| customers | 216 | 9 | tenants, customertypes |
| customertypes | 250 | 5 |  |
| departments | 278 | 8 | tenants |
| employees | 311 | 13 | tenants |
| employeeusers | 349 | 8 | tenants, employees |
| individuals | 383 | 7 | customers |
| orderaddresses | 412 | 11 | orders, addresstypes |
| orderitems | 447 | 8 | orders, products |
| orders | 480 | 9 | tenants, customers, orderstatus |
| ordersectors | 516 | 6 | tenants |
| orderstatehistory | 546 | 7 | orders, orderstatus |
| orderstatus | 578 | 8 | tenants, ordersectors |
| payments | 612 | 8 | orders, paymentstatus |
| paymentstatus | 645 | 5 |  |
| productimages | 673 | 9 | products |
| products | 706 | 17 | tenants, departments |
| profiles | 749 | 6 | tenants |
| profileusers | 779 | 7 | tenants, profiles, employeeusers |
| stockmovements | 814 | 7 | products |
| storebanners | 845 | 13 | tenants |
| tenants | 882 | 6 |  |
