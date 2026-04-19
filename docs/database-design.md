# Проектирование базы данных

База данных проекта **Household Services** предназначена для хранения информации о пользователях платформы, их ролях, категориях услуг, заявках клиентов, откликах мастеров, заказах и отзывах.

Модель построена вокруг основного бизнес-процесса платформы:

**request → response → order → review**

Это разделение позволяет хранить разные стадии взаимодействия клиента и мастера по отдельности:

- **request** — клиент создаёт заявку;
- **response** — мастер откликается на заявку;
- **order** — после принятия отклика создаётся заказ;
- **review** — после завершения заказа клиент может оставить отзыв.

Такой подход позволяет не смешивать в одной таблице данные разных этапов процесса и делает модель более понятной и нормализованной.

## Состав реквизитов сущностей

1. **Users**  
   `(user_id (pk), login (unique), password_hash, first_name, last_name, phone, created_at)`

2. **Roles**  
   `(role_id (pk), name (unique))`

3. **User_roles**  
   `(user_id (pk, fk), role_id (pk, fk))`

4. **Service_categories**  
   `(category_id (pk), name (unique), description)`

5. **Master_profiles**  
   `(user_id (pk, fk), description, experience_years)`

6. **Master_categories**  
   `(user_id (pk, fk), category_id (pk, fk))`

7. **Requests**  
   `(request_id (pk), client_id (fk), category_id (fk), request_status_id (fk), title, description, address, desired_date, created_at)`

8. **Request_statuses**  
   `(request_status_id (pk), name (unique))`

9. **Responses**  
   `(response_id (pk), request_id (fk), master_id (fk), response_status_id (fk), proposed_price, comment, created_at)`

10. **Response_statuses**  
    `(response_status_id (pk), name (unique))`

11. **Order_statuses**  
    `(order_status_id (pk), name (unique))`

12. **Orders**  
    `(order_id (pk), response_id (fk), order_status_id (fk), price, created_at, completed_at)`

13. **Reviews**  
    `(review_id (pk), order_id (fk, unique), rating, comment, created_at)`

## Пояснения к таблицам

1. **users** — учетные записи всех пользователей системы.

2. **roles** — типы пользователей в системе: клиент, мастер, администратор.

3. **user_roles** — назначение ролей конкретным пользователям, таблица связи между пользователями и ролями.

4. **service_categories** — категории бытовых услуг. Для каждой категории хранится уникальное имя и описание.

5. **master_profiles** — профессиональные профили мастеров.  
   Поле `description` содержит текстовый раздел «О себе», поле `experience_years` — количество лет опыта работы.

6. **master_categories** — специализации мастеров по категориям услуг.  
   Таблица показывает, какими категориями услуг владеет мастер. Поле `user_id` должно ссылаться только на пользователя с ролью мастера. Это ограничение планируется обеспечивать триггерной функцией.

7. **requests** — заявки клиентов на выполнение работ.  
   Таблица хранит информацию о заявке: автора, категорию услуги, статус, заголовок, описание, адрес, желаемую дату выполнения и дату создания.

8. **request_statuses** — возможные статусы заявок.  
   Например: `open`, `in_progress`, `completed`, `cancelled`.

9. **responses** — отклики мастеров на клиентские заявки.  
   Таблица хранит отклики мастеров, включая заявку, мастера, статус отклика, предложенную цену, комментарий и дату создания.

10. **response_statuses** — возможные статусы откликов мастеров.  
    Например: `pending`, `accepted`, `rejected`.

11. **orders** — согласованные заказы между клиентом и мастером.  
    Заказ создаётся на основе выбранного отклика мастера.

    Логика следующая:
    - клиент создаёт **request**;
    - мастер оставляет **response**;
    - клиент выбирает один из откликов;
    - на основе выбранного отклика создаётся **order**.

12. **order_statuses** — возможные статусы заказов.  
    Например: `created`, `in_progress`, `completed`, `cancelled`.

13. **reviews** — отзывы клиентов по завершённым услугам.  
    Отзыв привязывается к заказу и содержит оценку, комментарий и дату создания.

## IDEF1X-диаграмма

![IDEF1X diagram](idef1x.jpg)