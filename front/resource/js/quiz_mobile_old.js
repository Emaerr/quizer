let score = 0; // Переменная для хранения текущего счета
let currentQuestionIndex = 0; // Индекс текущего вопроса

// Массив вопросов и ответов
const questions = [
    {
        question: 'Какое животное можно увидеть на логотипе Porsche?',
        image: '../resource/src/porsche.png',
        answers: ['Чёрный конь', 'Ослик', 'Красный конь', 'Не знаю'],
        correctAnswer: 'Чёрный конь'
        
    },
    {
        question: 'Какой язык чаще всего используется для создания серверных приложений?',
        answers: ['Java', 'C#', 'Node.js', 'PHP'],
        correctAnswer: 'Node.js',

    },
    {
        question: 'Какой язык является стандартом для стилей веб-страниц?',
        answers: ['HTML', 'CSS', 'JavaScript', 'SQL'],
        correctAnswer: 'CSS'

    }
];

// Функция проверки ответа
function checkAnswer(selectedAnswer) {
    const result = document.getElementById('result');
    const scoreElement = document.getElementById('score');
    const currentQuestion = questions[currentQuestionIndex];

    // Проверяем выбранный ответ
    if (selectedAnswer === currentQuestion.correctAnswer) {
        result.textContent = 'Правильно!';
        result.style.color = 'green';
        score += 10; // Добавляем баллы за правильный ответ
        currentQuestionIndex++; // Переходим к следующему вопросу
        if (currentQuestionIndex < questions.length) {
            loadQuestion(); // Загружаем следующий вопрос
        } else {
            result.textContent = 'Викторина завершена! Ваш итоговый счет: ' + score;
        }
    } else {
        result.textContent = 'Неправильно, попробуйте еще раз.';
        result.style.color = 'red';
        score -= 5; // Снимаем баллы за неправильный ответ
    }

    // Обновляем отображение счета
    scoreElement.textContent = score;
}

// Функция загрузки текущего вопроса
function loadQuestion() {
    const currentQuestion = questions[currentQuestionIndex];
    const questionElement = document.getElementById('question');
    const answersElement = document.getElementById('answers');
    const imageElement = document.getElementById('question-image');

    // Обновляем текст вопроса
    questionElement.textContent = currentQuestion.question;

    // Проверяем наличие изображения и обновляем его
    if (currentQuestion.image) {
        imageElement.src = currentQuestion.image;
        imageElement.style.display = 'block';
    } else {
        imageElement.style.display = 'none'; // Скрываем изображение, если его нет
    }

    // Обновляем варианты ответов
    answersElement.innerHTML = ''; // Очищаем предыдущие ответы
    currentQuestion.answers.forEach(answer => {
        const li = document.createElement('li');
        li.textContent = answer;
        li.onclick = () => checkAnswer(answer);
        answersElement.appendChild(li);
    });
}