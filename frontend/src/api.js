import {
  MAIN_API_BASE,
  NOTIFICATION_API_BASE,
  STORAGE_KEYS
} from "./config.js";

export class ApiError extends Error {
  constructor(status, message, payload) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.payload = payload;
  }
}

export function getSession() {
  const rawSession = localStorage.getItem(STORAGE_KEYS.session);
  if (!rawSession) return null;

  try {
    return JSON.parse(rawSession);
  } catch {
    localStorage.removeItem(STORAGE_KEYS.session);
    return null;
  }
}

export function saveSession(session) {
  localStorage.setItem(STORAGE_KEYS.session, JSON.stringify(session));
}

export function clearSession() {
  localStorage.removeItem(STORAGE_KEYS.session);
}

function normalizePayload(payload) {
  if (payload === undefined || payload === null) return undefined;
  return JSON.stringify(payload);
}

function parseErrorMessage(status, payload, fallback) {
  if (typeof payload === "string" && payload.trim()) return payload;
  if (payload?.message) return payload.message;
  if (payload?.title) return payload.title;
  return fallback || `Request failed with status ${status}`;
}

export async function apiRequest(path, options = {}) {
  const {
    method = "GET",
    body,
    auth = true,
    base = "main",
    headers: customHeaders = {}
  } = options;
  const session = getSession();
  const baseUrl = base === "notification" ? NOTIFICATION_API_BASE : MAIN_API_BASE;
  const headers = {
    accept: "application/json",
    ...customHeaders
  };

  if (body !== undefined) headers["content-type"] = "application/json";
  if (auth && session?.token) {
    headers.authorization = `Bearer ${session.token}`;
  }

  let response;
  try {
    response = await fetch(`${baseUrl}${path}`, {
      method,
      headers,
      body: normalizePayload(body)
    });
  } catch (error) {
    throw new ApiError(0, "Не удалось подключиться к сервису", {
      detail: error.message
    });
  }

  const text = await response.text();
  let payload = null;
  if (text) {
    try {
      payload = JSON.parse(text);
    } catch {
      payload = text;
    }
  }

  if (!response.ok) {
    throw new ApiError(
      response.status,
      parseErrorMessage(response.status, payload, response.statusText),
      payload
    );
  }

  return payload;
}

export const api = {
  login: payload => apiRequest("/auth/login", { method: "POST", body: payload, auth: false }),
  register: payload =>
    apiRequest("/auth/register", { method: "POST", body: payload, auth: false }),
  protectedTest: () => apiRequest("/auth/protected-test"),
  currentUserTest: () => apiRequest("/auth/current-user-test"),

  getClientRequests: query => apiRequest(`/users/me/requests${query}`),
  getAvailableRequests: query => apiRequest(`/requests${query}`),
  getRequest: requestId => apiRequest(`/requests/${requestId}`),
  createRequest: payload => apiRequest("/requests", { method: "POST", body: payload }),
  cancelRequest: requestId =>
    apiRequest(`/requests/${requestId}/cancel`, { method: "PATCH" }),

  getResponsesForRequest: requestId => apiRequest(`/requests/${requestId}/responses`),
  createResponse: (requestId, payload) =>
    apiRequest(`/requests/${requestId}/responses`, { method: "POST", body: payload }),
  getMasterResponses: () => apiRequest("/masters/me/responses"),
  acceptResponse: responseId =>
    apiRequest(`/responses/${responseId}/accept`, { method: "POST" }),
  cancelResponse: responseId =>
    apiRequest(`/responses/${responseId}/cancel`, { method: "POST" }),

  getOrder: orderId => apiRequest(`/orders/${orderId}`),
  getClientOrders: () => apiRequest("/users/me/orders"),
  getMasterOrders: () => apiRequest("/masters/me/orders"),
  updateInitialMeeting: (orderId, payload) =>
    apiRequest(`/orders/${orderId}/initial-meeting`, { method: "PATCH", body: payload }),
  completeOrder: orderId => apiRequest(`/orders/${orderId}/complete`, { method: "PATCH" }),
  cancelOrder: orderId => apiRequest(`/orders/${orderId}/cancel`, { method: "PATCH" }),

  createReview: (orderId, payload) =>
    apiRequest(`/orders/${orderId}/reviews`, { method: "POST", body: payload }),
  getMasterReviews: masterId => apiRequest(`/masters/${masterId}/reviews`, { auth: false }),

  getUserProfile: () => apiRequest("/users/me"),
  updateUserProfile: payload =>
    apiRequest("/users/me", { method: "PATCH", body: payload }),
  changePassword: payload =>
    apiRequest("/users/me/password", { method: "PATCH", body: payload }),

  getMasterProfile: () => apiRequest("/masters/me"),
  updateMasterProfile: payload =>
    apiRequest("/masters/me", { method: "PATCH", body: payload }),
  getMasterCategories: () => apiRequest("/masters/me/categories"),
  replaceMasterCategories: payload =>
    apiRequest("/masters/me/categories", { method: "PUT", body: payload }),

  sendMainNotificationTest: () =>
    apiRequest("/notification-test/send", { method: "POST", auth: false }),
  sendNotification: payload =>
    apiRequest("/notification/send", {
      method: "POST",
      body: payload,
      auth: false,
      base: "notification"
    })
};
