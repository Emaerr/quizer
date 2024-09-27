let timeLeft = 15
let timerInterval;
let userAnswer = null;

function startTimer() {
    timerInterval = setInterval(function() {
        timeLeft--;
        document.getElementById("countdown").textContent = timeLeft;

        if (timeLeft <= 0) {
            clearInterval(timerInterval);
            fetchNextQuestion();
        }
    }, 1000);
}

function fetchNextQuestion() {
    fetch('/next-question', {  // эндпоинт сервера указать надо будет
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        },
    })
    .then(response => response.json())
    .then(data => {
        console.log('Данные получены:', data);
        loadNextQuestion(data);
    })
    .catch((error) => {
        console.error('Ошибка при получении данных:', error);
    });
}

function loadNextQuestion(data) {
    document.getElementById("questionText").textContent = data.question;
    const buttons = document.querySelectorAll(".option");
    buttons.forEach((button, index) => {
        button.textContent = data.answers[index];
        button.setAttribute("data-answer", data.answers[index]);
    });
}

Array.prototype.forEach.call(document.getElementsByClassName("option"), function(el) {
    el.addEventListener('click', function(e) {
        alert("Ответ принят");
    });
});