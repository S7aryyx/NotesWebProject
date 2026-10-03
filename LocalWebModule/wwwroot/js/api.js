const USER_URL = "/api/auth";
const NOTE_URL = "/api/notes";
const FOLDER_URL = "/api/folders";


async function call_api(url, options = {}) {
    try {
        const response = await fetch(url, options);

        const text = await response.text();

        let data = null;

        if (text) {
            try {
                data = JSON.parse(text);
            }
            catch {
                data = text;
            }
        }

        return {
            ok: response.ok,
            status: response.status,
            data: data
        };
    }
    catch (error) {
        return {
            ok: false,
            status: 0,
            data: "Ошибка соединения с API: " + error.message
        };
    }
}


// =====================================================
// USER API
// =====================================================

async function Login(login, password) {
    const request = {
        login: login,
        password: password
    };

    return await call_api(
        `${USER_URL}/login`,
        {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        }
    );
}


async function Register(email, login, password) {
    const request = {
        email: email,
        login: login,
        password: password
    };

    return await call_api(
        `${USER_URL}/register`,
        {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        }
    );
}


async function GetAllUsers() {
    return await call_api(USER_URL);
}


async function GetUserById(id) {
    return await call_api(`${USER_URL}/${id}`);
}


async function GetUserByLogin(login) {
    return await call_api(
        `${USER_URL}/${encodeURIComponent(login)}`
    );
}


async function GetUserByEmail(email) {
    return await call_api(
        `${USER_URL}/email/${encodeURIComponent(email)}`
    );
}


async function UpdateUser(id, email, login, password) {
    const request = {
        email: email,
        login: login,
        password: password
    };

    return await call_api(
        `${USER_URL}/${id}`,
        {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        }
    );
}


async function DeleteUser(id) {
    return await call_api(
        `${USER_URL}/${id}`,
        {
            method: "DELETE"
        }
    );
}


// =====================================================
// FOLDER API
// =====================================================

async function GetAllFolders() {
    return await call_api(FOLDER_URL);
}


async function GetFolderById(id) {
    return await call_api(`${FOLDER_URL}/${id}`);
}


async function GetFoldersByOwner(ownerId) {
    return await call_api(
        `${FOLDER_URL}/${ownerId}/folder`
    );
}


async function GetChildFolders(ownerId, parentFolderId) {
    return await call_api(
        `${FOLDER_URL}/${ownerId}/parent/${parentFolderId}/owner`
    );
}


async function GetRootFolders(ownerId) {
    return await call_api(
        `${FOLDER_URL}/${ownerId}/owner/root`
    );
}


async function CreateFolder(title, ownerId, parentFolderId) {
    const request = {
        title: title,
        ownerId: ownerId,
        parentFolderId: parentFolderId || null
    };

    return await call_api(
        FOLDER_URL,
        {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        }
    );
}


async function UpdateFolder(id, title, parentFolderId) {
    const request = {
        title: title,
        parentFolderId: parentFolderId || null
    };

    return await call_api(
        `${FOLDER_URL}/${id}`,
        {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        }
    );
}


async function DeleteFolder(id) {
    return await call_api(
        `${FOLDER_URL}/${id}`,
        {
            method: "DELETE"
        }
    );
}


async function DeleteFoldersByOwner(ownerId) {
    return await call_api(
        `${FOLDER_URL}/${ownerId}/delete_all`,
        {
            method: "DELETE"
        }
    );
}


// =====================================================
// NOTE API
// =====================================================

async function GetAllNotes() {
    return await call_api(NOTE_URL);
}


async function GetNoteById(id) {
    return await call_api(`${NOTE_URL}/${id}`);
}


async function GetNotesByOwner(ownerId) {
    return await call_api(
        `${NOTE_URL}/${ownerId}/owner`
    );
}


async function GetNotesByFolder(folderId) {
    return await call_api(
        `${NOTE_URL}/${folderId}/folder`
    );
}


async function CreateNote(
    title,
    content,
    ownerId,
    folderId,
    isFavorite,
    isArchive,
    noteType
) {
    const request = {
        title: title,
        content: content,
        ownerId: ownerId,
        folderId: folderId || null,
        isFavorite: isFavorite,
        isArchive: isArchive,
        noteType: noteType || null
    };

    return await call_api(
        NOTE_URL,
        {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        }
    );
}


async function UpdateNote(
    id,
    title,
    content,
    folderId,
    isFavorite,
    isArchive,
    noteType
) {
    const request = {
        title: title,
        content: content,
        folderId: folderId || null,
        isFavorite: isFavorite,
        isArchive: isArchive,
        noteType: noteType || null
    };

    return await call_api(
        `${NOTE_URL}/${id}`,
        {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        }
    );
}


async function DeleteNote(id) {
    return await call_api(
        `${NOTE_URL}/${id}`,
        {
            method: "DELETE"
        }
    );
}


async function DeleteNotesByOwner(ownerId) {
    return await call_api(
        `${NOTE_URL}/${ownerId}/owner`,
        {
            method: "DELETE"
        }
    );
}


async function DeleteNotesByFolder(folderId) {
    return await call_api(
        `${NOTE_URL}/${folderId}/folder`,
        {
            method: "DELETE"
        }
    );
}


async function ToggleFavorite(id) {
    return await call_api(
        `${NOTE_URL}/${id}/favorite`,
        {
            method: "POST"
        }
    );
}


async function ArchiveNote(id) {
    return await call_api(
        `${NOTE_URL}/${id}/archive`,
        {
            method: "POST"
        }
    );
}


async function RestoreNote(id) {
    return await call_api(
        `${NOTE_URL}/${id}/restore`,
        {
            method: "POST"
        }
    );
}