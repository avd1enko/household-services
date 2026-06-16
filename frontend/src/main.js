import { api, clearSession, getSession, saveSession } from "./api.js";
import {
  SERVICE_CATEGORIES,
  STATUS_KIND,
  STATUS_LABELS,
  STORAGE_KEYS
} from "./config.js";

const app = document.querySelector("#app");

const navItems = {
  client: [
    ["dashboard", "Обзор", "gauge"],
    ["client-requests", "Заявки", "clipboard"],
    ["client-orders", "Заказы", "briefcase"],
    ["client-reviews", "Отзывы", "star"],
    ["profile", "Профиль", "user"]
  ],
  master: [
    ["dashboard", "Обзор", "gauge"],
    ["master-profile", "Профиль мастера", "user"],
    ["available-requests", "Доступные заявки", "search"],
    ["master-responses", "Мои отклики", "message"],
    ["master-orders", "Заказы", "briefcase"],
    ["master-reviews", "Отзывы", "star"],
    ["profile", "Аккаунт", "shield"]
  ]
};

const roleLabels = {
  client: "Клиент",
  master: "Мастер"
};

const state = {
  session: getSession(),
  authMode: "login",
  preferredRole: "client",
  activeView: localStorage.getItem(STORAGE_KEYS.activeView) || "dashboard",
  busy: false,
  busyText: "",
  toast: null,
  expandedRequestId: null,
  clientRequestsMode: "list",
  selectedOrderId: null,
  filters: {
    clientRequests: {},
    availableRequests: {}
  },
  data: {
    clientRequests: [],
    availableRequests: [],
    responsesByRequest: {},
    masterResponses: [],
    clientOrders: [],
    masterOrders: [],
    orderDetails: {},
    masterProfile: null,
    masterCategories: [],
    userProfile: null,
    ownReviews: [],
    lookupReviews: [],
    lookupMasterId: ""
  }
};

function escapeHtml(value) {
  return String(value ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}

function formatApiError(error) {
  if (!error) return "Неизвестная ошибка";
  const prefix = error.status ? `HTTP ${error.status}: ` : "";
  return `${prefix}${error.message || "Запрос не выполнен"}`;
}

function setToast(type, text) {
  state.toast = { type, text };
}

function categoryMeta(categoryIdOrName) {
  return SERVICE_CATEGORIES.find(
    category =>
      category.categoryId === Number(categoryIdOrName) ||
      category.name === categoryIdOrName
  );
}

function categoryLabel(categoryIdOrName) {
  const category = categoryMeta(categoryIdOrName);
  return category ? category.label : categoryIdOrName || "Без категории";
}

function categoryOptions(selectedValue = "") {
  return [
    `<option value="">Все категории</option>`,
    ...SERVICE_CATEGORIES.map(category => {
      const selected =
        String(selectedValue) === String(category.categoryId) ? "selected" : "";
      return `<option value="${category.categoryId}" ${selected}>${escapeHtml(category.label)}</option>`;
    })
  ].join("");
}

function requiredCategoryOptions(selectedValue = "") {
  return [
    `<option value="" disabled ${selectedValue ? "" : "selected"}>Выберите категорию</option>`,
    ...SERVICE_CATEGORIES.map(category => {
      const selected =
        String(selectedValue) === String(category.categoryId) ? "selected" : "";
      return `<option value="${category.categoryId}" ${selected}>${escapeHtml(category.label)}</option>`;
    })
  ].join("");
}

function statusBadge(status) {
  const kind = STATUS_KIND[status] || "muted";
  const label = STATUS_LABELS[status] || status || "Статус";
  return `<span class="status status-${kind}">${escapeHtml(label)}</span>`;
}

function iconSvg(name, extraClass = "") {
  const icons = {
    gauge: `<path d="M12 15.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5Z"/><path d="M13.8 16.8 18 9"/><path d="M4.6 19a9 9 0 1 1 14.8 0"/>`,
    clipboard: `<path d="M9 5h6"/><path d="M9 3h6v4H9z"/><path d="M7 5H5a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2"/><path d="M8 12h8M8 16h5"/>`,
    briefcase: `<path d="M9 7V5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2v2"/><path d="M4 7h16v11a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2z"/><path d="M4 12h16"/><path d="M10 12v2h4v-2"/>`,
    star: `<path d="m12 3 2.7 5.5 6.1.9-4.4 4.3 1 6.1L12 17l-5.4 2.8 1-6.1-4.4-4.3 6.1-.9z"/>`,
    user: `<path d="M12 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z"/><path d="M4 21a8 8 0 0 1 16 0"/>`,
    bell: `<path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9Z"/><path d="M10 21h4"/>`,
    search: `<circle cx="11" cy="11" r="7"/><path d="m20 20-3.5-3.5"/>`,
    message: `<path d="M4 5h16v11H8l-4 4z"/><path d="M8 9h8M8 13h5"/>`,
    shield: `<path d="M12 3 20 6v6c0 5-3.4 8.4-8 9-4.6-.6-8-4-8-9V6z"/><path d="m9 12 2 2 4-5"/>`,
    plus: `<path d="M12 5v14M5 12h14"/>`,
    refresh: `<path d="M20 7v5h-5"/><path d="M4 17v-5h5"/><path d="M19 12a7 7 0 0 0-12.1-4.8L4 10"/><path d="M5 12a7 7 0 0 0 12.1 4.8L20 14"/>`,
    logout: `<path d="M10 4H5v16h5"/><path d="M14 16l4-4-4-4"/><path d="M8 12h10"/>`,
    bolt: `<path d="M13 2 4 14h7l-1 8 10-13h-7z"/>`,
    home: `<path d="m3 11 9-8 9 8"/><path d="M5 10v10h14V10"/><path d="M10 20v-6h4v6"/>`,
    wrench: `<path d="M14.7 6.3a4 4 0 0 0 5 5L11 20l-5-5 8.7-8.7Z"/><path d="m7 17-3 3"/>`,
    calendar: `<path d="M7 3v4M17 3v4"/><path d="M4 8h16"/><path d="M5 5h14v16H5z"/>`,
    check: `<path d="m5 12 4 4L19 6"/>`,
    clock: `<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>`,
    sparkles: `<path d="M12 3l1.6 4.4L18 9l-4.4 1.6L12 15l-1.6-4.4L6 9l4.4-1.6z"/><path d="M19 14l.9 2.1L22 17l-2.1.9L19 20l-.9-2.1L16 17l2.1-.9z"/><path d="M5 14l.8 1.7L8 16.5l-2.2.8L5 19l-.8-1.7-2.2-.8 2.2-.8z"/>`
  };

  return `
    <svg class="icon ${extraClass}" viewBox="0 0 24 24" aria-hidden="true" focusable="false">
      <g fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2">
        ${icons[name] || icons.sparkles}
      </g>
    </svg>
  `;
}

function renderBrandMark(extraClass = "") {
  return `
    <div class="brand-mark ${extraClass}" aria-hidden="true">
      <svg viewBox="0 0 48 48" role="img" focusable="false">
        <path class="brand-orbit" d="M9 28c4-13 16-19 30-17-4 13-16 19-30 17Z" />
        <path class="brand-house" d="M14 25.5 24 17l10 8.5V36H14V25.5Z" />
        <path class="brand-roof" d="M11 26 24 15l13 11" />
        <path class="brand-bolt" d="M26.6 19 19 29h5l-1.6 7L30 25.8h-5.2Z" />
      </svg>
    </div>
  `;
}

function renderHeroVisual() {
  return `
    <div class="hero-visual" aria-hidden="true">
      <div class="hero-glow"></div>
      <div class="hero-phone">
        <div class="hero-phone-top">
          <span class="signal-dot"></span>
          <span>Live request</span>
        </div>
        <div class="hero-task-card active">
          <span class="task-icon">${iconSvg("bolt")}</span>
          <div>
            <strong>Срочная заявка</strong>
            <small>Сантехника · сегодня 18:00</small>
          </div>
        </div>
        <div class="hero-task-card">
          <span class="task-icon">${iconSvg("user")}</span>
          <div>
            <strong>3 мастера готовы</strong>
            <small>Лучшее предложение: 2 500 ₽</small>
          </div>
        </div>
        <div class="hero-task-card">
          <span class="task-icon">${iconSvg("check")}</span>
          <div>
            <strong>Заказ подтвержден</strong>
            <small>Контакты уже в кабинете</small>
          </div>
        </div>
      </div>
      <div class="floating-chip floating-chip-top">${iconSvg("clock")} быстрый отклик</div>
      <div class="floating-chip floating-chip-bottom">${iconSvg("shield")} проверенный процесс</div>
    </div>
  `;
}

function formatDate(value, options = {}) {
  if (!value) return "Не назначено";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "Некорректная дата";
  return new Intl.DateTimeFormat("ru-RU", {
    dateStyle: options.short ? "short" : "medium",
    timeStyle: options.dateOnly ? undefined : "short"
  }).format(date);
}

function formatMoney(value) {
  const amount = Number(value || 0);
  return new Intl.NumberFormat("ru-RU", {
    style: "currency",
    currency: "RUB",
    maximumFractionDigits: 0
  }).format(amount);
}

function pluralizeRu(count, one, few, many) {
  const abs = Math.abs(Number(count));
  const mod10 = abs % 10;
  const mod100 = abs % 100;
  if (mod10 === 1 && mod100 !== 11) return one;
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return few;
  return many;
}

function toInputDateTime(value) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  const offset = date.getTimezoneOffset() * 60000;
  return new Date(date.getTime() - offset).toISOString().slice(0, 16);
}

