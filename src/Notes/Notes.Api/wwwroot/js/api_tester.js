const resultElement =
    document.getElementById("api_result");


function showResult(result) {
    resultElement.textContent =
        JSON.stringify(
            result,
            null,
            4
        );
}


// ========================================
// LOGIN
// ========================================

document
    .getElementById("test_login_button")
    .addEventListener(
        "click",
        async function () {
            const login =
                document.getElementById(
                    "test_login"
                ).value;


            const password =
                document.getElementById(
                    "test_password"
                ).value;


            const result =
                await apiRequest(
                    "/api/auth/login",
                    {
                        method: "POST",

                        headers:
                        {
                            "Content-Type":
                                "application/json"
                        },

                        body: JSON.stringify(
                            {
                                login: login,
                                password: password
                            }
                        )
                    }
                );


            showResult(result);
        }
    );


// ========================================
// REGISTER
// ========================================

document
    .getElementById("test_register_button")
    .addEventListener(
        "click",
        async function () {
            const email =
                document.getElementById(
                    "test_email"
                ).value;


            const login =
                document.getElementById(
                    "test_register_login"
                ).value;


            const password =
                document.getElementById(
                    "test_register_password"
                ).value;


            const result =
                await apiRequest(
                    "/api/auth/register",
                    {
                        method: "POST",

                        headers:
                        {
                            "Content-Type":
                                "application/json"
                        },

                        body: JSON.stringify(
                            {
                                email: email,
                                login: login,
                                password: password
                            }
                        )
                    }
                );


            showResult(result);
        }
    );


// ========================================
// GET ALL USERS
// ========================================

document
    .getElementById("get_users_button")
    .addEventListener(
        "click",
        async function () {
            const result =
                await apiRequest(
                    "/api/auth"
                );


            showResult(result);
        }
    );


// ========================================
// GET USER
// ========================================

document
    .getElementById("get_user_button")
    .addEventListener(
        "click",
        async function () {
            const id =
                document.getElementById(
                    "test_user_id"
                ).value;


            const result =
                await apiRequest(
                    `/api/auth/${id}`
                );


            showResult(result);
        }
    );


// ========================================
// GET OWNER FOLDERS
// ========================================

document
    .getElementById("get_owner_folders_button")
    .addEventListener(
        "click",
        async function () {
            const ownerId =
                document.getElementById(
                    "test_folder_owner"
                ).value;


            const result =
                await apiRequest(
                    `/api/folders/${ownerId}/folder`
                );


            showResult(result);
        }
    );


// ========================================
// CREATE FOLDER
// ========================================

document
    .getElementById("create_folder_test_button")
    .addEventListener(
        "click",
        async function () {
            const title =
                document.getElementById(
                    "test_folder_title"
                ).value;


            const ownerId =
                document.getElementById(
                    "test_folder_owner"
                ).value;


            const parentFolderId =
                document.getElementById(
                    "test_parent_folder"
                ).value;


            const request =
            {
                title: title,

                ownerId: ownerId,

                parentFolderId:
                    parentFolderId === ""
                        ? null
                        : parentFolderId
            };


            const result =
                await apiRequest(
                    "/api/folders",
                    {
                        method: "POST",

                        headers:
                        {
                            "Content-Type":
                                "application/json"
                        },

                        body:
                            JSON.stringify(request)
                    }
                );


            showResult(result);
        }
    );


// ========================================
// GET FOLDER
// ========================================

document
    .getElementById("get_folder_button")
    .addEventListener(
        "click",
        async function () {
            const id =
                document.getElementById(
                    "test_folder_id"
                ).value;


            const result =
                await apiRequest(
                    `/api/folders/${id}`
                );


            showResult(result);
        }
    );


// ========================================
// DELETE FOLDER
// ========================================

document
    .getElementById("delete_folder_button")
    .addEventListener(
        "click",
        async function () {
            const id =
                document.getElementById(
                    "test_folder_id"
                ).value;


            const result =
                await apiRequest(
                    `/api/folders/${id}`,
                    {
                        method: "DELETE"
                    }
                );


            showResult(result);
        }
    );


// ========================================
// GET OWNER NOTES
// ========================================

document
    .getElementById("get_owner_notes_button")
    .addEventListener(
        "click",
        async function () {
            const ownerId =
                document.getElementById(
                    "test_note_owner"
                ).value;


            const result =
                await apiRequest(
                    `/api/notes/${ownerId}/owner`
                );


            showResult(result);
        }
    );


// ========================================
// CREATE NOTE
// ========================================

document
    .getElementById("create_note_test_button")
    .addEventListener(
        "click",
        async function () {
            const title =
                document.getElementById(
                    "test_note_title"
                ).value;


            const content =
                document.getElementById(
                    "test_note_content"
                ).value;


            const ownerId =
                document.getElementById(
                    "test_note_owner"
                ).value;


            const folderId =
                document.getElementById(
                    "test_note_folder"
                ).value;


            const noteType =
                document.getElementById(
                    "test_note_type"
                ).value;


            const request =
            {
                title: title,

                content: content,

                ownerId: ownerId,

                folderId:
                    folderId === ""
                        ? null
                        : folderId,

                isFavorite: false,

                isArchive: false,

                noteType: noteType
            };


            const result =
                await apiRequest(
                    "/api/notes",
                    {
                        method: "POST",

                        headers:
                        {
                            "Content-Type":
                                "application/json"
                        },

                        body:
                            JSON.stringify(request)
                    }
                );


            showResult(result);
        }
    );


// ========================================
// GET NOTE
// ========================================

document
    .getElementById("get_note_button")
    .addEventListener(
        "click",
        async function () {
            const id =
                document.getElementById(
                    "test_note_id"
                ).value;


            const result =
                await apiRequest(
                    `/api/notes/${id}`
                );


            showResult(result);
        }
    );


// ========================================
// FAVORITE
// ========================================

document
    .getElementById("favorite_note_button")
    .addEventListener(
        "click",
        async function () {
            const id =
                document.getElementById(
                    "test_note_id"
                ).value;


            const result =
                await apiRequest(
                    `/api/notes/${id}/favorite`,
                    {
                        method: "POST"
                    }
                );


            showResult(result);
        }
    );


// ========================================
// ARCHIVE
// ========================================

document
    .getElementById("archive_note_button")
    .addEventListener(
        "click",
        async function () {
            const id =
                document.getElementById(
                    "test_note_id"
                ).value;


            const result =
                await apiRequest(
                    `/api/notes/${id}/archive`,
                    {
                        method: "POST"
                    }
                );


            showResult(result);
        }
    );


// ========================================
// RESTORE
// ========================================

document
    .getElementById("restore_note_button")
    .addEventListener(
        "click",
        async function () {
            const id =
                document.getElementById(
                    "test_note_id"
                ).value;


            const result =
                await apiRequest(
                    `/api/notes/${id}/restore`,
                    {
                        method: "POST"
                    }
                );


            showResult(result);
        }
    );


// ========================================
// DELETE NOTE
// ========================================

document
    .getElementById("delete_note_button")
    .addEventListener(
        "click",
        async function () {
            const id =
                document.getElementById(
                    "test_note_id"
                ).value;


            const result =
                await apiRequest(
                    `/api/notes/${id}`,
                    {
                        method: "DELETE"
                    }
                );


            showResult(result);
        }
    );