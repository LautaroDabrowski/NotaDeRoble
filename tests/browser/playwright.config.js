const { defineConfig } = require('@playwright/test');

module.exports = defineConfig({
    testDir: '.',
    testMatch: '*.spec.js',
    workers: 1,
    timeout: 60000,
    use: { baseURL: process.env.BASE_URL || 'http://localhost:5068', browserName: 'chromium' },
});