function toApiDateTime(value) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  return date.toISOString();
}

function buildQuery(filter) {
  const params = new URLSearchParams();
  const fields = [
    "categoryId",
    "title",
    "desiredDateFrom",
    "desiredDateTo",
    "createdAtFrom",
    "createdAtTo"
  ];

  for (const field of fields) {
    const value = filter[field];
    if (!value) continue;
    if (field.endsWith("From") || field.endsWith("To")) {
      params.set(field, toApiDateTime(value));
    } else {
      params.set(field, value);
    }
  }

  const query = params.toString();
  return query ? `?${query}` : "";
}

function readFilter(form) {
  const formData = new FormData(form);
  return {
    categoryId: formData.get("categoryId")?.toString() || "",
    title: formData.get("title")?.toString().trim() || "",
    desiredDateFrom: formData.get("desiredDateFrom")?.toString() || "",
    desiredDateTo: formData.get("desiredDateTo")?.toString() || "",
    createdAtFrom: formData.get("createdAtFrom")?.toString() || "",
    createdAtTo: formData.get("createdAtTo")?.toString() || ""
  };
}

function resetDataForSession() {
  state.expandedRequestId = null;
  state.clientRequestsMode = "list";
  state.selectedOrderId = null;
  state.filters.clientRequests = {};
  state.filters.availableRequests = {};
  state.data = {
    clientRequests: [],
    availableRequests: [],
    responsesByRequest: {},
    masterResponses: [],
    clientOrders: [],
    masterOrders: [],
    orderDetails: {},
    masterProfile: null,
    masterCategories: [],
    userProfile: null,
    ownReviews: [],
    lookupReviews: [],
    lookupMasterId: ""
  };
}

function ensureActiveView() {
  if (!state.session) return;
  const items = navItems[state.session.role] || navItems.client;
  const valid = items.some(([view]) => view === state.activeView);
  if (!valid) state.activeView = "dashboard";
  localStorage.setItem(STORAGE_KEYS.activeView, state.activeView);
}

async function withBusy(text, task, successText = "") {
  state.busy = true;
  state.busyText = text;
  state.toast = null;
  render();

  try {
    await task();
    if (successText) setToast("success", successText);
  } catch (error) {
    setToast("error", formatApiError(error));
  } finally {
    state.busy = false;
    state.busyText = "";
    render();
  }
}

async function loadDashboardData() {
  if (state.session.role === "client") {
    await Promise.all([safeLoadUserProfile(), loadClientRequests(), loadClientOrders()]);
    return;
  }

  await Promise.all([
    safeLoadUserProfile(),
    loadMasterProfile(),
    loadAvailableRequests(),
    loadMasterResponses(),
    loadMasterOrders(),
    loadOwnReviews()
  ]);
}

async function refreshActiveView(successText = "") {
  if (!state.session) return;
  ensureActiveView();

  await withBusy(
    "Загружаю данные",
    async () => {
      switch (state.activeView) {
        case "dashboard":
          await loadDashboardData();
          break;
        case "client-requests":
          await loadClientRequests();
          break;
        case "client-orders":
          await loadClientOrders();
          break;
        case "client-reviews":
          await loadClientOrders();
          if (state.data.lookupMasterId) {
            await loadReviewsForMaster(state.data.lookupMasterId);
          }
          break;
        case "master-profile":
          await Promise.all([loadMasterProfile(), loadMasterCategories()]);
          break;
        case "available-requests":
          await Promise.all([loadAvailableRequests(), loadMasterResponses(), loadMasterProfile()]);
          break;
        case "master-responses":
          await loadMasterResponses();
          break;
        case "master-orders":
          await loadMasterOrders();
          break;
        case "master-reviews":
          await loadOwnReviews();
          break;
        case "profile":
          await loadUserProfile();
          break;
        default:
          await loadDashboardData();
      }
    },
    successText
  );
}

async function loadClientRequests() {
  const requests = await api.getClientRequests(
    buildQuery(state.filters.clientRequests)
  );
  state.data.clientRequests = requests;

  await Promise.all(
    requests.map(async request => {
      try {
        state.data.responsesByRequest[request.requestId] =
          await api.getResponsesForRequest(request.requestId);
      } catch {
        state.data.responsesByRequest[request.requestId] = null;
      }
    })
  );
}

async function loadAvailableRequests() {
  state.data.availableRequests = await api.getAvailableRequests(
    buildQuery(state.filters.availableRequests)
  );
}

async function loadRequestResponses(requestId) {
  state.data.responsesByRequest[requestId] =
    await api.getResponsesForRequest(requestId);
  state.expandedRequestId = Number(requestId);
}

async function loadMasterResponses() {
  state.data.masterResponses = await api.getMasterResponses();
}

async function loadClientOrders() {
  state.data.clientOrders = await api.getClientOrders();
}

async function loadMasterOrders() {
  state.data.masterOrders = await api.getMasterOrders();
}

async function loadOrder(orderId) {
  state.data.orderDetails[orderId] = await api.getOrder(orderId);
  state.selectedOrderId = Number(orderId);
}

async function loadMasterProfile() {
  state.data.masterProfile = await api.getMasterProfile();
}

async function loadMasterCategories() {
  state.data.masterCategories = await api.getMasterCategories();
}

async function loadUserProfile() {
  state.data.userProfile = await api.getUserProfile();
}

async function safeLoadUserProfile() {
  try {
    await loadUserProfile();
  } catch {
    state.data.userProfile = null;
  }
}

async function loadOwnReviews() {
  if (!state.session) return;
  state.data.ownReviews = await api.getMasterReviews(state.session.userId);
}

async function loadReviewsForMaster(masterId) {
  state.data.lookupMasterId = String(masterId);
  state.data.lookupReviews = await api.getMasterReviews(masterId);
}

function renderToast() {
  if (!state.toast) return "";
  return `<div class="toast toast-${state.toast.type}" role="status">${escapeHtml(state.toast.text)}</div>`;
}

function renderBusy() {
  if (!state.busy) return "";
  return `
    <div class="busy" aria-live="polite">
      <span class="spinner" aria-hidden="true"></span>
      ${escapeHtml(state.busyText || "Выполняю запрос")}
    </div>
  `;
}

