function val(id) {
    const element = document.getElementById(id);

    if (!element) {
        return "";
    }

    return element.value;
}


function showResult(id, result) {
    const element = document.getElementById(id);

    if (!element) {
        return;
    }

    let output = "";

    output += "HTTP STATUS: " + result.status + "\n";
    output += "SUCCESS: " + result.ok + "\n\n";

    if (result.data !== null) {
        if (typeof result.data === "object") {
            output += JSON.stringify(result.data, null, 4);
        }
        else {
            output += result.data;
        }
    }
    else {
        output += "API не вернуло тело ответа.";
    }

    element.textContent = output;

    element.classList.remove("success");
    element.classList.remove("error");

    if (result.ok) {
        element.classList.add("success");
    }
    else {
        element.classList.add("error");
    }
}


// =====================================================
// USER
// =====================================================

async function run_Login() {
    const login = val("login_login");
    const password = val("login_password");

    const result = await Login(login, password);

    showResult("login_result", result);
}


async function run_Register() {
    const email = val("register_email");
    const login = val("register_login");
    const password = val("register_password");

    const result = await Register(
        email,
        login,
        password
    );

    showResult("register_result", result);
}


async function run_GetAllUsers() {
    const result = await GetAllUsers();

    showResult("get_all_users_result", result);
}


async function run_GetUserById() {
    const id = val("user_id");

    const result = await GetUserById(id);

    showResult("get_user_by_id_result", result);
}


async function run_GetUserByLogin() {
    const login = val("user_login");

    const result = await GetUserByLogin(login);

    showResult("get_user_by_login_result", result);
}


async function run_GetUserByEmail() {
    const email = val("user_email");

    const result = await GetUserByEmail(email);

    showResult("get_user_by_email_result", result);
}


async function run_UpdateUser() {
    const id = val("update_user_id");
    const email = val("update_user_email");
    const login = val("update_user_login");
    const password = val("update_user_password");

    const result = await UpdateUser(
        id,
        email,
        login,
        password
    );

    showResult("update_user_result", result);
}


async function run_DeleteUser() {
    const id = val("delete_user_id");

    const result = await DeleteUser(id);

    showResult("delete_user_result", result);
}


// =====================================================
// FOLDERS
// =====================================================

async function run_GetAllFolders() {
    const result = await GetAllFolders();

    showResult("get_all_folders_result", result);
}


async function run_GetFolderById() {
    const id = val("folder_id");

    const result = await GetFolderById(id);

    showResult("get_folder_by_id_result", result);
}


async function run_GetFoldersByOwner() {
    const ownerId = val("folders_owner_id");

    const result = await GetFoldersByOwner(ownerId);

    showResult("get_folders_by_owner_result", result);
}


async function run_GetChildFolders() {
    const ownerId = val("child_owner_id");
    const parentId = val("parent_folder_id");

    const result = await GetChildFolders(
        ownerId,
        parentId
    );

    showResult("get_child_folders_result", result);
}


async function run_GetRootFolders() {
    const ownerId = val("root_owner_id");

    const result = await GetRootFolders(ownerId);

    showResult("get_root_folders_result", result);
}


async function run_CreateFolder() {
    const title = val("create_folder_title");
    const ownerId = val("create_folder_owner_id");
    const parentId = val("create_folder_parent_id");

    const result = await CreateFolder(
        title,
        ownerId,
        parentId
    );

    showResult("create_folder_result", result);
}


async function run_UpdateFolder() {
    const id = val("update_folder_id");
    const title = val("update_folder_title");
    const parentId = val("update_folder_parent_id");

    const result = await UpdateFolder(
        id,
        title,
        parentId
    );

    showResult("update_folder_result", result);
}


async function run_DeleteFolder() {
    const id = val("delete_folder_id");

    const result = await DeleteFolder(id);

    showResult("delete_folder_result", result);
}


async function run_DeleteFoldersByOwner() {
    const ownerId = val("delete_folders_owner_id");

    const result = await DeleteFoldersByOwner(ownerId);

    showResult(
        "delete_folders_by_owner_result",
        result
    );
}


// =====================================================
// NOTES
// =====================================================

async function run_GetAllNotes() {
    const result = await GetAllNotes();

    showResult("get_all_notes_result", result);
}


async function run_GetNoteById() {
    const id = val("note_id");

    const result = await GetNoteById(id);

    showResult("get_note_by_id_result", result);
}


async function run_GetNotesByOwner() {
    const ownerId = val("notes_owner_id");

    const result = await GetNotesByOwner(ownerId);

    showResult("get_notes_by_owner_result", result);
}


async function run_GetNotesByFolder() {
    const folderId = val("notes_folder_id");

    const result = await GetNotesByFolder(folderId);

    showResult("get_notes_by_folder_result", result);
}


async function run_CreateNote() {
    const title = val("create_note_title");
    const content = val("create_note_content");
    const ownerId = val("create_note_owner_id");
    const folderId = val("create_note_folder_id");
    const noteType = val("create_note_type");

    const result = await CreateNote(
        title,
        content,
        ownerId,
        folderId,
        false,
        false,
        noteType
    );

    showResult("create_note_result", result);
}


async function run_UpdateNote() {
    const id = val("update_note_id");
    const title = val("update_note_title");
    const content = val("update_note_content");
    const folderId = val("update_note_folder_id");
    const favorite = document.getElementById("update_note_favorite").checked;
    const archive = document.getElementById("update_note_archive").checked;
    const noteType = val("update_note_type");

    const result = await UpdateNote(
        id,
        title,
        content,
        folderId,
        favorite,
        archive,
        noteType
    );

    showResult("update_note_result", result);
}


async function run_DeleteNote() {
    const id = val("delete_note_id");

    const result = await DeleteNote(id);

    showResult("delete_note_result", result);
}


async function run_DeleteNotesByOwner() {
    const ownerId = val("delete_notes_owner_id");

    const result = await DeleteNotesByOwner(ownerId);

    showResult(
        "delete_notes_by_owner_result",
        result
    );
}


async function run_DeleteNotesByFolder() {
    const folderId = val("delete_notes_folder_id");

    const result = await DeleteNotesByFolder(folderId);

    showResult(
        "delete_notes_by_folder_result",
        result
    );
}


async function run_ToggleFavorite() {
    const id = val("favorite_note_id");

    const result = await ToggleFavorite(id);

    showResult("favorite_result", result);
}


async function run_ArchiveNote() {
    const id = val("archive_note_id");

    const result = await ArchiveNote(id);

    showResult("archive_result", result);
}


async function run_RestoreNote() {
    const id = val("restore_note_id");

    const result = await RestoreNote(id);

    showResult("restore_result", result);
}