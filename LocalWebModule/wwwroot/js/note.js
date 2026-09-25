let currentUser = null;
let notes = [];
let folders = [];

let currentNoteId = null;
let currentMode = "all";


document.addEventListener("DOMContentLoaded", async function () {

    currentUser = getCurrentUser();

    if (!currentUser) {

        window.location.href =
            "/pages/login.html";

        return;
    }


    document.getElementById("currentUser").textContent =
        currentUser.login;


    document
        .getElementById("createNoteButton")
        .addEventListener("click", createNewNote);


    document
        .getElementById("saveNoteButton")
        .addEventListener("click", saveNote);


    document
        .getElementById("cancelNoteButton")
        .addEventListener("click", closeEditor);


    document
        .getElementById("allNotesButton")
        .addEventListener("click", function () {

            currentMode = "all";

            setActiveButton(this);

            loadNotes();
        });


    document
        .getElementById("favoriteButton")
        .addEventListener("click", function () {

            currentMode = "favorite";

            setActiveButton(this);

            renderNotes();
        });


    document
        .getElementById("archiveButton")
        .addEventListener("click", function () {

            currentMode = "archive";

            setActiveButton(this);

            renderNotes();
        });


    document
        .getElementById("searchInput")
        .addEventListener("input", function () {

            renderNotes();
        });


    await loadFolders();
    await loadNotes();

});


async function loadNotes() {

    try {

        const response = await fetch(
            `/ api / notes / ${ currentUser.id }/owner`
        );

if (!response.ok) {

    console.error(
        "Ошибка загрузки заметок:",
        response.status
    );

    return;
}

notes = await response.json();

renderNotes();

    }
    catch (error) {

    console.error(error);
}
}


async function loadFolders() {

    try {

        const response = await fetch(
            `/api/folders/${currentUser.id}/folder`
        );

        if (!response.ok) {

            console.error(
                "Ошибка загрузки папок:",
                response.status
            );

            return;
        }

        folders = await response.json();

        renderFolders();
        renderFolderSelect();

    }
    catch (error) {

        console.error(error);
    }
}


function renderFolders() {

    const container =
        document.getElementById("folderList");

    container.innerHTML = "";

    if (folders.length === 0) {

        container.innerHTML =
            `<div class="empty-sidebar">
                Папок пока нет
            </div>`;

        return;
    }


    folders.forEach(function (folder) {

        const button =
            document.createElement("button");

        button.className = "folder-item";

        button.textContent =
            "📁 " + folder.title;

        button.addEventListener(
            "click",
            function () {

                loadFolderNotes(folder.id);
            }
        );

        container.appendChild(button);

    });
}


function renderFolderSelect() {

    const select =
        document.getElementById("noteFolder");

    select.innerHTML =
        `<option value="">
            Без папки
        </option>`;


    folders.forEach(function (folder) {

        const option =
            document.createElement("option");

        option.value = folder.id;
        option.textContent = folder.title;

        select.appendChild(option);

    });
}


async function loadFolderNotes(folderId) {

    try {

        const response = await fetch(
            `/api/notes/${folderId}/folder`
        );

        if (!response.ok) {
            return;
        }

        notes = await response.json();

        currentMode = "folder";

        document.getElementById("pageTitle").textContent =
            "Папка";

        renderNotes();

    }
    catch (error) {

        console.error(error);
    }
}


function renderNotes() {

    const container =
        document.getElementById("notesContainer");

    container.innerHTML = "";


    let filteredNotes = [...notes];


    if (currentMode === "favorite") {

        filteredNotes =
            filteredNotes.filter(
                note => note.isFavorite
            );
    }


    if (currentMode === "archive") {

        filteredNotes =
            filteredNotes.filter(
                note => note.isArchive
            );
    }


    const search =
        document
            .getElementById("searchInput")
            .value
            .toLowerCase()
            .trim();


    if (search !== "") {

        filteredNotes =
            filteredNotes.filter(note =>
                note.title.toLowerCase().includes(search) ||
                note.content.toLowerCase().includes(search)
            );
    }


    document.getElementById("noteCount").textContent =
        `${filteredNotes.length} заметок`;


    if (filteredNotes.length === 0) {

        container.innerHTML = `
            <div class="empty-notes">
                <h2>Заметок пока нет</h2>
                <p>
                    Создайте новую заметку,
                    чтобы начать работу.
                </p>
            </div>
        `;

        return;
    }


    filteredNotes.forEach(function (note) {

        const card =
            document.createElement("article");

        card.className = "note-card";


        const favorite =
            note.isFavorite
                ? "★"
                : "☆";


        card.innerHTML = `

            <div class="note-card-header">

                <h2>
                    ${escapeHtml(note.title)}
                </h2>

                <button
                    class="favorite-button"
                    onclick="toggleFavorite('${note.id}')"
                >
                    ${favorite}
                </button>

            </div>

            <p class="note-preview">
                ${escapeHtml(note.content)}
            </p>

            <div class="note-card-footer">

                <span>
                    ${formatDate(note.updatedAt)}
                </span>

                <div>

                    <button
                        onclick="editNote('${note.id}')"
                    >
                        Открыть
                    </button>

                    ${note.isArchive
                ?
                `<button
                            onclick="restoreNote('${note.id}')"
                        >
                            Восстановить
                        </button>`
                :
                `<button
                            onclick="archiveNote('${note.id}')"
                        >
                            Архив
                        </button>`
            }

                    <button
                        onclick="deleteNote('${note.id}')"
                    >
                        Удалить
                    </button>

                </div>

            </div>
        `;


        container.appendChild(card);

    });
}