function renderAuthPage() {
  const isLogin = state.authMode === "login";

  return `
    <main class="auth-layout">
      <section class="auth-intro hero-card">
        <div class="hero-copy">
          <div class="brand-row">
            ${renderBrandMark("brand-mark-large")}
            <div>
              <strong>Household Services</strong>
              <span>быстрый поиск бытовых мастеров</span>
            </div>
          </div>
        <p class="eyebrow">Профессионально. Быстро. Без лишнего шума.</p>
        <h1>Бытовые услуги, которые не выглядят как лотерея</h1>
        <p class="lead">
          Создай заявку, сравни предложения мастеров и выбери того, кому доверяешь.
          Сервис ведет весь путь: заявка, отклик, заказ, отзыв.
        </p>
        <div class="hero-actions">
          <button class="primary hero-cta" data-action="auth-mode" data-mode="register" data-role="client" type="button">${iconSvg("plus")} Создать заявку</button>
          <button class="secondary hero-cta" data-action="auth-mode" data-mode="register" data-role="master" type="button">${iconSvg("wrench")} Стать мастером</button>
        </div>
        <div class="hero-points">
          <span>${iconSvg("clipboard")} Заявка за минуту</span>
          <span>${iconSvg("message")} Отклики с ценой</span>
          <span>${iconSvg("star")} Отзывы после заказа</span>
        </div>
        </div>
        ${renderHeroVisual()}
      </section>

      <section class="auth-panel" aria-label="Авторизация">
        ${renderToast()}
        ${renderBusy()}
        <div class="segmented" role="tablist">
          <button class="${isLogin ? "active" : ""}" data-action="auth-mode" data-mode="login" type="button">Вход</button>
          <button class="${!isLogin ? "active" : ""}" data-action="auth-mode" data-mode="register" type="button">Регистрация</button>
        </div>
        ${isLogin ? renderLoginForm() : renderRegisterForm()}
      </section>
    </main>
  `;
}

function renderLoginForm() {
  return `
    <form class="stack" data-form="login">
      <label>
        <span>Логин</span>
        <input name="login" autocomplete="username" required />
      </label>
      <label>
        <span>Пароль</span>
        <input name="password" type="password" autocomplete="current-password" required />
      </label>
      <button class="primary full" type="submit">${iconSvg("shield")} Войти</button>
    </form>
  `;
}

function renderRegisterForm() {
  const selectedRole = state.preferredRole || "client";
  return `
    <form class="stack" data-form="register">
      <div class="two-cols">
        <label>
          <span>Имя</span>
          <input name="firstName" autocomplete="given-name" required />
        </label>
        <label>
          <span>Фамилия</span>
          <input name="lastName" autocomplete="family-name" required />
        </label>
      </div>
      <label>
        <span>Телефон</span>
        <input name="phone" autocomplete="tel" placeholder="+7 900 000-00-00" required />
      </label>
      <label>
        <span>Логин</span>
        <input name="login" autocomplete="username" required />
      </label>
      <label>
        <span>Пароль</span>
        <input name="password" type="password" autocomplete="new-password" required minlength="4" />
      </label>
      <fieldset class="role-choice">
        <legend>Роль</legend>
        <label>
          <input type="radio" name="role" value="client" ${selectedRole === "client" ? "checked" : ""} />
          <span>Клиент</span>
        </label>
        <label>
          <input type="radio" name="role" value="master" ${selectedRole === "master" ? "checked" : ""} />
          <span>Мастер</span>
        </label>
      </fieldset>
      <button class="primary full" type="submit">${iconSvg("sparkles")} Создать аккаунт</button>
    </form>
  `;
}

function renderShell() {
  ensureActiveView();
  const items = navItems[state.session.role] || navItems.client;

  return `
    <div class="app-shell">
      <aside class="sidebar">
        <div class="sidebar-brand">
          ${renderBrandMark()}
          <div>
            <strong>Household Services</strong>
            <span>${escapeHtml(roleLabels[state.session.role] || state.session.role)}</span>
          </div>
        </div>
        <nav class="nav-list" aria-label="Главная навигация">
          ${items.map(([view, label, icon]) => `
            <button class="${state.activeView === view ? "active" : ""}" data-action="set-view" data-view="${view}" type="button">
              ${iconSvg(icon)}
              <span>${escapeHtml(label)}</span>
            </button>
          `).join("")}
        </nav>
        <div class="sidebar-footer">
          <span>${escapeHtml(state.session.login)}</span>
          <button class="ghost" data-action="logout" type="button">${iconSvg("logout")} Выйти</button>
        </div>
      </aside>

      <main class="content">
        <header class="topbar">
          <div>
            <p class="eyebrow">${escapeHtml(roleLabels[state.session.role] || state.session.role)}</p>
            <h1>${escapeHtml(currentViewTitle())}</h1>
          </div>
          <div class="topbar-actions">
            ${state.session.role === "client" ? `
              <button class="primary" data-action="open-create-request" type="button">${iconSvg("plus")} Новая заявка</button>
            ` : `
              <button class="primary" data-action="set-view" data-view="available-requests" type="button">${iconSvg("search")} Найти заказы</button>
            `}
            <button class="secondary" data-action="refresh" type="button">${iconSvg("refresh")} Обновить</button>
          </div>
        </header>
        ${renderToast()}
        ${renderBusy()}
        ${renderActiveView()}
      </main>
    </div>
  `;
}

function currentViewTitle() {
  const items = navItems[state.session?.role] || [];
  const item = items.find(([view]) => view === state.activeView);
  return item?.[1] || "Обзор";
}

function renderActiveView() {
  switch (state.activeView) {
    case "dashboard":
      return renderDashboard();
    case "client-requests":
      return renderClientRequests();
    case "client-orders":
      return renderOrders("client");
    case "client-reviews":
      return renderClientReviews();
    case "master-profile":
      return renderMasterProfile();
    case "available-requests":
      return renderAvailableRequests();
    case "master-responses":
      return renderMasterResponses();
    case "master-orders":
      return renderOrders("master");
    case "master-reviews":
      return renderMasterReviews();
    case "profile":
      return renderUserProfile();
    default:
      return renderDashboard();
  }
}

function renderDashboard() {
  if (state.session.role === "client") {
    const requests = state.data.clientRequests;
    const orders = state.data.clientOrders;
    const profile = state.data.userProfile;
    const firstName = profile?.firstName || state.session.login;
    const openRequests = requests.filter(item => item.status === "open").length;
    const completedOrders = orders.filter(item => item.status === "completed").length;

    return `
      <section class="dashboard-hero client-hero">
        <div>
          <p class="eyebrow">Need help fast?</p>
          <h2>${escapeHtml(firstName)}, оставь заявку, а мастера сами предложат условия</h2>
          <p>Создание заявки занимает меньше минуты. После откликов можно сравнить цену, опыт и выбрать исполнителя.</p>
        </div>
        <button class="primary hero-cta" data-action="open-create-request" type="button">${iconSvg("plus")} Создать заявку</button>
      </section>
      <section class="metric-grid">
        ${metricCard("Мои заявки", requests.length, "clipboard")}
        ${metricCard("Открытые заявки", openRequests, "bolt")}
        ${metricCard("Заказы", orders.length, "briefcase")}
        ${metricCard("Завершено", completedOrders, "check")}
      </section>
      <section class="split">
        <div class="surface">
          <div class="section-title">
            <div>
              <h2>Ближайшие заявки</h2>
              <p>Создание, отмена и просмотр откликов.</p>
            </div>
            <button class="secondary" data-action="set-view" data-view="client-requests" type="button">${iconSvg("clipboard")} Открыть</button>
          </div>
          ${renderCompactRequestList(requests.slice(0, 4))}
        </div>
        <div class="surface">
          <div class="section-title">
            <div>
              <h2>Заказы</h2>
              <p>Встречи, завершение, отмена и отзывы.</p>
            </div>
            <button class="secondary" data-action="set-view" data-view="client-orders" type="button">${iconSvg("briefcase")} Открыть</button>
          </div>
          ${renderCompactOrderList(orders.slice(0, 4), "client")}
        </div>
      </section>
    `;
  }

  const profile = state.data.masterProfile;
  const available = state.data.availableRequests;
  const responses = state.data.masterResponses;
  const orders = state.data.masterOrders;

  return `
    <section class="dashboard-hero master-hero">
      <div>
        <p class="eyebrow">Master workspace</p>
        <h2>Быстро находи подходящие заявки и веди заказы</h2>
        <p>Список заявок фильтруется по твоим специализациям, а все отклики и заказы собраны в рабочем кабинете.</p>
      </div>
      <button class="primary hero-cta" data-action="set-view" data-view="available-requests" type="button">${iconSvg("search")} Найти заказы</button>
    </section>
    <section class="metric-grid">
      ${metricCard("Специализации", profile?.categories?.length || 0, "wrench")}
      ${metricCard("Доступные заявки", available.length, "search")}
      ${metricCard("Мои отклики", responses.length, "message")}
      ${metricCard("Заказы", orders.length, "briefcase")}
    </section>
    <section class="split">
      <div class="surface">
        <div class="section-title">
          <div>
            <h2>Профиль мастера</h2>
            <p>${escapeHtml(profile?.description || "Описание пока не заполнено")}</p>
          </div>
          <button class="secondary" data-action="set-view" data-view="master-profile" type="button">${iconSvg("user")} Настроить</button>
        </div>
        <div class="chip-row">
          ${(profile?.categories || []).map(category => `<span class="chip">${escapeHtml(categoryLabel(category.name))}</span>`).join("") || `<span class="muted">Категории не выбраны</span>`}
        </div>
      </div>
      <div class="surface">
        <div class="section-title">
          <div>
            <h2>Новые заявки</h2>
            <p>Показываются только категории из специализаций.</p>
          </div>
          <button class="secondary" data-action="set-view" data-view="available-requests" type="button">${iconSvg("search")} Смотреть</button>
        </div>
        ${renderCompactAvailableList(available.slice(0, 4))}
      </div>
    </section>
  `;
}

