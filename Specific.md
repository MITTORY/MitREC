﻿# **Введение**
В этом документа описаны все требования к приложению MitREC, не вошедшие в описание прецедентов.
# **Функциональность**
Имеющая отношение ко многим прецедентам
## **Регистрация событий и обработка ошибок**
Возможность регистрировать различные события, происходящие в приложении, такие как ошибки и предупреждения.
## **Подключаемые бизнес-правила**
Необходимо обеспечить возможность настройки системы для проработки различных сценариев.
## **Безопасность**
Гарантирование конфиденциальности пользовательских данных и защита их от утечек или несанкционированного использования.

Реализация механизмов безопасности для аутентификации пользователей.
# **Удобство использования**
## **Человеческие факторы**
1. Разработка пользовательского интерфейса, ориентированного на интуитивное понимание и легкость в использовании даже для новых пользователей.
2. Обеспечение высокой читаемости текста и доступности интерфейса для пользователей.

# **Производительность**
Для записи видео не требуется большого количества ресурсов компьютера.

# **Ограничения**
Для разработки рекомендуется использовать C# WPF в Visual Studio 2022, так как он предоставляет качественные библиотеки и упрощают процесс разработки.


# **Бесплатные компоненты на основе открытого кода**
Программа и используемые библиотеки являются бесплатными.

# **Интерфейсы**
## **Важные интерфейсы и аппаратные средства**
1. Обязательно: Компьютер
2. Необязательно: Средство записи звука, например, микрофон

## **Программные интерфейсы**
Для захвата звука: Взаимодействие с библиотекой NAudio
Для захвата экрана: Взаимодействие с библиотекой Accord

# **Бизнес-правила**
|**Имя**|**Правило**|**Возможность изменения**|**Источник**|
| :-: | :-: | :-: | :-: |
|Прав1|Бесплатное приложение, не включающее в себя внутренние покупки|Да, разработчик может изменять политику стоимости.|Разработчик|
|Прав1|Открытый код программы|Да, разработчик может изменять политику открытости кода.|Разработчик|