const themeKey = "instituto.theme";

export function readTheme() {
    let saved;
    try { saved = localStorage.getItem(themeKey); } catch { /* Almacenamiento deshabilitado. */ }
    const theme = saved === "dark" || saved === "light"
        ? saved : (window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light");
    document.documentElement.dataset.bsTheme = theme;
    return theme;
}

export function setTheme(theme) {
    const value = theme === "dark" ? "dark" : "light";
    document.documentElement.dataset.bsTheme = value;
    try { localStorage.setItem(themeKey, value); } catch { /* El tema sigue funcionando en esta página. */ }
}

export async function copyLink(url) {
    if (!window.isSecureContext || !navigator.clipboard) return false;
    try {
        await navigator.clipboard.writeText(url);
        return true;
    } catch { return false; }
}