function metricCard(label, value, icon) {
  return `
    <article class="metric">
      <div class="metric-icon">${iconSvg(icon)}</div>
      <span>${escapeHtml(label)}</span>
      <strong>${escapeHtml(value)}</strong>
    </article>
  `;
}

function renderCompactRequestList(requests) {
  if (!requests.length) return emptyState("Заявок пока нет");
  return `
    <div class="compact-list">
      ${requests.map(request => `
        <div class="compact-row">
          <div>
            <strong>${escapeHtml(request.title)}</strong>
            <span>${escapeHtml(categoryLabel(request.categoryName || request.categoryId))} · ${formatDate(request.desiredDate)}</span>
          </div>
          ${statusBadge(request.status)}
        </div>
      `).join("")}
    </div>
  `;
}

function renderCompactAvailableList(requests) {
  if (!requests.length) return emptyState("Подходящих открытых заявок нет");
  return `
    <div class="compact-list">
      ${requests.map(request => `
        <div class="compact-row">
          <div>
            <strong>${escapeHtml(request.title)}</strong>
            <span>${escapeHtml(categoryLabel(request.categoryName))} · ${escapeHtml(request.address)}</span>
          </div>
          ${statusBadge(request.status)}
        </div>
      `).join("")}
    </div>
  `;
}

function renderCompactOrderList(orders, role) {
  if (!orders.length) return emptyState("Заказов пока нет");
  return `
    <div class="compact-list">
      ${orders.map(order => `
        <div class="compact-row">
          <div>
            <strong>${escapeHtml(order.requestTitle)}</strong>
            <span>${role === "client"
              ? `Мастер: ${escapeHtml(order.masterFirstName)} ${escapeHtml(order.masterLastName)}`
              : `Клиент: ${escapeHtml(order.clientFirstName)} ${escapeHtml(order.clientLastName)}`
            }</span>
          </div>
          ${statusBadge(order.status)}
        </div>
      `).join("")}
    </div>
  `;
}

function renderRequestFilter(kind) {
  const filter = state.filters[kind];
  const formName =
    kind === "clientRequests" ? "client-request-filter" : "available-request-filter";

  return `
    <form class="filter-bar" data-form="${formName}">
      <label>
        <span>Категория</span>
        <select name="categoryId">${categoryOptions(filter.categoryId)}</select>
      </label>
      <label>
        <span>Поиск</span>
        <input name="title" value="${escapeHtml(filter.title || "")}" placeholder="Название заявки" />
      </label>
      <label>
        <span>Желаемая дата от</span>
        <input type="datetime-local" name="desiredDateFrom" value="${escapeHtml(filter.desiredDateFrom || "")}" />
      </label>
      <label>
        <span>Желаемая дата до</span>
        <input type="datetime-local" name="desiredDateTo" value="${escapeHtml(filter.desiredDateTo || "")}" />
      </label>
      <label>
        <span>Создана от</span>
        <input type="datetime-local" name="createdAtFrom" value="${escapeHtml(filter.createdAtFrom || "")}" />
      </label>
      <label>
        <span>Создана до</span>
        <input type="datetime-local" name="createdAtTo" value="${escapeHtml(filter.createdAtTo || "")}" />
      </label>
      <div class="filter-actions">
        <button class="secondary" type="submit">${iconSvg("search")} Найти</button>
        <button class="ghost" data-action="reset-filter" data-filter="${kind}" type="button">${iconSvg("refresh")} Сбросить</button>
      </div>
    </form>
  `;
}

function renderClientRequests() {
  const isCreateMode = state.clientRequestsMode === "create";

  return `
    ${renderRequestModeTabs()}
    ${isCreateMode ? renderCreateRequestPanel() : renderClientRequestsPanel()}
  `;
}

function renderRequestModeTabs() {
  const requests = state.data.clientRequests;
  const loadedResponseCount = requests.reduce((sum, request) => {
    const responses = state.data.responsesByRequest[request.requestId];
    return sum + (Array.isArray(responses) ? responses.length : 0);
  }, 0);

  return `
    <section class="surface compact-surface">
      <div class="request-mode-tabs" role="tablist" aria-label="Раздел заявок">
        <button class="${state.clientRequestsMode === "list" ? "active" : ""}" data-action="set-client-request-mode" data-mode="list" type="button">
          ${iconSvg("clipboard")} Мои заявки
          <span>${requests.length}</span>
        </button>
        <button class="${state.clientRequestsMode === "create" ? "active" : ""}" data-action="set-client-request-mode" data-mode="create" type="button">
          ${iconSvg("plus")} Создать заявку
        </button>
      </div>
      <div class="request-insight">
        <span>${iconSvg("message")} ${loadedResponseCount ? `${loadedResponseCount} ${pluralizeRu(loadedResponseCount, "отклик", "отклика", "откликов")} по текущему списку` : "Отклики появятся прямо на карточках заявок"}</span>
      </div>
    </section>
  `;
}

function renderCreateRequestPanel() {
  return `
    <section class="surface request-create-surface">
      <div class="section-title">
        <div>
          <p class="eyebrow">Fast request</p>
          <h2>Создать заявку</h2>
          <p>Коротко опиши задачу, выбери категорию, адрес и желаемое время.</p>
        </div>
        <button class="secondary" data-action="set-client-request-mode" data-mode="list" type="button">${iconSvg("clipboard")} К списку заявок</button>
      </div>
      <form class="request-form" data-form="create-request">
        <label>
          <span>Категория</span>
          <select name="categoryId" required>${requiredCategoryOptions()}</select>
        </label>
        <label>
          <span>Название</span>
          <input name="title" required maxlength="120" placeholder="Например, заменить смеситель" />
        </label>
        <label>
          <span>Желаемая дата</span>
          <input name="desiredDate" type="datetime-local" required />
        </label>
        <label>
          <span>Адрес</span>
          <input name="address" required maxlength="220" />
        </label>
        <label class="wide">
          <span>Описание</span>
          <textarea name="description" rows="4" required></textarea>
        </label>
        <button class="primary hero-cta" type="submit">${iconSvg("plus")} Создать заявку</button>
      </form>
    </section>
  `;
}

function renderClientRequestsPanel() {
  return `
    <section class="surface">
      <div class="section-title">
        <div>
          <h2>Мои заявки</h2>
          <p>Отклики подгружаются автоматически и видны прямо на карточках.</p>
        </div>
        <button class="primary" data-action="set-client-request-mode" data-mode="create" type="button">${iconSvg("plus")} Новая заявка</button>
      </div>
      ${renderRequestFilter("clientRequests")}
      ${renderClientRequestList()}
    </section>
  `;
}

function renderClientRequestList() {
  const requests = state.data.clientRequests;
  if (!requests.length) return emptyState("По текущим условиям заявок нет");

  return `
    <div class="item-list">
      ${requests.map(request => renderClientRequestItem(request)).join("")}
    </div>
  `;
}

