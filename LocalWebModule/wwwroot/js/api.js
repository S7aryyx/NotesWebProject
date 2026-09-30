//wwwroot -> Папка js
//Файл : api.js

//Тут будут лежать обращения к user.js , note.js , folder.js
//
//Прописать единый метод для работы с section

const USER_URL = "https://localhost:7080/api/auth";
const NOTE_URL = "https://localhost:7080/api/notes";
const FODLER_URL = "https://localhost:7080/api/folders";

//url v1. USER_URL + url {/email/{email}}
//url v2. ПОЛНАЯ ССЫЛКА , которая будет собираться прямо в запросе (в методе) N api.

async function call_api(url, options = {})
{
    const response = await fetch(url, options);
    let data;
    try
    {
        //.text , который применяется к запросу ->
        //обработает и сохранит и УСПЕШНЫЙ ответ и ОШИБОЧНЫЙ(который C# подставляет сам).
        // PS .json обрабатывает только УСПЕШНЫЙ ответ
        const text = await response.text();
        data = text ? JSON.parse(text) : null;
        //вставляем в DATA переменную TEXT , только если она ЗАПОЛНЕНА, в противном случае NULL

        return { ok: response.ok, status: response.status, data: data };
    }
    catch (error)
    {
        data = JSON.parse(text);
        return { ok: false, status: 0, data: "Ошибка:" + error.message };
    }
}
//Название API
//Поля ввода
//Кнопка
//Окно с ответом от API = {НОМЕР_СТАТУСА, Статус , Сообщение}

//-=== User Functions ===-
async function Login(login, password)
{
   let data = call_api(`${USER_URL}/login`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.parse(login, password) });
   return data;
} 

async function Register(email,login,password)
{
    let data = call_api(`${USER_URL}/register`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.parse(login, password , email) });
    return data;
}

async function GetAllUsers() { }
async function GetUserById() { }




//-=== Note Functions ===-



//-=== Fodler Functions ===-