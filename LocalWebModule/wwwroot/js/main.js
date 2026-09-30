return { ok: response.ok, status: response.status, data: data };

function showResult(p_id, result)
{
    const element = document.getElementById(p_id);
    const statusLine = `Статус:${result.status} | КОД_СТАТУСА:${result.ok}\n\n`;
    const bodyLine = JSON.stringify(result.data);

    element.textContent = statusLine + bodyLine;
}

//Единый метод , который отобразит на странице полученный RETURN

//N методы , которые соберут с <input>ЧТО-ТО ТАМ</input>
//Собрали -> вызвали из api.js N метод и полученные данные отобразили,
//через единый метод

async function run_Login() //Метод к которому обращается кнопка.
{
    // Вариант 1
    // let result;
    // const user_login = val("id-инпута");
    // const user_password = val("id-инпута");

    // result = await Login(user_login, user_password);
    //Вывод данных в КАКУЮ-ТО секцию

    //Варинт 2
    const result = await Login(val("id-инпута"), val("id-инпута"));
    showResult(result);
}