function renderClientRequestItem(request) {
  const responses = state.data.responsesByRequest[request.requestId];
  const expanded = Number(state.expandedRequestId) === Number(request.requestId);
  const responseCount = Array.isArray(responses) ? responses.length : 0;

  return `
    <article class="item-card ${responseCount ? "has-responses" : ""}">
      <div class="item-main">
        <div>
          <div class="item-kicker">${escapeHtml(categoryLabel(request.categoryName))}</div>
          <h3>${escapeHtml(request.title)}</h3>
          <p>Желаемая дата: ${formatDate(request.desiredDate)} · Создана: ${formatDate(request.createdAt)}</p>
        </div>
        ${statusBadge(request.status)}
      </div>
      ${renderRequestResponseSummary(responses)}
      <div class="actions">
        <button class="secondary" data-action="load-request-responses" data-request-id="${request.requestId}" type="button">
          ${iconSvg("message")} ${expanded ? "Обновить отклики" : responseCount ? `Показать отклики (${responseCount})` : "Проверить отклики"}
        </button>
        ${request.status === "open" ? `
          <button class="danger" data-action="cancel-request" data-request-id="${request.requestId}" type="button">${iconSvg("logout")} Отменить</button>
        ` : ""}
      </div>
      ${expanded ? renderResponsesForRequest(request.requestId, responses) : ""}
    </article>
  `;
}

function renderRequestResponseSummary(responses) {
  if (responses === null) {
    return `
      <div class="response-summary muted-summary">
        <span class="summary-icon">${iconSvg("message")}</span>
        <div>
          <strong>Отклики не загрузились</strong>
          <span>Можно обновить карточку вручную.</span>
        </div>
      </div>
    `;
  }

  if (!Array.isArray(responses)) {
    return `
      <div class="response-summary muted-summary">
        <span class="summary-icon">${iconSvg("clock")}</span>
        <div>
          <strong>Проверяю отклики</strong>
          <span>Список подтянется после обновления.</span>
        </div>
      </div>
    `;
  }

  if (!responses.length) {
    return `
      <div class="response-summary">
        <span class="summary-icon">${iconSvg("message")}</span>
        <div>
          <strong>Пока нет откликов</strong>
          <span>Когда мастер ответит, это появится здесь.</span>
        </div>
      </div>
    `;
  }

  const pendingCount = responses.filter(response => response.status === "pending").length;
  const accepted = responses.find(response => response.status === "accepted");
  const bestPrice = Math.min(...responses.map(response => Number(response.proposedPrice || 0)).filter(Boolean));

  return `
    <div class="response-summary active-summary">
      <span class="summary-icon">${iconSvg(accepted ? "check" : "bell")}</span>
      <div>
        <strong>${accepted ? "Отклик принят" : `${pendingCount || responses.length} ${pluralizeRu(pendingCount || responses.length, "новый отклик", "новых отклика", "новых откликов")}`}</strong>
        <span>${bestPrice ? `Лучшая цена: ${formatMoney(bestPrice)} · ` : ""}${responses.length} всего</span>
      </div>
    </div>
  `;
}

function renderResponsesForRequest(requestId, responses) {
  if (!responses) {
    return `<div class="sub-panel">${emptyState("Отклики еще не загружены")}</div>`;
  }

  if (!responses.length) {
    return `<div class="sub-panel">${emptyState("Мастера пока не откликнулись")}</div>`;
  }

  return `
    <div class="sub-panel">
      <h4>Отклики мастеров</h4>
      <div class="sub-list">
        ${responses.map(response => `
          <div class="response-row">
            <div>
              <strong>${escapeHtml(response.masterFirstName)} ${escapeHtml(response.masterLastName)}</strong>
              <span>${formatMoney(response.proposedPrice)} · ${escapeHtml(response.masterPhone)}</span>
              <p>${escapeHtml(response.comment || "Комментарий не указан")}</p>
              <small>${escapeHtml(response.masterDescription || "Профиль без описания")} · опыт: ${escapeHtml(response.masterExperienceYears ?? 0)} лет</small>
            </div>
            <div class="row-actions">
              ${statusBadge(response.status)}
              <button class="ghost" data-action="view-master-reviews" data-master-id="${response.masterId}" type="button">${iconSvg("star")} Отзывы</button>
              ${response.status === "pending" ? `
                <button class="primary" data-action="accept-response" data-response-id="${response.responseId}" data-request-id="${requestId}" type="button">${iconSvg("check")} Принять</button>
              ` : ""}
            </div>
          </div>
        `).join("")}
      </div>
    </div>
  `;
}

function renderAvailableRequests() {
  const profile = state.data.masterProfile;
  const hasCategories = (profile?.categories || []).length > 0;

  return `
    <section class="surface">
      <div class="section-title">
        <div>
          <h2>Доступные заявки</h2>
          <p>Список строится backend’ом по специализациям мастера.</p>
        </div>
      </div>
      ${!hasCategories ? `
        <div class="notice">
          Чтобы увидеть заявки, выберите специализации в профиле мастера.
        </div>
      ` : ""}
      ${renderRequestFilter("availableRequests")}
      ${renderAvailableRequestList()}
    </section>
  `;
}

function renderAvailableRequestList() {
  const requests = state.data.availableRequests;
  const existingByRequest = new Map(
    state.data.masterResponses.map(response => [Number(response.requestId), response])
  );

  if (!requests.length) return emptyState("Подходящих открытых заявок нет");

  return `
    <div class="item-list">
      ${requests.map(request => {
        const existingResponse = existingByRequest.get(Number(request.requestId));
        return `
          <article class="item-card">
            <div class="item-main">
              <div>
                <div class="item-kicker">${escapeHtml(categoryLabel(request.categoryName))}</div>
                <h3>${escapeHtml(request.title)}</h3>
                <p>${escapeHtml(request.description)}</p>
              </div>
              ${statusBadge(request.status)}
            </div>
            <dl class="details-grid">
              <div><dt>Клиент</dt><dd>${escapeHtml(request.clientFirstName)} ${escapeHtml(request.clientLastName)}</dd></div>
              <div><dt>Адрес</dt><dd>${escapeHtml(request.address)}</dd></div>
              <div><dt>Желаемая дата</dt><dd>${formatDate(request.desiredDate)}</dd></div>
              <div><dt>Создана</dt><dd>${formatDate(request.createdAt)}</dd></div>
            </dl>
            ${existingResponse ? `
              <div class="notice compact">
                Отклик уже создан: ${statusBadge(existingResponse.status)} ${formatMoney(existingResponse.proposedPrice)}
              </div>
            ` : renderCreateResponseForm(request.requestId)}
          </article>
        `;
      }).join("")}
    </div>
  `;
}

function renderCreateResponseForm(requestId) {
  return `
    <form class="inline-form" data-form="create-response">
      <input type="hidden" name="requestId" value="${requestId}" />
      <label>
        <span>Цена</span>
        <input name="proposedPrice" type="number" min="1" step="1" required />
      </label>
      <label class="grow">
        <span>Комментарий</span>
        <input name="comment" placeholder="Сроки, материалы, детали работы" />
      </label>
      <button class="primary" type="submit">${iconSvg("message")} Откликнуться</button>
    </form>
  `;
}

function renderMasterResponses() {
  const responses = state.data.masterResponses;

  return `
    <section class="surface">
      <div class="section-title">
        <div>
          <h2>Мои отклики</h2>
          <p>Ожидающие отклики можно отозвать, обработанные остаются в истории.</p>
        </div>
      </div>
      ${!responses.length ? emptyState("Откликов пока нет") : `
        <div class="item-list">
          ${responses.map(response => `
            <article class="item-card">
              <div class="item-main">
                <div>
                  <div class="item-kicker">${escapeHtml(categoryLabel(response.categoryName))}</div>
                  <h3>${escapeHtml(response.requestTitle)}</h3>
                  <p>${formatMoney(response.proposedPrice)} · ${formatDate(response.createdAt)}</p>
                  <p>${escapeHtml(response.comment || "Комментарий не указан")}</p>
                </div>
                ${statusBadge(response.status)}
              </div>
              <div class="actions">
                ${response.status === "pending" ? `
                  <button class="danger" data-action="cancel-response" data-response-id="${response.responseId}" type="button">${iconSvg("logout")} Отозвать</button>
                ` : ""}
              </div>
            </article>
          `).join("")}
        </div>
      `}
    </section>
  `;
}

