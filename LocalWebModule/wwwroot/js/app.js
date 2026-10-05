let currentUser = null;

let folders = [];
let notes = [];

let currentNoteId = null;
let currentNoteFolderId = null;

let selectedView = "all";
let selectedFolderId = null;


// ========================================
// START
// ========================================

document.addEventListener("DOMContentLoaded", async function () {
    if (!checkAuthorization()) {
        return;
    }

    setupEvents();
    loadAccount();
    await loadData();
});


// ========================================
// AUTHORIZATION
// ========================================

function checkAuthorization() {
    const userJson = sessionStorage.getItem("currentUser");

    if (!userJson) {
        window.location.href = "/login.html";
        return false;
    }

    try {
        currentUser = JSON.parse(userJson);
    }
    catch {
        sessionStorage.removeItem("currentUser");
        window.location.href = "/login.html";
        return false;
    }

    if (!currentUser.id) {
        sessionStorage.removeItem("currentUser");
        window.location.href = "/login.html";
        return false;
    }

    return true;
}


// ========================================
// EVENTS
// ========================================

function setupEvents() {
    document.getElementById("sidebar_toggle")
        .addEventListener("click", toggleSidebar);

    document.getElementById("account_button")
        .addEventListener("click", openAccount);

    document.getElementById("logout_button")
        .addEventListener("click", logout);

    document.getElementById("refresh_button")
        .addEventListener("click", loadData);

    document.getElementById("new_note_button")
        .addEventListener("click", createNewNote);

    document.getElementById("panel_new_note_button")
        .addEventListener("click", createNewNote);

    document.getElementById("empty_new_note_button")
        .addEventListener("click", createNewNote);

    document.getElementById("save_note_button")
        .addEventListener("click", saveNote);

    document.getElementById("delete_note_button")
        .addEventListener("click", deleteCurrentNote);

    document.getElementById("favorite_button")
        .addEventListener("click", toggleFavorite);

    document.getElementById("archive_button")
        .addEventListener("click", archiveCurrentNote);

    document.getElementById("new_folder_button")
        .addEventListener("click", openFolderModal);

    document.getElementById("create_folder_button")
        .addEventListener("click", createFolder);
}


// ========================================
// DATA
// ========================================

async function loadData() {
    await Promise.all([
        loadFolders(),
        loadNotes()
    ]);

    renderNavigationTree();
    renderNotesList();
}


async function loadFolders() {
    const response = await apiRequest(
        `/api/folders/${currentUser.id}/folder`
    );

    if (!response.ok) {
        console.error(response.data);
        return;
    }

    folders = response.data || [];
}


async function loadNotes() {
    const response = await apiRequest(
        `/api/notes/${currentUser.id}/owner`
    );

    if (!response.ok) {
        console.error(response.data);
        return;
    }

    notes = response.data || [];
}


// ========================================
// ACCOUNT
// ========================================

function loadAccount() {
    if (!currentUser) {
        return;
    }

    const login = currentUser.login || "Пользователь";
    const email = currentUser.email || "";
    const firstLetter = login.charAt(0).toUpperCase() || "?";

    document.getElementById("sidebar_user_login").textContent = login;
    document.getElementById("sidebar_user_email").textContent = email;
    document.getElementById("account_avatar").textContent = firstLetter;
    document.getElementById("modal_account_avatar").textContent = firstLetter;

    document.getElementById("account_login").textContent = login;
    document.getElementById("account_email").textContent = email;
    document.getElementById("account_id").textContent = currentUser.id;
    document.getElementById("account_created").textContent =
        formatDate(currentUser.createdAt);
}


function openAccount() {
    const modal = new bootstrap.Modal(
        document.getElementById("accountModal")
    );

    modal.show();
}


function logout() {
    sessionStorage.removeItem("currentUser");
    window.location.href = "/login.html";
}


// ========================================
// SIDEBAR
// ========================================

function toggleSidebar() {
    document.getElementById("sidebar")
        .classList.toggle("collapsed");
}


function renderNavigationTree() {
    const container = document.getElementById("navigation_tree");
    container.innerHTML = "";

    addSpecialNavigation(container, "all", "Все заметки", "bi-journal-text");
    addSpecialNavigation(container, "favorite", "Избранное", "bi-star");
    addSpecialNavigation(container, "archive", "Архив", "bi-archive");

    const separator = document.createElement("div");
    separator.className = "tree-separator";
    container.appendChild(separator);

    const rootFolders = folders.filter(function (folder) {
        return folder.parentFolderId == null;
    });

    rootFolders.forEach(function (folder) {
        renderFolderTree(folder, container, 0);
    });

    const rootNotes = notes.filter(function (note) {
        return note.folderId == null && !note.isArchive;
    });

    if (rootNotes.length > 0) {
        const group = document.createElement("div");
        group.className = "tree-group";

        rootNotes.forEach(function (note) {
            group.appendChild(createTreeNote(note, 0));
        });

        container.appendChild(group);
    }

    if (rootFolders.length === 0 && rootNotes.length === 0) {
        const empty = document.createElement("div");
        empty.className = "empty-list";
        empty.textContent = "Папок пока нет";
        container.appendChild(empty);
    }
}


