var query = window.location.search.substring(1);
if (query == "theme=dark") {
    document.documentElement.setAttribute('data-theme', 'dark');
} else {
    document.documentElement.setAttribute('data-theme', 'light');
}