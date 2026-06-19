export const environment = {
  production: true,
  // In Docker, Nginx proxies these so we use
  // relative paths — no localhost needed
  apiUrl: '/api',
  hubUrl: '/hubs/notifications',
};