function addSpecialNavigation(container, view, title, icon) {
    const button = document.createElement("button");
    button.className = "tree-item tree-special";

    if (selectedView === view) {
        button.classList.add("active");
    }

    button.innerHTML = `
        <i class="bi ${icon}"></i>
        <span class="tree-name">${title}</span>
    `;

    button.addEventListener("click", function () {
        selectedView = view;
        selectedFolderId = null;
        currentNoteId = null;
        currentNoteFolderId = null;

        clearEditor();
        renderNavigationTree();
        renderNotesList();
        updatePageHeader();
    });

    container.appendChild(button);
}


function renderFolderTree(folder, container, level) {
    const folderButton = document.createElement("button");
    folderButton.className = "tree-item tree-folder";

    if (selectedView === "folder" && selectedFolderId === folder.id) {
        folderButton.classList.add("active");
    }

    folderButton.style.paddingLeft = (9 + level * 16) + "px";

    folderButton.innerHTML = `
        <span class="tree-arrow">›</span>
        <i class="bi bi-folder"></i>
        <span class="tree-name"></span>
    `;

    folderButton.querySelector(".tree-name").textContent = folder.title;

    folderButton.addEventListener("click", function () {
        selectedView = "folder";
        selectedFolderId = folder.id;
        currentNoteId = null;
        currentNoteFolderId = null;

        clearEditor();
        renderNavigationTree();
        renderNotesList();
        updatePageHeader();
    });

    container.appendChild(folderButton);

    const group = document.createElement("div");
    group.className = "tree-group";

    const childFolders = folders.filter(function (child) {
        return child.parentFolderId === folder.id;
    });

    childFolders.forEach(function (child) {
        renderFolderTree(child, group, level + 1);
    });

    const folderNotes = notes.filter(function (note) {
        return note.folderId === folder.id && !note.isArchive;
    });

    folderNotes.forEach(function (note) {
        group.appendChild(createTreeNote(note, level + 1));
    });

    container.appendChild(group);
}


function createTreeNote(note, level) {
    const button = document.createElement("button");
    button.className = "tree-item tree-note";

    if (currentNoteId === note.id) {
        button.classList.add("active");
    }

    if (note.isFavorite) {
        button.classList.add("favorite");
    }

    button.style.paddingLeft = (9 + level * 16) + "px";

    button.innerHTML = `
        <i class="bi bi-file-earmark-text"></i>
        <span class="tree-name"></span>
    `;

    button.querySelector(".tree-name").textContent = note.title;

    button.addEventListener("click", function () {
        openNote(note);
    });

    return button;
}


// ========================================
// NOTE LIST
// ========================================

function getVisibleNotes() {
    if (selectedView === "archive") {
        return notes.filter(function (note) {
            return note.isArchive;
        });
    }

    if (selectedView === "favorite") {
        return notes.filter(function (note) {
            return note.isFavorite && !note.isArchive;
        });
    }

    if (selectedView === "folder") {
        return notes.filter(function (note) {
            return note.folderId === selectedFolderId && !note.isArchive;
        });
    }

    return notes.filter(function (note) {
        return !note.isArchive;
    });
}


function renderNotesList() {
    const container = document.getElementById("notes_list");
    container.innerHTML = "";

    const visibleNotes = getVisibleNotes();

    document.getElementById("notes_count").textContent =
        visibleNotes.length + " " + getNoteWord(visibleNotes.length);

    if (visibleNotes.length === 0) {
        container.innerHTML = `
            <div class="empty-list">
                Здесь пока нет заметок
            </div>
        `;

        updatePageHeader();
        return;
    }

    visibleNotes.sort(function (a, b) {
        return new Date(b.updatedAt) - new Date(a.updatedAt);
    });

    visibleNotes.forEach(function (note) {
        const button = document.createElement("button");
        button.className = "note-list-item";

        if (note.id === currentNoteId) {
            button.classList.add("active");
        }

        const type = getNoteTypeLabel(note.noteType);
        const preview = note.content || "Нет текста";

        button.innerHTML = `
            <div class="note-list-title"></div>
            <div class="note-list-preview"></div>
            <div class="note-list-meta">
                <span class="note-type-badge"></span>
                <span>${formatDate(note.updatedAt)}</span>
            </div>
        `;

        button.querySelector(".note-list-title").textContent =
            (note.isFavorite ? "★ " : "") + note.title;

        button.querySelector(".note-list-preview").textContent = preview;
        button.querySelector(".note-type-badge").textContent = type;

        button.addEventListener("click", function () {
            openNote(note);
        });

        container.appendChild(button);
    });

    updatePageHeader();
}


