--
-- PostgreSQL database dump
--

\restrict S8Q7oxpFIab9ZztfcnCf1JDxBTFIZqdEiPveoaLfaPyqiTN61yBoQyfg3jEcaEs

-- Dumped from database version 16.11 (Debian 16.11-1.pgdg13+1)
-- Dumped by pg_dump version 16.11 (Debian 16.11-1.pgdg13+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: create_order_after_response_accepted(); Type: FUNCTION; Schema: public; Owner: household_user
--

CREATE FUNCTION public.create_order_after_response_accepted() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE 
   accepted_status_id integer;
   in_progress_status_id integer;
BEGIN
   SELECT response_status_id
   INTO accepted_status_id
   FROM response_statuses
   WHERE name = 'accepted';
   
   SELECT order_status_id
   INTO in_progress_status_id
   FROM order_statuses
   WHERE name = 'in_progress';
   
   IF OLD.response_status_id <> NEW.response_status_id AND NEW.response_status_id = accepted_status_id
   THEN
       INSERT INTO orders(
       response_id,
       order_status_id,
       price,
       initial_meeting_at,
       created_at,
       completed_at)
       
       VALUES (
       NEW.response_id,
       in_progress_status_id,
       NEW.proposed_price,
       NULL,
       NOW(),
       NULL);
   END IF;
   RETURN NEW;
   
END;
$$;


ALTER FUNCTION public.create_order_after_response_accepted() OWNER TO household_user;

--
-- Name: ensure_review_order_completed(); Type: FUNCTION; Schema: public; Owner: household_user
--

CREATE FUNCTION public.ensure_review_order_completed() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
   order_status_name text;
BEGIN
   SELECT os.name
   INTO order_status_name
   FROM orders o
   JOIN order_statuses os ON os.order_status_id = o.order_status_id
   WHERE o.order_id = NEW.order_id;

   IF order_status_name IS NULL
   THEN
       RAISE EXCEPTION 'Order % does not exist.', NEW.order_id
           USING ERRCODE = 'foreign_key_violation';
   END IF;

   IF order_status_name <> 'completed'
   THEN
       RAISE EXCEPTION 'Review can be created only for completed order %. Current status is %.', NEW.order_id, order_status_name
           USING ERRCODE = 'check_violation';
   END IF;

   RETURN NEW;
END;
$$;


ALTER FUNCTION public.ensure_review_order_completed() OWNER TO household_user;

--
-- Name: prevent_duplicate_review_per_order(); Type: FUNCTION; Schema: public; Owner: household_user
--

CREATE FUNCTION public.prevent_duplicate_review_per_order() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
   IF EXISTS (
       SELECT 1
       FROM reviews
       WHERE order_id = NEW.order_id
   )
   THEN
       RAISE EXCEPTION 'Review for order % already exists.', NEW.order_id
           USING ERRCODE = 'unique_violation';
   END IF;

   RETURN NEW;
END;
$$;


ALTER FUNCTION public.prevent_duplicate_review_per_order() OWNER TO household_user;

--
-- Name: reject_responses_when_one_accepted(); Type: FUNCTION; Schema: public; Owner: household_user
--

CREATE FUNCTION public.reject_responses_when_one_accepted() RETURNS trigger
    LANGUAGE plpgsql
    AS $$

DECLARE
   accepted_status_id INT;
   rejected_status_id INT;
   pending_status_id INT;
BEGIN
SELECT response_status_id
INTO accepted_status_id
FROM response_statuses
WHERE name = 'accepted';

SELECT response_status_id
INTO rejected_status_id
FROM response_statuses
WHERE name = 'rejected';

SELECT response_status_id
INTO pending_status_id
FROM response_statuses
WHERE name = 'pending';

IF NEW.response_status_id = accepted_status_id THEN
   UPDATE responses
   SET response_status_id = rejected_status_id
   WHERE request_id = NEW.request_id
   AND response_id <> NEW.response_id
   AND response_status_id = pending_status_id;
END IF;
RETURN NEW;
END;
$$;


ALTER FUNCTION public.reject_responses_when_one_accepted() OWNER TO household_user;

--
-- Name: set_order_completed_at_when_completed(); Type: FUNCTION; Schema: public; Owner: household_user
--

CREATE FUNCTION public.set_order_completed_at_when_completed() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    completed_status_id INT;
BEGIN
    SELECT order_status_id
    INTO completed_status_id
    FROM order_statuses
    WHERE name = 'completed';

    IF NEW.order_status_id = completed_status_id THEN
        NEW.completed_at = NOW();
    END IF;

    RETURN NEW;
END;
$$;


ALTER FUNCTION public.set_order_completed_at_when_completed() OWNER TO household_user;

--
-- Name: set_request_in_progress_when_response_accepted(); Type: FUNCTION; Schema: public; Owner: household_user
--

CREATE FUNCTION public.set_request_in_progress_when_response_accepted() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    accepted_status_id INT;
    in_progress_status_id INT;
BEGIN
    SELECT response_status_id
    INTO accepted_status_id
    FROM response_statuses
    WHERE name = 'accepted';

    SELECT request_status_id
    INTO in_progress_status_id
    FROM request_statuses
    WHERE name = 'in_progress';

    IF NEW.response_status_id = accepted_status_id THEN
        UPDATE requests
        SET request_status_id = in_progress_status_id
        WHERE request_id = NEW.request_id;
    END IF;

    RETURN NEW;
END;
$$;


ALTER FUNCTION public.set_request_in_progress_when_response_accepted() OWNER TO household_user;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: household_user
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO household_user;

--
-- Name: master_categories; Type: TABLE; Schema: public; Owner: household_user
--

CREATE TABLE public.master_categories (
    user_id integer NOT NULL,
    category_id integer NOT NULL
);


ALTER TABLE public.master_categories OWNER TO household_user;

--
-- Name: master_profiles; Type: TABLE; Schema: public; Owner: household_user
--

CREATE TABLE public.master_profiles (
    user_id integer NOT NULL,
    description character varying(2000) NOT NULL,
    experience_years integer NOT NULL
);


ALTER TABLE public.master_profiles OWNER TO household_user;

--
-- Name: orders; Type: TABLE; Schema: public; Owner: household_user
--

CREATE TABLE public.orders (
    order_id integer NOT NULL,
    response_id integer NOT NULL,
    order_status_id integer NOT NULL,
    price numeric(10,2) NOT NULL,
    initial_meeting_at timestamp with time zone,
    created_at timestamp with time zone NOT NULL,
    completed_at timestamp with time zone
);


ALTER TABLE public.orders OWNER TO household_user;

--
-- Name: requests; Type: TABLE; Schema: public; Owner: household_user
--

CREATE TABLE public.requests (
    request_id integer NOT NULL,
    client_id integer NOT NULL,
    category_id integer NOT NULL,
    request_status_id integer NOT NULL,
    title character varying(200) NOT NULL,
    description character varying(2000) NOT NULL,
    address character varying(300) NOT NULL,
    desired_date timestamp with time zone NOT NULL,
    created_at timestamp with time zone NOT NULL
);


ALTER TABLE public.requests OWNER TO household_user;

--
-- Name: responses; Type: TABLE; Schema: public; Owner: household_user
--

CREATE TABLE public.responses (
    response_id integer NOT NULL,
    request_id integer NOT NULL,
    master_id integer NOT NULL,
    response_status_id integer NOT NULL,
    proposed_price numeric(10,2) NOT NULL,
    comment character varying(1000),
    created_at timestamp with time zone NOT NULL
);


ALTER TABLE public.responses OWNER TO household_user;

--
-- Name: reviews; Type: TABLE; Schema: public; Owner: household_user
--

CREATE TABLE public.reviews (
    review_id integer NOT NULL,
    order_id integer NOT NULL,
    rating integer NOT NULL,
    comment character varying(1000),
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT "CK_reviews_rating_range" CHECK (((rating >= 1) AND (rating <= 5)))
);


ALTER TABLE public.reviews OWNER TO household_user;

--
-- Name: service_categories; Type: TABLE; Schema: public; Owner: household_user
--

CREATE TABLE public.service_categories (
    category_id integer NOT NULL,
    name character varying(100) NOT NULL,
    description character varying(500)
);


ALTER TABLE public.service_categories OWNER TO household_user;

--
-- Name: users; Type: TABLE; Schema: public; Owner: household_user
--

CREATE TABLE public.users (
    user_id integer NOT NULL,
    login character varying(50) NOT NULL,
    password_hash character varying(255) NOT NULL,
    first_name character varying(100) NOT NULL,
    last_name character varying(100) NOT NULL,
    phone character varying(20) NOT NULL,
    created_at timestamp with time zone NOT NULL
);


ALTER TABLE public.users OWNER TO household_user;

--
-- Name: master_reviews_view; Type: VIEW; Schema: public; Owner: household_user
--

CREATE VIEW public.master_reviews_view AS
 SELECT rev.review_id,
    rev.order_id,
    rev.rating,
    rev.comment,
    rev.created_at AS review_created_at,
    o.completed_at AS order_completed_at,
    req.request_id,
    req.title AS request_title,
    req.category_id,
    sc.name AS category_name,
    req.client_id,
    client.first_name AS client_first_name,
    client.last_name AS client_last_name,
    r.master_id,
    master.first_name AS master_first_name,
    master.last_name AS master_last_name
   FROM ((((((public.reviews rev
     JOIN public.orders o ON ((o.order_id = rev.order_id)))
     JOIN public.responses r ON ((r.response_id = o.response_id)))
     JOIN public.requests req ON ((req.request_id = r.request_id)))
     JOIN public.service_categories sc ON ((sc.category_id = req.category_id)))
     JOIN public.users client ON ((client.user_id = req.client_id)))
     JOIN public.users master ON ((master.user_id = r.master_id)));


ALTER VIEW public.master_reviews_view OWNER TO household_user;

--
-- Name: order_statuses; Type: TABLE; Schema: public; Owner: household_user
--

CREATE TABLE public.order_statuses (
    order_status_id integer NOT NULL,
    name character varying(50) NOT NULL
);


ALTER TABLE public.order_statuses OWNER TO household_user;

--
-- Name: order_details_view; Type: VIEW; Schema: public; Owner: household_user
--

CREATE VIEW public.order_details_view AS
 SELECT o.order_id,
    os.name AS status,
    o.price,
    o.initial_meeting_at,
    o.created_at,
    o.completed_at,
    req.request_id,
    req.title AS request_title,
    req.description AS request_description,
    req.address AS request_address,
    req.desired_date,
    req.category_id,
    req.client_id,
    sc.name AS category_name,
    client.first_name AS client_first_name,
    client.last_name AS client_last_name,
    client.phone AS client_phone,
    r.master_id,
    master.first_name AS master_first_name,
    master.last_name AS master_last_name,
    master.phone AS master_phone
   FROM ((((((public.orders o
     JOIN public.order_statuses os ON ((o.order_status_id = os.order_status_id)))
     JOIN public.responses r ON ((r.response_id = o.response_id)))
     JOIN public.requests req ON ((req.request_id = r.request_id)))
     JOIN public.service_categories sc ON ((req.category_id = sc.category_id)))
     JOIN public.users client ON ((req.client_id = client.user_id)))
     JOIN public.users master ON ((r.master_id = master.user_id)));


ALTER VIEW public.order_details_view OWNER TO household_user;

--
-- Name: order_statuses_order_status_id_seq; Type: SEQUENCE; Schema: public; Owner: household_user
--

ALTER TABLE public.order_statuses ALTER COLUMN order_status_id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.order_statuses_order_status_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: orders_order_id_seq; Type: SEQUENCE; Schema: public; Owner: household_user
--

ALTER TABLE public.orders ALTER COLUMN order_id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.orders_order_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: request_statuses; Type: TABLE; Schema: public; Owner: household_user
--

CREATE TABLE public.request_statuses (
    request_status_id integer NOT NULL,
    name character varying(50) NOT NULL
);


ALTER TABLE public.request_statuses OWNER TO household_user;

--
-- Name: request_statuses_request_status_id_seq; Type: SEQUENCE; Schema: public; Owner: household_user
--

ALTER TABLE public.request_statuses ALTER COLUMN request_status_id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.request_statuses_request_status_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: requests_request_id_seq; Type: SEQUENCE; Schema: public; Owner: household_user
--

ALTER TABLE public.requests ALTER COLUMN request_id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.requests_request_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: response_statuses; Type: TABLE; Schema: public; Owner: household_user
--

CREATE TABLE public.response_statuses (
    response_status_id integer NOT NULL,
    name character varying(50) NOT NULL
);


ALTER TABLE public.response_statuses OWNER TO household_user;

--
-- Name: response_statuses_response_status_id_seq; Type: SEQUENCE; Schema: public; Owner: household_user
--

ALTER TABLE public.response_statuses ALTER COLUMN response_status_id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.response_statuses_response_status_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: responses_response_id_seq; Type: SEQUENCE; Schema: public; Owner: household_user
--

ALTER TABLE public.responses ALTER COLUMN response_id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.responses_response_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: reviews_review_id_seq; Type: SEQUENCE; Schema: public; Owner: household_user
--

ALTER TABLE public.reviews ALTER COLUMN review_id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.reviews_review_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: roles; Type: TABLE; Schema: public; Owner: household_user
--

CREATE TABLE public.roles (
    role_id integer NOT NULL,
    name character varying(50) NOT NULL
);


ALTER TABLE public.roles OWNER TO household_user;

--
-- Name: roles_RoleId_seq; Type: SEQUENCE; Schema: public; Owner: household_user
--

ALTER TABLE public.roles ALTER COLUMN role_id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."roles_RoleId_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: service_categories_category_id_seq; Type: SEQUENCE; Schema: public; Owner: household_user
--

ALTER TABLE public.service_categories ALTER COLUMN category_id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.service_categories_category_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: user_roles; Type: TABLE; Schema: public; Owner: household_user
--

CREATE TABLE public.user_roles (
    user_id integer NOT NULL,
    role_id integer NOT NULL
);


ALTER TABLE public.user_roles OWNER TO household_user;

--
-- Name: users_user_id_seq; Type: SEQUENCE; Schema: public; Owner: household_user
--

ALTER TABLE public.users ALTER COLUMN user_id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.users_user_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: household_user
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20260421201358_InitialCreate	10.0.7
20260422165718_AddLookupEntities	10.0.7
20260424104518_AddCoreEntities	10.0.7
20260512114841_SyncModelChanges	10.0.7
20260513122529_SeedLookupData	10.0.7
20260518130530_RemoveCreatedOrderStatus	10.0.7
20260522134959_AddCancelledResponseStatus	10.0.7
20260531150015_AddResponseAcceptedOrderTrigger	10.0.7
20260601105058_AddOrdersView	10.0.7
20260601202423_MakeInitialMeetingAtNullable	10.0.7
20260610150849_AddMasterReviewsView	10.0.7
20260610153315_AddReviewInsertValidationTriggers	10.0.7
20260613104822_FixInitialMeetingAtNullable	10.0.7
20260615115903_AddOrderCompletedAtTrigger	10.0.7
20260615115949_AddResponseAcceptedRequestInProgressTrigger	10.0.7
20260615122303_AddRejectResponsesWhenOneAcceptedTrigger	10.0.7
\.


--
-- Data for Name: master_categories; Type: TABLE DATA; Schema: public; Owner: household_user
--

COPY public.master_categories (user_id, category_id) FROM stdin;
\.


--
-- Data for Name: master_profiles; Type: TABLE DATA; Schema: public; Owner: household_user
--

COPY public.master_profiles (user_id, description, experience_years) FROM stdin;
\.


--
-- Data for Name: order_statuses; Type: TABLE DATA; Schema: public; Owner: household_user
--

COPY public.order_statuses (order_status_id, name) FROM stdin;
1	in_progress
2	completed
3	cancelled
\.


--
-- Data for Name: orders; Type: TABLE DATA; Schema: public; Owner: household_user
--

COPY public.orders (order_id, response_id, order_status_id, price, initial_meeting_at, created_at, completed_at) FROM stdin;
1	1	2	100.00	2026-06-13 10:21:28.770923+00	2026-06-13 10:15:50.048477+00	2026-06-13 10:23:04.661649+00
2	2	2	10.00	2026-09-13 10:45:35.629+00	2026-06-13 10:37:57.087111+00	\N
\.


--
-- Data for Name: request_statuses; Type: TABLE DATA; Schema: public; Owner: household_user
--

COPY public.request_statuses (request_status_id, name) FROM stdin;
1	open
2	in_progress
3	completed
4	cancelled
\.


--
-- Data for Name: requests; Type: TABLE DATA; Schema: public; Owner: household_user
--

COPY public.requests (request_id, client_id, category_id, request_status_id, title, description, address, desired_date, created_at) FROM stdin;
1	1	1	1	string	string	string	2026-06-13 09:26:46.481+00	2026-06-13 09:26:55.72675+00
2	1	3	1	string	string	string	2026-06-13 10:33:10.307+00	2026-06-13 10:36:39.779989+00
3	4	3	1	Помыть окна	Срочно!	Пушкина д.1	2026-06-18 11:03:00+00	2026-06-17 11:03:21.692313+00
\.


--
-- Data for Name: response_statuses; Type: TABLE DATA; Schema: public; Owner: household_user
--

COPY public.response_statuses (response_status_id, name) FROM stdin;
1	pending
2	accepted
3	rejected
4	cancelled
\.


--
-- Data for Name: responses; Type: TABLE DATA; Schema: public; Owner: household_user
--

COPY public.responses (response_id, request_id, master_id, response_status_id, proposed_price, comment, created_at) FROM stdin;
1	1	2	2	100.00	string	2026-06-13 10:14:03.958171+00
2	2	2	2	10.00	string	2026-06-13 10:37:04.762363+00
\.


--
-- Data for Name: reviews; Type: TABLE DATA; Schema: public; Owner: household_user
--

COPY public.reviews (review_id, order_id, rating, comment, created_at) FROM stdin;
1	1	5	YOOOOOO	2026-06-13 10:22:35.82536+00
2	2	5	great	2026-06-13 10:46:49.870302+00
\.


--
-- Data for Name: roles; Type: TABLE DATA; Schema: public; Owner: household_user
--

COPY public.roles (role_id, name) FROM stdin;
1	client
2	master
\.


--
-- Data for Name: service_categories; Type: TABLE DATA; Schema: public; Owner: household_user
--

COPY public.service_categories (category_id, name, description) FROM stdin;
1	plumbing	Plumbing installation, repairs, and maintenance.
2	electrical	Electrical installation, diagnostics, and repairs.
3	cleaning	Regular, deep, and post-renovation cleaning.
4	appliance_repair	Diagnostics and repair of household appliances.
5	furniture_assembly	Furniture assembly, installation, and minor repairs.
\.


--
-- Data for Name: user_roles; Type: TABLE DATA; Schema: public; Owner: household_user
--

COPY public.user_roles (user_id, role_id) FROM stdin;
1	1
2	2
3	1
4	1
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: household_user
--

COPY public.users (user_id, login, password_hash, first_name, last_name, phone, created_at) FROM stdin;
1	string	AQAAAAIAAYagAAAAEP1SPHsvm/1bK9K5+QNEeWyul8oYrfHk2jC7UIH2DXx527G/TGIHmdjp8OL426L2Kw==	string	string	string	2026-06-13 09:24:56.387666+00
2	string1	AQAAAAIAAYagAAAAEMo569l3CV43nVQ+Njig+GBJIkk2NPmSo4Y4Vt5qg38GRa/zmr+/zKlp31X/zlPjbw==	string	string	string	2026-06-13 09:25:39.885987+00
3	string2	AQAAAAIAAYagAAAAEMzMYNlviGUXZjCoCowTAe5NINIoyhYVlGo4s2qg1q7k7CMD7mtT2KyEYPbHdezG7Q==	string	string	string	2026-06-13 09:25:54.706886+00
4	usl	AQAAAAIAAYagAAAAEOOe0c2j5RSUG47w+bmAJ5tLQXbkZ+++WhH/UaFvB0fiX0dJ3ZPpLl1cm7gP2K7vhQ==	a	a	+7 952 748 73 46	2026-06-17 11:03:00.937308+00
\.


--
-- Name: order_statuses_order_status_id_seq; Type: SEQUENCE SET; Schema: public; Owner: household_user
--

SELECT pg_catalog.setval('public.order_statuses_order_status_id_seq', 5, false);


--
-- Name: orders_order_id_seq; Type: SEQUENCE SET; Schema: public; Owner: household_user
--

SELECT pg_catalog.setval('public.orders_order_id_seq', 2, true);


--
-- Name: request_statuses_request_status_id_seq; Type: SEQUENCE SET; Schema: public; Owner: household_user
--

SELECT pg_catalog.setval('public.request_statuses_request_status_id_seq', 5, false);


--
-- Name: requests_request_id_seq; Type: SEQUENCE SET; Schema: public; Owner: household_user
--

SELECT pg_catalog.setval('public.requests_request_id_seq', 3, true);


--
-- Name: response_statuses_response_status_id_seq; Type: SEQUENCE SET; Schema: public; Owner: household_user
--

SELECT pg_catalog.setval('public.response_statuses_response_status_id_seq', 5, false);


--
-- Name: responses_response_id_seq; Type: SEQUENCE SET; Schema: public; Owner: household_user
--

SELECT pg_catalog.setval('public.responses_response_id_seq', 2, true);


--
-- Name: reviews_review_id_seq; Type: SEQUENCE SET; Schema: public; Owner: household_user
--

SELECT pg_catalog.setval('public.reviews_review_id_seq', 2, true);


--
-- Name: roles_RoleId_seq; Type: SEQUENCE SET; Schema: public; Owner: household_user
--

SELECT pg_catalog.setval('public."roles_RoleId_seq"', 3, false);


--
-- Name: service_categories_category_id_seq; Type: SEQUENCE SET; Schema: public; Owner: household_user
--

SELECT pg_catalog.setval('public.service_categories_category_id_seq', 6, false);


--
-- Name: users_user_id_seq; Type: SEQUENCE SET; Schema: public; Owner: household_user
--

SELECT pg_catalog.setval('public.users_user_id_seq', 4, true);


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: master_categories PK_master_categories; Type: CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.master_categories
    ADD CONSTRAINT "PK_master_categories" PRIMARY KEY (user_id, category_id);


--
-- Name: master_profiles PK_master_profiles; Type: CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.master_profiles
    ADD CONSTRAINT "PK_master_profiles" PRIMARY KEY (user_id);


--
-- Name: order_statuses PK_order_statuses; Type: CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.order_statuses
    ADD CONSTRAINT "PK_order_statuses" PRIMARY KEY (order_status_id);


--
-- Name: orders PK_orders; Type: CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT "PK_orders" PRIMARY KEY (order_id);


--
-- Name: request_statuses PK_request_statuses; Type: CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.request_statuses
    ADD CONSTRAINT "PK_request_statuses" PRIMARY KEY (request_status_id);


--
-- Name: requests PK_requests; Type: CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT "PK_requests" PRIMARY KEY (request_id);


--
-- Name: response_statuses PK_response_statuses; Type: CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.response_statuses
    ADD CONSTRAINT "PK_response_statuses" PRIMARY KEY (response_status_id);


--
-- Name: responses PK_responses; Type: CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.responses
    ADD CONSTRAINT "PK_responses" PRIMARY KEY (response_id);


--
-- Name: reviews PK_reviews; Type: CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.reviews
    ADD CONSTRAINT "PK_reviews" PRIMARY KEY (review_id);


--
-- Name: roles PK_roles; Type: CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT "PK_roles" PRIMARY KEY (role_id);


--
-- Name: service_categories PK_service_categories; Type: CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.service_categories
    ADD CONSTRAINT "PK_service_categories" PRIMARY KEY (category_id);


--
-- Name: user_roles PK_user_roles; Type: CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.user_roles
    ADD CONSTRAINT "PK_user_roles" PRIMARY KEY (user_id, role_id);


--
-- Name: users PK_users; Type: CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT "PK_users" PRIMARY KEY (user_id);


--
-- Name: IX_master_categories_category_id; Type: INDEX; Schema: public; Owner: household_user
--

CREATE INDEX "IX_master_categories_category_id" ON public.master_categories USING btree (category_id);


--
-- Name: IX_order_statuses_name; Type: INDEX; Schema: public; Owner: household_user
--

CREATE UNIQUE INDEX "IX_order_statuses_name" ON public.order_statuses USING btree (name);


--
-- Name: IX_orders_order_status_id; Type: INDEX; Schema: public; Owner: household_user
--

CREATE INDEX "IX_orders_order_status_id" ON public.orders USING btree (order_status_id);


--
-- Name: IX_orders_response_id; Type: INDEX; Schema: public; Owner: household_user
--

CREATE UNIQUE INDEX "IX_orders_response_id" ON public.orders USING btree (response_id);


--
-- Name: IX_request_statuses_name; Type: INDEX; Schema: public; Owner: household_user
--

CREATE UNIQUE INDEX "IX_request_statuses_name" ON public.request_statuses USING btree (name);


--
-- Name: IX_requests_category_id; Type: INDEX; Schema: public; Owner: household_user
--

CREATE INDEX "IX_requests_category_id" ON public.requests USING btree (category_id);


--
-- Name: IX_requests_client_id; Type: INDEX; Schema: public; Owner: household_user
--

CREATE INDEX "IX_requests_client_id" ON public.requests USING btree (client_id);


--
-- Name: IX_requests_request_status_id; Type: INDEX; Schema: public; Owner: household_user
--

CREATE INDEX "IX_requests_request_status_id" ON public.requests USING btree (request_status_id);


--
-- Name: IX_response_statuses_name; Type: INDEX; Schema: public; Owner: household_user
--

CREATE UNIQUE INDEX "IX_response_statuses_name" ON public.response_statuses USING btree (name);


--
-- Name: IX_responses_master_id; Type: INDEX; Schema: public; Owner: household_user
--

CREATE INDEX "IX_responses_master_id" ON public.responses USING btree (master_id);


--
-- Name: IX_responses_request_id_master_id; Type: INDEX; Schema: public; Owner: household_user
--

CREATE UNIQUE INDEX "IX_responses_request_id_master_id" ON public.responses USING btree (request_id, master_id);


--
-- Name: IX_responses_response_status_id; Type: INDEX; Schema: public; Owner: household_user
--

CREATE INDEX "IX_responses_response_status_id" ON public.responses USING btree (response_status_id);


--
-- Name: IX_reviews_order_id; Type: INDEX; Schema: public; Owner: household_user
--

CREATE UNIQUE INDEX "IX_reviews_order_id" ON public.reviews USING btree (order_id);


--
-- Name: IX_roles_name; Type: INDEX; Schema: public; Owner: household_user
--

CREATE UNIQUE INDEX "IX_roles_name" ON public.roles USING btree (name);


--
-- Name: IX_service_categories_name; Type: INDEX; Schema: public; Owner: household_user
--

CREATE UNIQUE INDEX "IX_service_categories_name" ON public.service_categories USING btree (name);


--
-- Name: IX_user_roles_role_id; Type: INDEX; Schema: public; Owner: household_user
--

CREATE INDEX "IX_user_roles_role_id" ON public.user_roles USING btree (role_id);


--
-- Name: IX_users_login; Type: INDEX; Schema: public; Owner: household_user
--

CREATE UNIQUE INDEX "IX_users_login" ON public.users USING btree (login);


--
-- Name: responses trg_create_order_after_response_accepted; Type: TRIGGER; Schema: public; Owner: household_user
--

CREATE TRIGGER trg_create_order_after_response_accepted AFTER UPDATE ON public.responses FOR EACH ROW EXECUTE FUNCTION public.create_order_after_response_accepted();


--
-- Name: responses trg_reject_responses_when_one_accepted; Type: TRIGGER; Schema: public; Owner: household_user
--

CREATE TRIGGER trg_reject_responses_when_one_accepted AFTER UPDATE OF response_status_id ON public.responses FOR EACH ROW WHEN ((old.response_status_id IS DISTINCT FROM new.response_status_id)) EXECUTE FUNCTION public.reject_responses_when_one_accepted();


--
-- Name: reviews trg_reviews_prevent_duplicate_per_order; Type: TRIGGER; Schema: public; Owner: household_user
--

CREATE TRIGGER trg_reviews_prevent_duplicate_per_order BEFORE INSERT ON public.reviews FOR EACH ROW EXECUTE FUNCTION public.prevent_duplicate_review_per_order();


--
-- Name: reviews trg_reviews_require_completed_order; Type: TRIGGER; Schema: public; Owner: household_user
--

CREATE TRIGGER trg_reviews_require_completed_order BEFORE INSERT ON public.reviews FOR EACH ROW EXECUTE FUNCTION public.ensure_review_order_completed();


--
-- Name: orders trg_set_order_completed_at_when_completed; Type: TRIGGER; Schema: public; Owner: household_user
--

CREATE TRIGGER trg_set_order_completed_at_when_completed BEFORE UPDATE OF order_status_id ON public.orders FOR EACH ROW WHEN ((old.order_status_id IS DISTINCT FROM new.order_status_id)) EXECUTE FUNCTION public.set_order_completed_at_when_completed();


--
-- Name: responses trg_set_request_in_progress_when_response_accepted; Type: TRIGGER; Schema: public; Owner: household_user
--

CREATE TRIGGER trg_set_request_in_progress_when_response_accepted AFTER UPDATE OF response_status_id ON public.responses FOR EACH ROW WHEN ((old.response_status_id IS DISTINCT FROM new.response_status_id)) EXECUTE FUNCTION public.set_request_in_progress_when_response_accepted();


--
-- Name: master_categories FK_master_categories_service_categories_category_id; Type: FK CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.master_categories
    ADD CONSTRAINT "FK_master_categories_service_categories_category_id" FOREIGN KEY (category_id) REFERENCES public.service_categories(category_id) ON DELETE CASCADE;


--
-- Name: master_categories FK_master_categories_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.master_categories
    ADD CONSTRAINT "FK_master_categories_users_user_id" FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE CASCADE;


--
-- Name: master_profiles FK_master_profiles_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.master_profiles
    ADD CONSTRAINT "FK_master_profiles_users_user_id" FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE CASCADE;


--
-- Name: orders FK_orders_responses_response_id; Type: FK CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT "FK_orders_responses_response_id" FOREIGN KEY (response_id) REFERENCES public.responses(response_id) ON DELETE CASCADE;


--
-- Name: requests FK_requests_users_client_id; Type: FK CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT "FK_requests_users_client_id" FOREIGN KEY (client_id) REFERENCES public.users(user_id) ON DELETE RESTRICT;


--
-- Name: responses FK_responses_requests_request_id; Type: FK CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.responses
    ADD CONSTRAINT "FK_responses_requests_request_id" FOREIGN KEY (request_id) REFERENCES public.requests(request_id) ON DELETE CASCADE;


--
-- Name: responses FK_responses_users_master_id; Type: FK CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.responses
    ADD CONSTRAINT "FK_responses_users_master_id" FOREIGN KEY (master_id) REFERENCES public.users(user_id) ON DELETE RESTRICT;


--
-- Name: reviews FK_reviews_orders_order_id; Type: FK CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.reviews
    ADD CONSTRAINT "FK_reviews_orders_order_id" FOREIGN KEY (order_id) REFERENCES public.orders(order_id) ON DELETE CASCADE;


--
-- Name: user_roles FK_user_roles_roles_role_id; Type: FK CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.user_roles
    ADD CONSTRAINT "FK_user_roles_roles_role_id" FOREIGN KEY (role_id) REFERENCES public.roles(role_id) ON DELETE CASCADE;


--
-- Name: user_roles FK_user_roles_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: household_user
--

ALTER TABLE ONLY public.user_roles
    ADD CONSTRAINT "FK_user_roles_users_user_id" FOREIGN KEY (user_id) REFERENCES public.users(user_id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict S8Q7oxpFIab9ZztfcnCf1JDxBTFIZqdEiPveoaLfaPyqiTN61yBoQyfg3jEcaEs

