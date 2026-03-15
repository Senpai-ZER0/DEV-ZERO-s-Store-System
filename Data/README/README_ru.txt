ZERO's Store System — первичный каркас мода

Что включено:
- новый SubtypeId: NpcStoreBlock
- согласованные Data/CubeBlocks, EntityComponents, EntityContainers
- минимальный C#-скелет под новую архитектуру
- session, block logic, модели конфига, генератор и сервис регенерации

Что пока НЕ реализовано:
- реальная генерация offers/orders
- RHF
- команды администратора
- профили станций
- массовый реген всех NPC-магазинов
- расчет цен и работа с prefab-кораблями

Замечание:
скриптовый каркас написан как стартовая основа и не проверялся компиляцией в Space Engineers.

---
Step2 update:
- added TradeMode parser from CustomData
- added test generator with SteelPlate offer and Construction order
- added first regeneration flow and log output
- StoreBlockSynchronizer still contains placeholders for real store API write calls