function updatePageHeader() {
    const title = document.getElementById("page_title");
    const subtitle = document.getElementById("page_subtitle");

    if (selectedView === "favorite") {
        title.textContent = "Избранное";
        subtitle.textContent = "Избранные заметки";
        return;
    }

    if (selectedView === "archive") {
        title.textContent = "Архив";
        subtitle.textContent = "Архивные заметки";
        return;
    }

    if (selectedView === "folder") {
        const folder = folders.find(function (item) {
            return item.id === selectedFolderId;
        });

        title.textContent = folder ? folder.title : "Папка";
        subtitle.textContent = "Заметки в папке";
        return;
    }

    title.textContent = "Все заметки";
    subtitle.textContent = "Ваши заметки без архива";
}


// ========================================
// FOLDER CREATE
// ========================================

function openFolderModal() {
    document.getElementById("folder_title").value = "";

    const modal = new bootstrap.Modal(
        document.getElementById("folderModal")
    );

    modal.show();
}


async function createFolder() {
    const title = document.getElementById("folder_title").value.trim();

    if (title === "") {
        alert("Введите название папки");
        return;
    }

    const parentFolderId =
        selectedView === "folder"
            ? selectedFolderId
            : null;

    const request = {
        title: title,
        ownerId: currentUser.id,
        parentFolderId: parentFolderId
    };

    const response = await apiRequest(
        "/api/folders",
        {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        }
    );

    if (!response.ok) {
        alert("Не удалось создать папку: " + response.data);
        console.error(response.data);
        return;
    }

    const modalElement = document.getElementById("folderModal");
    const modal = bootstrap.Modal.getInstance(modalElement);

    if (modal) {
        modal.hide();
    }

    await loadData();
}


// ========================================
// NOTE EDITOR
// ========================================

function openNote(note) {
    currentNoteId = note.id;
    currentNoteFolderId = note.folderId || null;

    document.getElementById("empty_editor").classList.add("d-none");
    document.getElementById("note_editor").classList.remove("d-none");

    document.getElementById("note_title").value = note.title || "";
    document.getElementById("note_content").value = note.content || "";
    document.getElementById("note_type").value = note.noteType || "personal";

    const folder = folders.find(function (item) {
        return item.id === note.folderId;
    });

    document.getElementById("note_folder_name").innerHTML =
        `<i class="bi bi-folder"></i> ${folder ? escapeHtml(folder.title) : "Без папки"}`;

    document.getElementById("note_updated").textContent =
        "Изменено: " + formatDate(note.updatedAt);

    updateEditorButtons(note);
    renderNavigationTree();
    renderNotesList();
}


function createNewNote() {
    currentNoteId = null;
    currentNoteFolderId =
        selectedView === "folder"
            ? selectedFolderId
            : null;

    document.getElementById("empty_editor").classList.add("d-none");
    document.getElementById("note_editor").classList.remove("d-none");

    document.getElementById("note_title").value = "";
    document.getElementById("note_content").value = "";
    document.getElementById("note_type").value = "personal";
    document.getElementById("note_status").textContent = "Новая заметка";
    document.getElementById("note_updated").textContent = "";

    const folder = folders.find(function (item) {
        return item.id === currentNoteFolderId;
    });

    document.getElementById("note_folder_name").innerHTML =
        `<i class="bi bi-folder"></i> ${folder ? escapeHtml(folder.title) : "Без папки"}`;

    document.getElementById("favorite_button").innerHTML =
        `<i class="bi bi-star"></i> Избранное`;

    document.getElementById("archive_button").innerHTML =
        `<i class="bi bi-archive"></i> Архив`;

    document.getElementById("note_title").focus();
}


function clearEditor() {
    currentNoteId = null;
    currentNoteFolderId = null;

    document.getElementById("empty_editor").classList.remove("d-none");
    document.getElementById("note_editor").classList.add("d-none");
}


