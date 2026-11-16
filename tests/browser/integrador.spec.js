const { test, expect } = require('@playwright/test');
const { randomUUID } = require('node:crypto');
const path = require('node:path');

test('registro, sesión tras F5, validación, CRUD, búsqueda, paginación y tema móvil', async ({ page }) => {
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    const user = `u_${randomUUID().replaceAll('-', '').slice(0, 12)}`;
    const password = `Prueba${randomUUID()}`;
    await page.goto('/registro');
    await page.getByLabel('Nombre de usuario', { exact: true }).fill(user);
    await page.getByLabel('Correo electrónico', { exact: true }).fill(`${user}@example.test`);
    await page.getByLabel('Contraseña', { exact: true }).fill(password);
    await page.getByLabel('Confirmar contraseña').fill(password);
    await page.getByRole('button', { name: 'Registrarse', exact: true }).click();
    await expect(page).toHaveURL(/\/login$/);
    await page.getByLabel('Usuario o correo electrónico').fill(user);
    await page.getByLabel('Contraseña', { exact: true }).fill(password);
    await page.getByRole('button', { name: 'Ingresar', exact: true }).click();
    await expect(page.getByRole('heading', { name: 'Empleados registrados' })).toBeVisible();
    await expect(page.getByLabel('Paginación de empleados')).toContainText('Página 1 de 2');
    await page.reload();
    await expect(page.getByRole('heading', { name: 'Empleados registrados' })).toBeVisible();
    await page.getByRole('button', { name: 'Siguiente' }).click();
    await expect(page.getByLabel('Paginación de empleados')).toContainText('Página 2 de 2');
    await page.getByRole('button', { name: 'Anterior' }).click();

    await page.getByRole('link', { name: 'Nuevo empleado' }).click();
    await page.getByRole('button', { name: 'Guardar empleado' }).click();
    await expect(page.getByText('El DNI es obligatorio.').first()).toBeVisible();
    await page.getByLabel('DNI', { exact: true }).fill('90000001');
    await page.getByLabel('Nombre', { exact: true }).fill('Prueba');
    await page.getByLabel('Apellido', { exact: true }).fill('Integrador');
    await page.getByLabel('Departamento', { exact: true }).selectOption('1');
    await page.getByLabel('Localidad', { exact: true }).selectOption('1865');
    await page.getByRole('button', { name: 'Guardar empleado' }).click();
    await expect(page.getByText('El DNI ya está registrado en otro empleado.').first()).toBeVisible();
    await page.getByLabel('DNI', { exact: true }).fill('12345678');
    await page.getByRole('button', { name: 'Guardar empleado' }).click();
    await expect(page).toHaveURL(/\/empleados$/);

    await page.getByLabel('Buscar por DNI, nombre o apellido').fill('Integrador');
    await page.getByRole('button', { name: 'Buscar', exact: true }).click();
    await expect(page.getByLabel('Paginación de empleados')).toContainText('1 registros');
    await page.getByRole('link', { name: 'Editar a Prueba Integrador' }).click();
    await page.getByLabel('Nombre', { exact: true }).fill('Prueba Editada');
    await page.getByRole('button', { name: 'Actualizar', exact: true }).click();
    await page.getByRole('link', { name: 'Ver a Prueba Editada Integrador' }).click();
    await expect(page.getByText('Prueba Editada', { exact: true })).toBeVisible();
    await page.getByRole('link', { name: 'Volver', exact: true }).click();
    await page.getByRole('button', { name: 'Eliminar a Prueba Editada Integrador' }).click();
    await page.getByRole('button', { name: 'Cancelar', exact: true }).click();
    await expect(page.getByRole('link', { name: 'Ver a Prueba Editada Integrador' })).toBeVisible();
    await page.getByRole('button', { name: 'Eliminar a Prueba Editada Integrador' }).click();
    await page.getByRole('button', { name: 'Eliminar', exact: true }).click();
    await expect(page.getByText('No hay empleados para mostrar.')).toBeVisible();
    await page.getByRole('button', { name: 'Limpiar', exact: true }).click();
    await expect(page.getByLabel('Paginación de empleados')).toContainText('12 registros');

    const themeButton = page.getByRole('button', { name: /Modo (claro|oscuro)/ });
    if ((await themeButton.textContent()).trim() === 'Modo claro') await themeButton.click();
    await expect(page.locator('html')).toHaveAttribute('data-bs-theme', 'light');
    if (process.env.CAPTURE_SCREENSHOTS === '1')
        await page.screenshot({ path: path.resolve('../../docs/capturas/empleados-claro.png'), fullPage: true });
    await page.getByRole('button', { name: 'Modo oscuro', exact: true }).click();
    await page.reload();
    await expect(page.locator('html')).toHaveAttribute('data-bs-theme', 'dark');
    await expect(page.getByRole('heading', { name: 'Empleados registrados' })).toBeVisible();
    await page.setViewportSize({ width: 375, height: 812 });
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBeTruthy();
    if (process.env.CAPTURE_SCREENSHOTS === '1')
        await page.screenshot({ path: path.resolve('../../docs/capturas/empleados-oscuro-movil.png'), fullPage: true });
    await page.getByRole('button', { name: 'Cerrar sesión' }).click();
    await expect(page).toHaveURL(/\/login$/);
    await page.goto('/empleados/nuevo');
    await expect(page).toHaveURL(/\/login$/);
    expect(errors).toEqual([]);
});
