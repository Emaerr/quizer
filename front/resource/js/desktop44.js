let timeLeft = 15;
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
    fetch('http://localhost:3000/questions', {  // эндпоинт сервера указать надо будет
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        },
    })
    .then(response => response.json())
    .then(data => {
        console.log('Данные получены:', data);
        const nextQuestion = Array.isArray(data) ? data[0] : data;
        loadNextQuestion(nextQuestion);
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
        console.log(buttons)
    });
    resetTimer();
}

Array.prototype.forEach.call(document.getElementsByClassName("option"), function(el) {
    el.addEventListener('click', function(e) {
        const userAnswer = this.getAttribute("data-answer");
        alert("Ответ принят");
        fetchNextQuestion();
    });
});

function resetTimer() {
    timeLeft = 15
    document.getElementById('countdown').textContent = timeLeft;
    startTimer();
}