function renderOrders(role) {
  const orders = role === "client" ? state.data.clientOrders : state.data.masterOrders;
  const selectedOrder = state.selectedOrderId
    ? state.data.orderDetails[state.selectedOrderId]
    : null;

  return `
    ${selectedOrder ? renderOrderDetails(selectedOrder, role) : ""}
    <section class="surface">
      <div class="section-title">
        <div>
          <h2>${role === "client" ? "Мои заказы" : "Заказы мастера"}</h2>
          <p>Заказ появляется после принятия отклика клиентом.</p>
        </div>
      </div>
      ${!orders.length ? emptyState("Заказов пока нет") : `
        <div class="item-list">
          ${orders.map(order => `
            <article class="item-card">
              <div class="item-main">
                <div>
                  <div class="item-kicker">${escapeHtml(categoryLabel(order.categoryName))}</div>
                  <h3>${escapeHtml(order.requestTitle)}</h3>
                  <p>${formatMoney(order.price)} · создан: ${formatDate(order.createdAt)}</p>
                </div>
                ${statusBadge(order.status)}
              </div>
              <dl class="details-grid">
                <div><dt>${role === "client" ? "Мастер" : "Клиент"}</dt><dd>${role === "client"
                  ? `${escapeHtml(order.masterFirstName)} ${escapeHtml(order.masterLastName)}`
                  : `${escapeHtml(order.clientFirstName)} ${escapeHtml(order.clientLastName)}`
                }</dd></div>
                ${role === "master" ? `<div><dt>Телефон клиента</dt><dd>${escapeHtml(order.clientPhone)}</dd></div>` : ""}
                <div><dt>Встреча</dt><dd>${formatDate(order.initialMeetingAt)}</dd></div>
                <div><dt>Завершен</dt><dd>${formatDate(order.completedAt)}</dd></div>
              </dl>
              <div class="actions">
                <button class="secondary" data-action="load-order" data-order-id="${order.orderId}" type="button">${iconSvg("briefcase")} Детали</button>
                ${order.status === "in_progress" ? renderOrderQuickActions(order, role) : ""}
                ${role === "client" && order.status === "completed" ? `
                  <button class="ghost" data-action="prepare-review" data-order-id="${order.orderId}" type="button">${iconSvg("star")} Оставить отзыв</button>
                ` : ""}
              </div>
            </article>
          `).join("")}
        </div>
      `}
    </section>
  `;
}

function renderOrderQuickActions(order, role) {
  return `
    ${role === "client" ? `
      <button class="primary" data-action="complete-order" data-order-id="${order.orderId}" type="button">${iconSvg("check")} Завершить</button>
    ` : ""}
    <button class="danger" data-action="cancel-order" data-order-id="${order.orderId}" type="button">${iconSvg("logout")} Отменить</button>
  `;
}

function renderOrderDetails(order, role) {
  return `
    <section class="surface highlighted">
      <div class="section-title">
        <div>
          <h2>Заказ #${escapeHtml(order.orderId)}</h2>
          <p>${escapeHtml(order.requestTitle)} · ${escapeHtml(categoryLabel(order.categoryName))}</p>
        </div>
        ${statusBadge(order.status)}
      </div>
      <dl class="details-grid wide-details">
        <div><dt>Цена</dt><dd>${formatMoney(order.price)}</dd></div>
        <div><dt>Создан</dt><dd>${formatDate(order.createdAt)}</dd></div>
        <div><dt>Желаемая дата</dt><dd>${formatDate(order.desiredDate)}</dd></div>
        <div><dt>Встреча</dt><dd>${formatDate(order.initialMeetingAt)}</dd></div>
        <div><dt>Клиент</dt><dd>${escapeHtml(order.clientFirstName)} ${escapeHtml(order.clientLastName)} · ${escapeHtml(order.clientPhone)}</dd></div>
        <div><dt>Мастер</dt><dd>${escapeHtml(order.masterFirstName)} ${escapeHtml(order.masterLastName)} · ${escapeHtml(order.masterPhone)}</dd></div>
        <div class="span-2"><dt>Адрес</dt><dd>${escapeHtml(order.requestAddress)}</dd></div>
        <div class="span-2"><dt>Описание заявки</dt><dd>${escapeHtml(order.requestDescription)}</dd></div>
      </dl>
      ${order.status === "in_progress" ? `
        <form class="inline-form" data-form="update-meeting">
          <input type="hidden" name="orderId" value="${order.orderId}" />
          <label>
            <span>Первая встреча</span>
            <input name="initialMeetingAt" type="datetime-local" value="${escapeHtml(toInputDateTime(order.initialMeetingAt))}" required />
          </label>
          <button class="secondary" type="submit">${iconSvg("calendar")} Сохранить встречу</button>
        </form>
      ` : ""}
      ${role === "client" && order.status === "completed" ? renderReviewForm(order.orderId) : ""}
    </section>
  `;
}

function renderReviewForm(orderId) {
  return `
    <form class="review-form" data-form="create-review">
      <input type="hidden" name="orderId" value="${orderId}" />
      <label>
        <span>Оценка</span>
        <select name="rating" required>
          <option value="5">5 — отлично</option>
          <option value="4">4 — хорошо</option>
          <option value="3">3 — нормально</option>
          <option value="2">2 — плохо</option>
          <option value="1">1 — очень плохо</option>
        </select>
      </label>
      <label class="grow">
        <span>Комментарий</span>
        <textarea name="comment" rows="3" placeholder="Что было сделано хорошо, что стоит улучшить"></textarea>
      </label>
      <button class="primary" type="submit">${iconSvg("star")} Опубликовать отзыв</button>
    </form>
  `;
}

function renderClientReviews() {
  const completedOrders = state.data.clientOrders.filter(order => order.status === "completed");

  return `
    <section class="surface">
      <div class="section-title">
        <div>
          <h2>Отзывы мастера</h2>
          <p>Посмотри репутацию мастера перед выбором исполнителя.</p>
        </div>
      </div>
      ${renderReviewLookupForm()}
      ${renderReviewList(state.data.lookupReviews)}
    </section>
    <section class="surface">
      <div class="section-title">
        <div>
          <h2>Завершенные заказы</h2>
          <p>Отзыв можно оставить один раз по завершенному заказу.</p>
        </div>
      </div>
      ${!completedOrders.length ? emptyState("Завершенных заказов пока нет") : `
        <div class="compact-list">
          ${completedOrders.map(order => `
            <div class="compact-row">
              <div>
                <strong>${escapeHtml(order.requestTitle)}</strong>
                <span>Мастер: ${escapeHtml(order.masterFirstName)} ${escapeHtml(order.masterLastName)} · ${formatMoney(order.price)}</span>
              </div>
              <button class="secondary" data-action="prepare-review" data-order-id="${order.orderId}" type="button">${iconSvg("star")} Отзыв</button>
            </div>
          `).join("")}
        </div>
      `}
    </section>
  `;
}

function renderMasterReviews() {
  return `
    <section class="surface">
      <div class="section-title">
        <div>
          <h2>Отзывы обо мне</h2>
          <p>Отзывы клиентов, которые видят будущие заказчики.</p>
        </div>
      </div>
      ${renderReviewList(state.data.ownReviews)}
    </section>
    <section class="surface">
      <div class="section-title">
        <div>
          <h2>Проверить публичные отзывы</h2>
          <p>Быстрый поиск отзывов по номеру мастера.</p>
        </div>
      </div>
      ${renderReviewLookupForm()}
      ${renderReviewList(state.data.lookupReviews)}
    </section>
  `;
}

function renderReviewLookupForm() {
  return `
    <form class="inline-form" data-form="lookup-reviews">
      <label>
        <span>Master ID</span>
        <input name="masterId" type="number" min="1" value="${escapeHtml(state.data.lookupMasterId)}" required />
      </label>
      <button class="secondary" type="submit">${iconSvg("star")} Показать отзывы</button>
    </form>
  `;
}

