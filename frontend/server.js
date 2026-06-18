import { createReadStream } from "node:fs";
import { access, readFile } from "node:fs/promises";
import { createServer, request as httpRequest } from "node:http";
import { request as httpsRequest } from "node:https";
import { extname, join, normalize, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const PORT = Number(process.env.PORT || 5173);
const HOST = process.env.HOST || "127.0.0.1";
// const MAIN_BACKEND = process.env.MAIN_BACKEND_URL || "http://localhost:5183";
const MAIN_BACKEND = process.env.MAIN_BACKEND_URL || "https://household-services-q5k0.onrender.com"
const NOTIFICATION_BACKEND =
  process.env.NOTIFICATION_BACKEND_URL || "http://localhost:5333";

const __dirname = fileURLToPath(new URL(".", import.meta.url));
const rootDir = resolve(__dirname);

const mimeTypes = {
  ".html": "text/html; charset=utf-8",
  ".css": "text/css; charset=utf-8",
  ".js": "text/javascript; charset=utf-8",
  ".json": "application/json; charset=utf-8",
  ".svg": "image/svg+xml",
  ".ico": "image/x-icon"
};

function sendJson(res, statusCode, payload) {
  res.writeHead(statusCode, {
    "content-type": "application/json; charset=utf-8",
    "cache-control": "no-store"
  });
  res.end(JSON.stringify(payload));
}

function proxyRequest(req, res, targetBase, stripPrefix = "") {
  const originalUrl = new URL(req.url || "/", `http://${HOST}:${PORT}`);
  const upstreamPath = stripPrefix
    ? originalUrl.pathname.replace(stripPrefix, "") || "/"
    : originalUrl.pathname;

  const upstreamUrl = new URL(
    `${upstreamPath}${originalUrl.search}`,
    targetBase
  );

  const requestClient =
    upstreamUrl.protocol === "https:" ? httpsRequest : httpRequest;

  const headers = { ...req.headers };
  headers.host = upstreamUrl.host;
  headers.origin = targetBase;

  const upstreamReq = requestClient(
    upstreamUrl,
    {
      method: req.method,
      headers
    },
    upstreamRes => {
      const responseHeaders = {
        ...upstreamRes.headers,
        "access-control-allow-origin": `http://${HOST}:${PORT}`,
        "access-control-allow-credentials": "true"
      };
      res.writeHead(upstreamRes.statusCode || 500, responseHeaders);
      upstreamRes.pipe(res);
    }
  );

  upstreamReq.on("error", error => {
    sendJson(res, 502, {
      message: "Backend service is unavailable",
      detail: error.message,
      target: targetBase
    });
  });

  req.pipe(upstreamReq);
}

async function serveStatic(req, res) {
  const requestUrl = new URL(req.url || "/", `http://${HOST}:${PORT}`);
  const pathname = decodeURIComponent(requestUrl.pathname);
  const normalizedPath = normalize(pathname).replace(/^(\.\.[/\\])+/, "");
  const requestedPath =
    normalizedPath === "/" ? "/index.html" : normalizedPath;
  const filePath = resolve(join(rootDir, requestedPath));

  if (!filePath.startsWith(rootDir)) {
    sendJson(res, 403, { message: "Forbidden" });
    return;
  }

  try {
    await access(filePath);
    const ext = extname(filePath);
    res.writeHead(200, {
      "content-type": mimeTypes[ext] || "application/octet-stream",
      "cache-control": "no-store"
    });
    createReadStream(filePath).pipe(res);
  } catch {
    const fallback = await readFile(join(rootDir, "index.html"));
    res.writeHead(200, {
      "content-type": "text/html; charset=utf-8",
      "cache-control": "no-store"
    });
    res.end(fallback);
  }
}

const server = createServer((req, res) => {
  if (req.method === "OPTIONS") {
    res.writeHead(204, {
      "access-control-allow-origin": `http://${HOST}:${PORT}`,
      "access-control-allow-methods": "GET,POST,PATCH,PUT,DELETE,OPTIONS",
      "access-control-allow-headers": "content-type,authorization"
    });
    res.end();
    return;
  }

  const url = new URL(req.url || "/", `http://${HOST}:${PORT}`);

  if (url.pathname.startsWith("/notification-api/")) {
    proxyRequest(req, res, NOTIFICATION_BACKEND, "/notification-api");
    return;
  }

  if (url.pathname.startsWith("/api/")) {
    proxyRequest(req, res, MAIN_BACKEND);
    return;
  }

  void serveStatic(req, res);
});

server.listen(PORT, HOST, () => {
  console.log(`Frontend: http://${HOST}:${PORT}`);
  console.log(`Main backend proxy: ${MAIN_BACKEND}`);
  console.log(`Notification proxy: ${NOTIFICATION_BACKEND}`);
});
