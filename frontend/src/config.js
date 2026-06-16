export const MAIN_API_BASE = "/api";
export const NOTIFICATION_API_BASE = "/notification-api/api";

export const RAW_MAIN_BACKEND_URL = "http://localhost:5183";
export const RAW_NOTIFICATION_BACKEND_URL = "http://localhost:5333";

export const STORAGE_KEYS = {
  session: "household-services.session",
  activeView: "household-services.activeView"
};

export const SERVICE_CATEGORIES = [
  {
    categoryId: 1,
    name: "plumbing",
    label: "Сантехника",
    description: "Монтаж, ремонт и обслуживание сантехники."
  },
  {
    categoryId: 2,
    name: "electrical",
    label: "Электрика",
    description: "Диагностика, монтаж и ремонт электрики."
  },
  {
    categoryId: 3,
    name: "cleaning",
    label: "Клининг",
    description: "Поддерживающая, генеральная и послеремонтная уборка."
  },
  {
    categoryId: 4,
    name: "appliance_repair",
    label: "Ремонт техники",
    description: "Диагностика и ремонт бытовой техники."
  },
  {
    categoryId: 5,
    name: "furniture_assembly",
    label: "Сборка мебели",
    description: "Сборка, установка и мелкий ремонт мебели."
  }
];

export const STATUS_LABELS = {
  open: "Открыта",
  in_progress: "В работе",
  completed: "Завершено",
  cancelled: "Отменено",
  pending: "Ожидает",
  accepted: "Принят",
  rejected: "Отклонен"
};

export const STATUS_KIND = {
  open: "info",
  in_progress: "warning",
  completed: "success",
  cancelled: "danger",
  pending: "warning",
  accepted: "success",
  rejected: "danger"
};