function renderReviewList(reviews) {
  if (!reviews?.length) return emptyState("Отзывы не найдены");

  return `
    <div class="review-list">
      ${reviews.map(review => `
        <article class="review-row">
          <div class="rating">${"★".repeat(Number(review.rating))}${"☆".repeat(5 - Number(review.rating))}</div>
          <div>
            <strong>${escapeHtml(review.requestTitle)}</strong>
            <span>${escapeHtml(categoryLabel(review.categoryName))} · ${escapeHtml(review.clientFirstName)} ${escapeHtml(review.clientLastName)} · ${formatDate(review.createdAt)}</span>
            <p>${escapeHtml(review.comment || "Комментарий не указан")}</p>
          </div>
        </article>
      `).join("")}
    </div>
  `;
}

function renderMasterProfile() {
  const profile = state.data.masterProfile;
  const selectedIds = new Set(
    (state.data.masterCategories.length
      ? state.data.masterCategories
      : profile?.categories || []
    ).map(category => Number(category.categoryId))
  );

  return `
    <section class="surface">
      <div class="section-title">
        <div>
          <h2>Данные аккаунта</h2>
          <p>Имя, телефон и логин приходят из профиля мастера.</p>
        </div>
      </div>
      ${profile ? `
        <dl class="details-grid">
          <div><dt>Логин</dt><dd>${escapeHtml(profile.login)}</dd></div>
          <div><dt>Имя</dt><dd>${escapeHtml(profile.firstName)} ${escapeHtml(profile.lastName)}</dd></div>
          <div><dt>Телефон</dt><dd>${escapeHtml(profile.phone)}</dd></div>
          <div><dt>User ID</dt><dd>${escapeHtml(profile.userId)}</dd></div>
        </dl>
      ` : emptyState("Профиль еще не загружен")}
    </section>

    <section class="surface">
      <div class="section-title">
        <div>
          <h2>Профессиональный профиль</h2>
          <p>Описание и опыт используются в откликах клиента.</p>
        </div>
      </div>
      <form class="stack" data-form="update-master-profile">
        <label>
          <span>Описание</span>
          <textarea name="description" rows="5">${escapeHtml(profile?.description || "")}</textarea>
        </label>
        <label>
          <span>Опыт, лет</span>
          <input name="experienceYears" type="number" min="0" step="1" value="${escapeHtml(profile?.experienceYears ?? 0)}" />
        </label>
        <button class="primary fit" type="submit">${iconSvg("check")} Сохранить профиль</button>
      </form>
    </section>

    <section class="surface">
      <div class="section-title">
        <div>
          <h2>Специализации</h2>
          <p>Список заменяется полностью при сохранении.</p>
        </div>
      </div>
      <form class="stack" data-form="update-master-categories">
        <div class="checkbox-grid">
          ${SERVICE_CATEGORIES.map(category => `
            <label class="check-tile">
              <input type="checkbox" name="categoryIds" value="${category.categoryId}" ${selectedIds.has(category.categoryId) ? "checked" : ""} />
              <span>
                <strong>${escapeHtml(category.label)}</strong>
                <small>${escapeHtml(category.description)}</small>
              </span>
            </label>
          `).join("")}
        </div>
        <button class="primary fit" type="submit">${iconSvg("wrench")} Сохранить специализации</button>
      </form>
    </section>
  `;
}

function renderUserProfile() {
  const profile = state.data.userProfile;
  const displayName = profile
    ? `${profile.firstName || ""} ${profile.lastName || ""}`.trim() || profile.login
    : state.session.login;

  return `
    <section class="profile-hero surface">
      <div class="profile-avatar">${escapeHtml(getInitials(displayName))}</div>
      <div class="profile-hero-copy">
        <p class="eyebrow">${escapeHtml(roleLabels[profile?.role || state.session.role] || state.session.role)}</p>
        <h2>${escapeHtml(displayName)}</h2>
        <p>${profile ? `Аккаунт создан ${formatDate(profile.createdAt, { dateOnly: true })}` : "Данные профиля пока не загружены"}</p>
      </div>
      <div class="profile-stats">
        <div>
          <span>${iconSvg("check")}</span>
          <strong>${escapeHtml(profile?.completedOrdersCount ?? 0)}</strong>
          <small>завершено</small>
        </div>
        <div>
          <span>${iconSvg("calendar")}</span>
          <strong>${escapeHtml(profile?.daysSinceRegistration ?? 0)}</strong>
          <small>дней в сервисе</small>
        </div>
      </div>
    </section>

    <section class="split profile-split">
      <div class="surface">
        <div class="section-title">
          <div>
            <h2>Контактные данные</h2>
            <p>Эти данные используются в заявках, заказах и откликах.</p>
          </div>
        </div>
        ${profile ? `
          <dl class="details-grid profile-details">
            <div><dt>Логин</dt><dd>${escapeHtml(profile.login)}</dd></div>
            <div><dt>Телефон</dt><dd>${escapeHtml(profile.phone)}</dd></div>
            <div><dt>User ID</dt><dd>${escapeHtml(profile.userId)}</dd></div>
            <div><dt>Роль</dt><dd>${escapeHtml(roleLabels[profile.role] || profile.role)}</dd></div>
          </dl>
          <form class="stack profile-form" data-form="update-user-profile">
            <div class="two-cols">
              <label>
                <span>Имя</span>
                <input name="firstName" value="${escapeHtml(profile.firstName)}" required />
              </label>
              <label>
                <span>Фамилия</span>
                <input name="lastName" value="${escapeHtml(profile.lastName)}" required />
              </label>
            </div>
            <label>
              <span>Телефон</span>
              <input name="phone" value="${escapeHtml(profile.phone)}" required />
            </label>
            <button class="primary fit" type="submit">${iconSvg("check")} Сохранить данные</button>
          </form>
        ` : emptyState("Профиль пока не загружен")}
      </div>

      <div class="surface">
        <div class="section-title">
          <div>
            <h2>Безопасность</h2>
            <p>Сменить пароль можно без выхода из аккаунта.</p>
          </div>
        </div>
        <form class="stack" data-form="change-password">
          <label>
            <span>Текущий пароль</span>
            <input name="currentPassword" type="password" autocomplete="current-password" required />
          </label>
          <label>
            <span>Новый пароль</span>
            <input name="newPassword" type="password" autocomplete="new-password" required minlength="4" />
          </label>
          <button class="secondary fit" type="submit">${iconSvg("shield")} Обновить пароль</button>
        </form>
      </div>
    </section>
  `;
}

function getInitials(name) {
  const parts = String(name || "")
    .trim()
    .split(/\s+/)
    .filter(Boolean);
  if (!parts.length) return "HS";
  return parts.slice(0, 2).map(part => part[0]).join("").toUpperCase();
}

function emptyState(text) {
  return `<div class="empty">${escapeHtml(text)}</div>`;
}

function render() {
  app.innerHTML = state.session ? renderShell() : renderAuthPage();
}