function updateEditorButtons(note) {
    const favoriteButton = document.getElementById("favorite_button");
    const archiveButton = document.getElementById("archive_button");

    if (note.isFavorite) {
        favoriteButton.innerHTML =
            `<i class="bi bi-star-fill"></i> Избранное`;
    }
    else {
        favoriteButton.innerHTML =
            `<i class="bi bi-star"></i> Избранное`;
    }

    if (note.isArchive) {
        archiveButton.innerHTML =
            `<i class="bi bi-arrow-counterclockwise"></i> Восстановить`;
    }
    else {
        archiveButton.innerHTML =
            `<i class="bi bi-archive"></i> Архив`;
    }
}


// ========================================
// SAVE NOTE
// ========================================

async function saveNote() {
    const title = document.getElementById("note_title").value.trim();
    const content = document.getElementById("note_content").value;
    const noteType = document.getElementById("note_type").value;

    if (title === "") {
        alert("Введите название заметки");
        return;
    }

    let response;

    if (currentNoteId) {
        const currentNote = notes.find(function (note) {
            return note.id === currentNoteId;
        });

        const request = {
            title: title,
            content: content,
            folderId: currentNoteFolderId,
            isFavorite: currentNote ? currentNote.isFavorite : false,
            isArchive: currentNote ? currentNote.isArchive : false,
            noteType: noteType
        };

        response = await apiRequest(
            `/api/notes/${currentNoteId}`,
            {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(request)
            }
        );
    }
    else {
        const request = {
            title: title,
            content: content,
            ownerId: currentUser.id,
            folderId: currentNoteFolderId,
            isFavorite: false,
            isArchive: false,
            noteType: noteType
        };

        response = await apiRequest(
            "/api/notes",
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(request)
            }
        );
    }

    if (!response.ok) {
        alert("Не удалось сохранить заметку: " + response.data);
        console.error(response.data);
        return;
    }

    if (!currentNoteId && response.data) {
        currentNoteId = response.data.id;
        currentNoteFolderId = response.data.folderId || null;
    }

    document.getElementById("note_status").textContent = "Сохранено";

    await loadData();

    const savedNote = notes.find(function (note) {
        return note.id === currentNoteId;
    });

    if (savedNote) {
        openNote(savedNote);
    }
}


// ========================================
// FAVORITE
// ========================================

async function toggleFavorite() {
    if (!currentNoteId) {
        return;
    }

    const response = await apiRequest(
        `/api/notes/${currentNoteId}/favorite`,
        {
            method: "POST"
        }
    );

    if (!response.ok) {
        alert("Не удалось изменить избранное");
        return;
    }

    await loadData();

    const note = notes.find(function (item) {
        return item.id === currentNoteId;
    });

    if (note) {
        openNote(note);
    }
}


// ========================================
// ARCHIVE / RESTORE
// ========================================

async function archiveCurrentNote() {
    if (!currentNoteId) {
        return;
    }

    const currentNote = notes.find(function (note) {
        return note.id === currentNoteId;
    });

    if (!currentNote) {
        return;
    }

    const endpoint = currentNote.isArchive
        ? `/api/notes/${currentNoteId}/restore`
        : `/api/notes/${currentNoteId}/archive`;

    const response = await apiRequest(
        endpoint,
        {
            method: "POST"
        }
    );

    if (!response.ok) {
        alert("Не удалось изменить состояние архива");
        return;
    }

    currentNoteId = null;
    currentNoteFolderId = null;

    clearEditor();
    await loadData();
}


// ========================================
// DELETE
// ========================================

async function deleteCurrentNote() {
    if (!currentNoteId) {
        return;
    }

    if (!confirm("Удалить эту заметку?")) {
        return;
    }

    const response = await apiRequest(
        `/api/notes/${currentNoteId}`,
        {
            method: "DELETE"
        }
    );

    if (!response.ok) {
        alert("Не удалось удалить заметку");
        return;
    }

    clearEditor();
    await loadData();
}


// ========================================
// HELPERS
// ========================================

function getNoteTypeLabel(type) {
    switch (type) {
        case "personal":
            return "Личное";
        case "work":
            return "Работа";
        case "study":
            return "Учёба";
        case "project":
            return "Проект";
        case "other":
            return "Другое";
        default:
            return "Без типа";
    }
}


function getNoteWord(number) {
    if (number % 10 === 1 && number % 100 !== 11) {
        return "заметка";
    }

    if (number % 10 >= 2 && number % 10 <= 4 &&
        (number % 100 < 10 || number % 100 >= 20)) {
        return "заметки";
    }

    return "заметок";
}


function formatDate(date) {
    if (!date) {
        return "-";
    }

    const value = new Date(date);

    if (isNaN(value)) {
        return date;
    }

    return value.toLocaleString("ru-RU");
}


function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}