function createNewNote() {

    currentNoteId = null;

    document.getElementById("noteTitle").value = "";
    document.getElementById("noteContent").value = "";
    document.getElementById("noteFolder").value = "";
    document.getElementById("noteType").value = "";

    document
        .getElementById("editor")
        .classList.remove("hidden");

    document
        .getElementById("noteTitle")
        .focus();
}


function editNote(id) {

    const note =
        notes.find(n => n.id === id);

    if (!note) {
        return;
    }


    currentNoteId = id;

    document.getElementById("noteTitle").value =
        note.title;

    document.getElementById("noteContent").value =
        note.content;

    document.getElementById("noteFolder").value =
        note.folderId || "";

    document.getElementById("noteType").value =
        note.noteType || "";


    document
        .getElementById("editor")
        .classList.remove("hidden");
}


async function saveNote() {

    const title =
        document.getElementById("noteTitle").value;

    const content =
        document.getElementById("noteContent").value;

    const folderId =
        document.getElementById("noteFolder").value;

    const noteType =
        document.getElementById("noteType").value;


    if (title.trim() === "") {

        alert("Введите название заметки.");

        return;
    }


    try {

        let response;


        if (currentNoteId === null) {

            const request = {

                title: title,

                content: content,

                ownerId: currentUser.id,

                folderId:
                    folderId === ""
                        ? null
                        : folderId,

                isFavorite: false,

                isArchive: false,

                noteType:
                    noteType === ""
                        ? null
                        : noteType
            };


            response = await fetch(
                "/api/notes",
                {
                    method: "POST",

                    headers: {
                        "Content-Type":
                            "application/json"
                    },

                    body:
                        JSON.stringify(request)
                }
            );

        }
        else {

            const oldNote =
                notes.find(
                    note => note.id === currentNoteId
                );


            const request = {

                title: title,

                content: content,

                folderId:
                    folderId === ""
                        ? null
                        : folderId,

                isFavorite:
                    oldNote.isFavorite,

                isArchive:
                    oldNote.isArchive,

                noteType:
                    noteType === ""
                        ? null
                        : noteType
            };


            response = await fetch(
                `/api/notes/${currentNoteId}`,
                {
                    method: "PUT",

                    headers: {
                        "Content-Type":
                            "application/json"
                    },

                    body:
                        JSON.stringify(request)
                }
            );
        }


        if (!response.ok) {

            const error =
                await response.text();

            alert(error);

            return;
        }


        closeEditor();

        await loadNotes();

    }
    catch (error) {

        console.error(error);

        alert(
            "Не удалось сохранить заметку."
        );
    }
}


async function deleteNote(id) {

    if (!confirm(
        "Удалить эту заметку?"
    )) {
        return;
    }


    try {

        const response =
            await fetch(
                `/api/notes/${id}`,
                {
                    method: "DELETE"
                }
            );


        if (!response.ok) {

            alert(
                "Не удалось удалить заметку."
            );

            return;
        }


        await loadNotes();

    }
    catch (error) {

        console.error(error);
    }
}


async function toggleFavorite(id) {

    try {

        const response =
            await fetch(
                `/api/notes/${id}/favorite`,
                {
                    method: "POST"
                }
            );


        if (!response.ok) {
            return;
        }


        await loadNotes();

    }
    catch (error) {

        console.error(error);
    }
}


async function archiveNote(id) {

    try {

        const response =
            await fetch(
                `/api/notes/${id}/archive`,
                {
                    method: "POST"
                }
            );


        if (!response.ok) {
            return;
        }


        await loadNotes();

    }
    catch (error) {

        console.error(error);
    }
}


async function restoreNote(id) {

    try {

        const response =
            await fetch(
                `/api/notes/${id}/restore`,
                {
                    method: "POST"
                }
            );


        if (!response.ok) {
            return;
        }


        await loadNotes();

    }
    catch (error) {

        console.error(error);
    }
}


function closeEditor() {

    currentNoteId = null;

    document
        .getElementById("editor")
        .classList.add("hidden");
}


function setActiveButton(button) {

    document
        .querySelectorAll(".sidebar-button")
        .forEach(
            element =>
                element.classList.remove("active")
        );


    button.classList.add("active");
}


function formatDate(date) {

    if (!date) {
        return "";
    }

    return new Date(date).toLocaleString(
        "ru-RU",
        {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit"
        }
    );
}


function escapeHtml(value) {

    if (!value) {
        return "";
    }

    return value
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}