async function handleAction(actionElement) {
  const action = actionElement.dataset.action;

  if (action === "auth-mode") {
    state.authMode = actionElement.dataset.mode || "login";
    if (actionElement.dataset.role) {
      state.preferredRole = actionElement.dataset.role;
    }
    state.toast = null;
    render();
    return;
  }

  if (action === "logout") {
    clearSession();
    state.session = null;
    state.activeView = "dashboard";
    resetDataForSession();
    render();
    return;
  }

  if (action === "set-view") {
    state.activeView = actionElement.dataset.view || "dashboard";
    if (state.activeView === "client-requests") {
      state.clientRequestsMode = "list";
    }
    localStorage.setItem(STORAGE_KEYS.activeView, state.activeView);
    state.toast = null;
    render();
    await refreshActiveView();
    return;
  }

  if (action === "open-create-request") {
    state.activeView = "client-requests";
    state.clientRequestsMode = "create";
    localStorage.setItem(STORAGE_KEYS.activeView, state.activeView);
    state.toast = null;
    render();
    await refreshActiveView();
    return;
  }

  if (action === "set-client-request-mode") {
    state.clientRequestsMode = actionElement.dataset.mode || "list";
    state.toast = null;
    render();
    return;
  }

  if (action === "refresh") {
    await refreshActiveView("Данные обновлены");
    return;
  }

  if (action === "reset-filter") {
    const filterName = actionElement.dataset.filter;
    if (filterName && state.filters[filterName]) {
      state.filters[filterName] = {};
      await refreshActiveView("Фильтр сброшен");
    }
    return;
  }

  if (action === "load-request-responses") {
    const requestId = actionElement.dataset.requestId;
    await withBusy("Загружаю отклики", () => loadRequestResponses(requestId));
    return;
  }

  if (action === "cancel-request") {
    const requestId = actionElement.dataset.requestId;
    await withBusy(
      "Отменяю заявку",
      async () => {
        await api.cancelRequest(requestId);
        await loadClientRequests();
      },
      "Заявка отменена"
    );
    return;
  }

  if (action === "accept-response") {
    const responseId = actionElement.dataset.responseId;
    const requestId = actionElement.dataset.requestId;
    await withBusy(
      "Принимаю отклик",
      async () => {
        await api.acceptResponse(responseId);
        await Promise.all([
          loadClientRequests(),
          loadRequestResponses(requestId),
          loadClientOrders()
        ]);
      },
      "Отклик принят, заказ создан backend-триггером"
    );
    return;
  }

  if (action === "view-master-reviews") {
    const masterId = actionElement.dataset.masterId;
    state.activeView = state.session.role === "client" ? "client-reviews" : "master-reviews";
    localStorage.setItem(STORAGE_KEYS.activeView, state.activeView);
    await withBusy("Загружаю отзывы", () => loadReviewsForMaster(masterId));
    return;
  }

  if (action === "cancel-response") {
    const responseId = actionElement.dataset.responseId;
    await withBusy(
      "Отзываю отклик",
      async () => {
        await api.cancelResponse(responseId);
        await Promise.all([loadMasterResponses(), loadAvailableRequests()]);
      },
      "Отклик отозван"
    );
    return;
  }

  if (action === "load-order") {
    const orderId = actionElement.dataset.orderId;
    await withBusy("Загружаю заказ", () => loadOrder(orderId));
    return;
  }

  if (action === "complete-order") {
    const orderId = actionElement.dataset.orderId;
    await withBusy(
      "Завершаю заказ",
      async () => {
        await api.completeOrder(orderId);
        await Promise.all([loadClientOrders(), loadOrder(orderId)]);
      },
      "Заказ переведен в статус completed"
    );
    return;
  }

  if (action === "cancel-order") {
    const orderId = actionElement.dataset.orderId;
    await withBusy(
      "Отменяю заказ",
      async () => {
        await api.cancelOrder(orderId);
        if (state.session.role === "client") await loadClientOrders();
        if (state.session.role === "master") await loadMasterOrders();
        await loadOrder(orderId);
      },
      "Заказ отменен"
    );
    return;
  }

  if (action === "prepare-review") {
    const orderId = actionElement.dataset.orderId;
    state.activeView = "client-orders";
    localStorage.setItem(STORAGE_KEYS.activeView, state.activeView);
    await withBusy("Открываю заказ", () => loadOrder(orderId));
    return;
  }

}

async function handleSubmit(form) {
  const formName = form.dataset.form;
  const formData = new FormData(form);

  if (formName === "login") {
    await withBusy(
      "Выполняю вход",
      async () => {
        const session = await api.login({
          login: formData.get("login")?.toString().trim(),
          password: formData.get("password")?.toString()
        });
        saveSession(session);
        state.session = session;
        state.activeView = "dashboard";
        resetDataForSession();
        localStorage.setItem(STORAGE_KEYS.activeView, state.activeView);
        await loadDashboardData();
      },
      "Вход выполнен"
    );
    return;
  }

  if (formName === "register") {
    await withBusy(
      "Создаю аккаунт",
      async () => {
        const session = await api.register({
          login: formData.get("login")?.toString().trim(),
          password: formData.get("password")?.toString(),
          firstName: formData.get("firstName")?.toString().trim(),
          lastName: formData.get("lastName")?.toString().trim(),
          phone: formData.get("phone")?.toString().trim(),
          role: formData.get("role")?.toString()
        });
        saveSession(session);
        state.session = session;
        state.activeView = "dashboard";
        resetDataForSession();
        localStorage.setItem(STORAGE_KEYS.activeView, state.activeView);
        await loadDashboardData();
      },
      "Аккаунт создан"
    );
    return;
  }

  if (formName === "client-request-filter") {
    state.filters.clientRequests = readFilter(form);
    await refreshActiveView("Фильтр применен");
    return;
  }

  if (formName === "available-request-filter") {
    state.filters.availableRequests = readFilter(form);
    await refreshActiveView("Фильтр применен");
    return;
  }

  if (formName === "create-request") {
    await withBusy(
      "Создаю заявку",
      async () => {
        await api.createRequest({
          categoryId: Number(formData.get("categoryId")),
          title: formData.get("title")?.toString().trim(),
          description: formData.get("description")?.toString().trim(),
          address: formData.get("address")?.toString().trim(),
          desiredDate: toApiDateTime(formData.get("desiredDate")?.toString())
        });
        state.clientRequestsMode = "list";
        await loadClientRequests();
      },
      "Заявка создана"
    );
    return;
  }

  if (formName === "create-response") {
    const requestId = formData.get("requestId")?.toString();
    await withBusy(
      "Создаю отклик",
      async () => {
        await api.createResponse(requestId, {
          proposedPrice: Number(formData.get("proposedPrice")),
          comment: formData.get("comment")?.toString().trim() || null
        });
        await Promise.all([loadMasterResponses(), loadAvailableRequests()]);
      },
      "Отклик создан"
    );
    return;
  }

  if (formName === "update-meeting") {
    const orderId = formData.get("orderId")?.toString();
    await withBusy(
      "Сохраняю встречу",
      async () => {
        await api.updateInitialMeeting(orderId, {
          initialMeetingAt: toApiDateTime(formData.get("initialMeetingAt")?.toString())
        });
        if (state.session.role === "client") await loadClientOrders();
        if (state.session.role === "master") await loadMasterOrders();
        await loadOrder(orderId);
      },
      "Встреча обновлена"
    );
    return;
  }

  if (formName === "create-review") {
    const orderId = formData.get("orderId")?.toString();
    await withBusy(
      "Публикую отзыв",
      async () => {
        await api.createReview(orderId, {
          rating: Number(formData.get("rating")),
          comment: formData.get("comment")?.toString().trim() || null
        });
        await loadClientOrders();
        if (state.selectedOrderId) await loadOrder(state.selectedOrderId);
      },
      "Отзыв опубликован"
    );
    return;
  }

  if (formName === "update-user-profile") {
    await withBusy(
      "Сохраняю профиль",
      async () => {
        await api.updateUserProfile({
          firstName: formData.get("firstName")?.toString().trim(),
          lastName: formData.get("lastName")?.toString().trim(),
          phone: formData.get("phone")?.toString().trim()
        });
        await loadUserProfile();
      },
      "Профиль обновлен"
    );
    return;
  }

  if (formName === "change-password") {
    await withBusy(
      "Обновляю пароль",
      async () => {
        await api.changePassword({
          currentPassword: formData.get("currentPassword")?.toString(),
          newPassword: formData.get("newPassword")?.toString()
        });
        form.reset();
      },
      "Пароль обновлен"
    );
    return;
  }

  if (formName === "update-master-profile") {
    await withBusy(
      "Сохраняю профиль",
      async () => {
        await api.updateMasterProfile({
          description: formData.get("description")?.toString(),
          experienceYears: Number(formData.get("experienceYears"))
        });
        await loadMasterProfile();
      },
      "Профиль обновлен"
    );
    return;
  }

  if (formName === "update-master-categories") {
    await withBusy(
      "Сохраняю специализации",
      async () => {
        const categoryIds = formData
          .getAll("categoryIds")
          .map(value => Number(value));
        await api.replaceMasterCategories({ categoryIds });
        await Promise.all([loadMasterProfile(), loadMasterCategories(), loadAvailableRequests()]);
      },
      "Специализации обновлены"
    );
    return;
  }

  if (formName === "lookup-reviews") {
    const masterId = formData.get("masterId")?.toString();
    await withBusy("Загружаю отзывы", () => loadReviewsForMaster(masterId));
    return;
  }

}

app.addEventListener("click", event => {
  const actionElement = event.target.closest("[data-action]");
  if (!actionElement) return;
  event.preventDefault();
  void handleAction(actionElement);
});

app.addEventListener("submit", event => {
  event.preventDefault();
  void handleSubmit(event.target);
});

render();
if (state.session) {
  void refreshActiveView();
}
