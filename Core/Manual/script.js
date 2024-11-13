const pages = document.querySelectorAll(".menu-item");
const docs = document.getElementById("docs");
var current = 0;

pages.forEach((item) => {
    item.addEventListener('click', active_item);
})

document.querySelectorAll(".move-btn").forEach((item) => {
    item.addEventListener('click', switch_page);
})

function active_item() {
    var i = 0;

    pages.forEach((item) => {
        item.classList.remove('is-active');

        if (item.id == this.id) {
            current = i;
        }

        i++;
    });

    this.classList.add('is-active');
    const theme = encodeURIComponent(document.documentElement.getAttribute('data-theme'));
    docs.setAttribute('src', "docs/" + this.id + ".html?theme=" + theme);
    document.title = this.textContent;
}

function switch_page() {
    if (this.id == 'next') {
        current = Math.min(current + 1, pages.length - 1);
    } else {
        current = Math.max(current - 1, 0);
    }

    pages[current].dispatchEvent(new Event("click"));
}

function getPage(variable) {
    var query = window.location.search.substring(1);
    var vars = query.split('&');
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split('=');
        if (decodeURIComponent(pair[0]) == variable) {
            return decodeURIComponent(pair[1]);
        }
    }
}

const page = getPage("page");
if (page != null) {
    pages[page].dispatchEvent(new Event("click"));
}